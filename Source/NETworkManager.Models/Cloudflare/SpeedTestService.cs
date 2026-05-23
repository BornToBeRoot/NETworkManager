using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NETworkManager.Models.Cloudflare;

/// <summary>
/// Runs a network speed test against <c>speed.cloudflare.com</c>, modeled
/// after the official <a href="https://github.com/cloudflare/speedtest">cloudflare/speedtest</a>
/// JavaScript library. The full network logic is reimplemented in C#; no
/// telemetry is sent to <c>aim.cloudflare.com</c>.
/// </summary>
public class SpeedTestService : IDisposable
{
    private const string BaseUrl = "https://speed.cloudflare.com";
    private const string Origin = "https://speed.cloudflare.com";

    private const double DefaultEstimatedServerTimeMs = 10.0;
    private const double BandwidthFinishRequestDurationMs = 1000.0;
    private const double BandwidthMinRequestDurationMs = 10.0;
    private const double EstimatedHeaderFraction = 1.005;

    /// <summary>
    /// Minimum gap between mid-stream live throughput emissions, in ms.
    /// </summary>
    private const double LiveProgressIntervalMs = 250.0;

    private static readonly Regex ServerTimingRegex = new(@"(?:^|;)\s*dur=([0-9.]+)", RegexOptions.Compiled);

    private enum StepDirection { Download, Upload }

    /// <summary>
    /// Interleaved download/upload sequence from <c>defaultConfig.js</c> of the
    /// official cloudflare/speedtest library. The initial 100 KB download
    /// (bypassMinDuration) is executed separately between the two latency phases.
    /// </summary>
    private static readonly (StepDirection Direction, int Bytes, int Count)[] Steps =
    {
        (StepDirection.Download, 100_000,     9),
        (StepDirection.Download, 1_000_000,   8),
        (StepDirection.Upload,   100_000,     8),
        (StepDirection.Upload,   1_000_000,   6),
        (StepDirection.Download, 10_000_000,  6),
        (StepDirection.Upload,   10_000_000,  4),
        (StepDirection.Download, 25_000_000,  4),
        (StepDirection.Upload,   25_000_000,  4),
        (StepDirection.Download, 100_000_000, 3),
        (StepDirection.Upload,   50_000_000,  3),
        (StepDirection.Download, 250_000_000, 2),
    };

    private const int LatencyInitialProbes = 1;
    private const int LatencyMeasurementProbes = 20;

    private readonly HttpClient _client;

    public SpeedTestService()
    {
        _client = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
        _client.DefaultRequestHeaders.Add("Origin", Origin);
        _client.DefaultRequestHeaders.UserAgent.ParseAdd("NETworkManager");
    }

    /// <summary>
    /// Runs a full speed test sequence (metadata, latency, download, upload).
    /// Live estimates for each metric are reported via <paramref name="progress"/>
    /// as samples accumulate. The returned <see cref="SpeedTestResult"/>
    /// contains the final aggregated values.
    /// </summary>
    public async Task<SpeedTestResult> RunAsync(IProgress<SpeedTestProgress> progress, CancellationToken cancellationToken)
    {
        var pings = new List<double>();
        var downloadBps = new List<double>();
        var uploadBps = new List<double>();

        // 1. Metadata
        Emit(progress, SpeedTestPhase.FetchingMetadata, pings, downloadBps, uploadBps, null, null);
        var meta = await FetchMetaAsync(cancellationToken).ConfigureAwait(false);
        progress?.Report(new SpeedTestProgress(SpeedTestPhase.FetchingMetadata, Meta: meta));
        
        // 2. Initial latency probe (server-time estimation)
        Emit(progress, SpeedTestPhase.MeasuringLatency, pings, downloadBps, uploadBps, null, null);
        for (var i = 0; i < LatencyInitialProbes; i++)
        {
            await MeasureLatencyAsync(pings, cancellationToken).ConfigureAwait(false);
            Emit(progress, SpeedTestPhase.MeasuringLatency, pings, downloadBps, uploadBps, null, null);
        }

        // 3. Initial download estimate (100 KB × 1 — bypassMinDuration, between the two latency phases)
        Emit(progress, SpeedTestPhase.MeasuringDownload, pings, downloadBps, uploadBps, null, null);
        {
            var (bps, durationMs) = await MeasureDownloadAsync(100_000, cancellationToken,
                liveBps => Emit(progress, SpeedTestPhase.MeasuringDownload,
                    pings, downloadBps, uploadBps, liveBps, null)).ConfigureAwait(false);
            if (durationMs >= BandwidthMinRequestDurationMs)
            {
                downloadBps.Add(bps);
                Emit(progress, SpeedTestPhase.MeasuringDownload,
                    pings, downloadBps, uploadBps, null, null,
                    newDownloadSampleBps: bps);
            }
        }

        // 4. Proper unloaded latency measurement (20 probes)
        Emit(progress, SpeedTestPhase.MeasuringLatency, pings, downloadBps, uploadBps, null, null);
        for (var i = 0; i < LatencyMeasurementProbes; i++)
        {
            await MeasureLatencyAsync(pings, cancellationToken).ConfigureAwait(false);
            Emit(progress, SpeedTestPhase.MeasuringLatency, pings, downloadBps, uploadBps, null, null);
        }
        
        // 5. Interleaved download / upload (official cloudflare/speedtest sequence)
        var downloadStopped = false;
        var uploadStopped = false;
        var currentPhase = SpeedTestPhase.MeasuringDownload;
        Emit(progress, currentPhase, pings, downloadBps, uploadBps, null, null);

        foreach (var step in Steps)
        {
            if (step.Direction == StepDirection.Download && downloadStopped) continue;
            if (step.Direction == StepDirection.Upload   && uploadStopped)   continue;

            var stepPhase = step.Direction == StepDirection.Download
                ? SpeedTestPhase.MeasuringDownload
                : SpeedTestPhase.MeasuringUpload;
            if (stepPhase != currentPhase)
            {
                currentPhase = stepPhase;
                Emit(progress, currentPhase, pings, downloadBps, uploadBps, null, null);
            }

            for (var i = 0; i < step.Count; i++)
            {
                if (step.Direction == StepDirection.Download)
                {

                    var phaseSnapshot = currentPhase;
                    var (bps, durationMs) = await MeasureDownloadAsync(step.Bytes, cancellationToken,
                        liveBps => Emit(progress, phaseSnapshot,
                            pings, downloadBps, uploadBps, liveBps, null)).ConfigureAwait(false);
                    if (durationMs >= BandwidthMinRequestDurationMs)
                    {
                        downloadBps.Add(bps);
                        Emit(progress, currentPhase,
                            pings, downloadBps, uploadBps, null, null,
                            newDownloadSampleBps: bps);
                    }
                    if (durationMs >= BandwidthFinishRequestDurationMs)
                    {
                        downloadStopped = true;
                        break;
                    }
                }
                else
                {
                    var (bps, durationMs) = await MeasureUploadAsync(step.Bytes, cancellationToken)
                        .ConfigureAwait(false);
                    if (durationMs >= BandwidthMinRequestDurationMs)
                    {
                        uploadBps.Add(bps);
                        Emit(progress, currentPhase,
                            pings, downloadBps, uploadBps, null, null,
                            newUploadSampleBps: bps);
                    }
                    if (durationMs >= BandwidthFinishRequestDurationMs)
                    {
                        uploadStopped = true;
                        break;
                    }
                }
            }
        }

        // 6. Result
        return new SpeedTestResult
        {
            DownloadMbps = downloadBps.Count > 0 ? Percentile(downloadBps, 0.9) / 1_000_000.0 : null,
            UploadMbps   = uploadBps.Count   > 0 ? Percentile(uploadBps,   0.9) / 1_000_000.0 : null,
            LatencyMs    = pings.Count        > 0 ? Percentile(pings, 0.5)                     : null,
            JitterMs     = pings.Count       >= 2 ? Jitter(pings)                              : null,
            Isp = meta?.AsOrganization,
            ClientCity = meta?.City,
            ClientCountry = meta?.Country,
            ServerCity = meta?.Colo?.City,
            ServerCountry = meta?.Colo?.Cca2,
            ServerIata = meta?.Colo?.Iata
        };
    }

    /// <summary>
    /// Builds and emits a <see cref="SpeedTestProgress"/> using current
    /// sample lists. <paramref name="liveDownloadBpsOverride"/> and
    /// <paramref name="liveUploadBpsOverride"/> let mid-stream callers
    /// report an instantaneous estimate even before the first completed
    /// sample is in the list.
    /// </summary>
    private static void Emit(IProgress<SpeedTestProgress> progress, SpeedTestPhase phase,
        List<double> pings, List<double> downloadBps, List<double> uploadBps,
        double? liveDownloadBpsOverride, double? liveUploadBpsOverride,
        double? newDownloadSampleBps = null, double? newUploadSampleBps = null)
    {
        if (progress == null)
            return;

        // P90 of completed samples ⊔ current live instantaneous — consistent with
        // the final result formula; live override applies only before the first sample.
        double? downloadMbps = null;
        if (downloadBps.Count > 0 || liveDownloadBpsOverride.HasValue)
        {
            var p90Bps = downloadBps.Count > 0 ? Percentile(downloadBps, 0.9) : 0.0;
            if (liveDownloadBpsOverride.HasValue && liveDownloadBpsOverride.Value > p90Bps)
                p90Bps = liveDownloadBpsOverride.Value;
            downloadMbps = p90Bps / 1_000_000.0;
        }

        double? uploadMbps = null;
        if (uploadBps.Count > 0 || liveUploadBpsOverride.HasValue)
        {
            var p90Bps = uploadBps.Count > 0 ? Percentile(uploadBps, 0.9) : 0.0;
            if (liveUploadBpsOverride.HasValue && liveUploadBpsOverride.Value > p90Bps)
                p90Bps = liveUploadBpsOverride.Value;
            uploadMbps = p90Bps / 1_000_000.0;
        }

        double? latencyMs = pings.Count > 0 ? Percentile(pings, 0.5) : null;
        double? jitterMs = pings.Count >= 2 ? Jitter(pings) : null;

        var newDownloadSampleMbps = newDownloadSampleBps / 1_000_000.0;
        var newUploadSampleMbps = newUploadSampleBps / 1_000_000.0;

        progress.Report(new SpeedTestProgress(phase, downloadMbps, uploadMbps, latencyMs, jitterMs,
            newDownloadSampleMbps, newUploadSampleMbps));
    }

    #region Measurement primitives

    private async Task<SpeedTestMetaInfo> FetchMetaAsync(CancellationToken cancellationToken)
    {
        using var response = await _client.GetAsync($"{BaseUrl}/meta", cancellationToken)
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return JsonConvert.DeserializeObject<SpeedTestMetaInfo>(json);
    }

    private async Task MeasureLatencyAsync(List<double> pings, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/__down?bytes=0");
        using var response = await _client
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);
        sw.Stop();
        var ttfb = sw.Elapsed.TotalMilliseconds;
        await response.Content.CopyToAsync(Stream.Null, cancellationToken).ConfigureAwait(false);

        var serverTime = ParseServerTiming(response) ?? DefaultEstimatedServerTimeMs;
        var ping = ttfb - serverTime;
        if (ping < 0) ping = 0;
        pings.Add(ping);
    }

    private async Task<(double Bps, double DurationMs)> MeasureDownloadAsync(int bytes,
        CancellationToken cancellationToken, Action<double> onLiveBps)
    {
        var sw1 = Stopwatch.StartNew();
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/__down?bytes={bytes}");
        using var response = await _client
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);
        sw1.Stop();
        var ttfb = sw1.Elapsed.TotalMilliseconds;
        var serverTime = ParseServerTiming(response) ?? DefaultEstimatedServerTimeMs;
        var ping = ttfb - serverTime;
        if (ping < 0) ping = 0;

        var sw2 = Stopwatch.StartNew();
        long bodyBytes;
        double lastProgressMs = 0;
        long lastProgressBytes = 0;
        await using (var stream = await response.Content
                         .ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
        {
            var buffer = new byte[81920];
            long total = 0;
            int read;
            while ((read = await stream.ReadAsync(buffer, cancellationToken)
                       .ConfigureAwait(false)) > 0)
            {
                total += read;
                var elapsedMs = sw2.Elapsed.TotalMilliseconds;
                if (onLiveBps != null && elapsedMs - lastProgressMs >= LiveProgressIntervalMs
                                       && elapsedMs > 0)
                {
                    // Instantaneous throughput over the last interval (delta-based)
                    var deltaBytes = total - lastProgressBytes;
                    var deltaMs = elapsedMs - lastProgressMs;
                    var liveBps = 8.0 * deltaBytes / (deltaMs / 1000.0);
                    onLiveBps(liveBps);
                    lastProgressMs = elapsedMs;
                    lastProgressBytes = total;
                }
            }
            bodyBytes = total;
        }
        sw2.Stop();
        var bodyMs = sw2.Elapsed.TotalMilliseconds;
        var contentLength = response.Content.Headers.ContentLength ?? bodyBytes;

        var downloadDurationMs = ping + bodyMs;
        if (downloadDurationMs <= 0)
            return (0.0, downloadDurationMs);

        var bps = 8.0 * contentLength / (downloadDurationMs / 1000.0);
        return (bps, downloadDurationMs);
    }

    private async Task<(double Bps, double DurationMs)> MeasureUploadAsync(int bytes,
        CancellationToken cancellationToken)
    {
        var payload = new byte[bytes];
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/__up");
        using var content = new ByteArrayContent(payload);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        request.Content = content;

        var sw = Stopwatch.StartNew();
        using var response = await _client
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);
        sw.Stop();
        var ttfb = sw.Elapsed.TotalMilliseconds;
        await response.Content.CopyToAsync(Stream.Null, cancellationToken).ConfigureAwait(false);

        var uploadDurationMs = ttfb;
        if (uploadDurationMs <= 0)
            return (0.0, uploadDurationMs);

        var bps = 8.0 * bytes * EstimatedHeaderFraction / (uploadDurationMs / 1000.0);
        return (bps, uploadDurationMs);
    }

    #endregion

    #region Helpers

    private static double? ParseServerTiming(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("server-timing", out var values))
            return null;
        foreach (var value in values)
        {
            var match = ServerTimingRegex.Match(value);
            if (match.Success && double.TryParse(match.Groups[1].Value,
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var dur))
                return dur;
        }
        return null;
    }

    private static double Percentile(List<double> values, double p)
    {
        if (values == null || values.Count == 0)
            return 0.0;
        var sorted = values.OrderBy(v => v).ToList();
        if (sorted.Count == 1)
            return sorted[0];
        var rank = p * (sorted.Count - 1);
        var lower = (int)Math.Floor(rank);
        var upper = (int)Math.Ceiling(rank);
        if (lower == upper)
            return sorted[lower];
        var weight = rank - lower;
        return sorted[lower] * (1 - weight) + sorted[upper] * weight;
    }

    private static double Jitter(List<double> pings)
    {
        if (pings == null || pings.Count < 2)
            return 0.0;
        double sum = 0;
        for (var i = 1; i < pings.Count; i++)
            sum += Math.Abs(pings[i] - pings[i - 1]);
        return sum / (pings.Count - 1);
    }

    #endregion

    public void Dispose()
    {
        _client?.Dispose();
        GC.SuppressFinalize(this);
    }
}
