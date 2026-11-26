using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;

namespace NETworkManager.Models.Cloudflare;

/// <summary>
/// Runs a network speed test against <c>speed.cloudflare.com</c>, modeled
/// after the official <a href="https://github.com/cloudflare/speedtest">cloudflare/speedtest</a>
/// JavaScript library. The full network logic is reimplemented in C#; no
/// telemetry is sent to <c>aim.cloudflare.com</c>.
/// </summary>
public class SpeedTestService
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(SpeedTestService));

    private const string BaseUrl = "https://speed.cloudflare.com";
    private const string Origin = "https://speed.cloudflare.com";

    /// <summary>
    /// Server time if the response has no <c>server-timing</c> header (<c>estimatedServerTime</c> of the official library).
    /// </summary>
    private const double DefaultEstimatedServerTimeMs = 0.0;
    private const double BandwidthFinishRequestDurationMs = 1000.0;
    private const double BandwidthMinRequestDurationMs = 10.0;
    private const double EstimatedHeaderFraction = 1.005;

    /// <summary>
    /// Minimum gap between mid-stream live throughput emissions, in ms.
    /// </summary>
    private const double LiveProgressIntervalMs = 250.0;

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

    private static readonly HttpClient _client = CreateClient();

    private static HttpClient CreateClient()
    {
        var client = new HttpClient { Timeout = Timeout.InfiniteTimeSpan };
        client.DefaultRequestHeaders.Add("Origin", Origin);
        client.DefaultRequestHeaders.UserAgent.ParseAdd("NETworkManager");
        return client;
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
            DownloadMbps = downloadBps.Count > 0 ? SpeedTestCalculator.Percentile(downloadBps, 0.9) / 1_000_000.0 : null,
            UploadMbps   = uploadBps.Count   > 0 ? SpeedTestCalculator.Percentile(uploadBps,   0.9) / 1_000_000.0 : null,
            LatencyMs    = pings.Count        > 0 ? SpeedTestCalculator.Percentile(pings, 0.5)                     : null,
            JitterMs     = pings.Count       >= 2 ? SpeedTestCalculator.Jitter(pings)                              : null,
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
            var p90Bps = downloadBps.Count > 0 ? SpeedTestCalculator.Percentile(downloadBps, 0.9) : 0.0;
            if (liveDownloadBpsOverride.HasValue && liveDownloadBpsOverride.Value > p90Bps)
                p90Bps = liveDownloadBpsOverride.Value;
            downloadMbps = p90Bps / 1_000_000.0;
        }

        double? uploadMbps = null;
        if (uploadBps.Count > 0 || liveUploadBpsOverride.HasValue)
        {
            var p90Bps = uploadBps.Count > 0 ? SpeedTestCalculator.Percentile(uploadBps, 0.9) : 0.0;
            if (liveUploadBpsOverride.HasValue && liveUploadBpsOverride.Value > p90Bps)
                p90Bps = liveUploadBpsOverride.Value;
            uploadMbps = p90Bps / 1_000_000.0;
        }

        double? latencyMs = pings.Count > 0 ? SpeedTestCalculator.Percentile(pings, 0.5) : null;
        double? jitterMs = pings.Count >= 2 ? SpeedTestCalculator.Jitter(pings) : null;

        var newDownloadSampleMbps = newDownloadSampleBps / 1_000_000.0;
        var newUploadSampleMbps = newUploadSampleBps / 1_000_000.0;

        progress.Report(new SpeedTestProgress(phase, downloadMbps, uploadMbps, latencyMs, jitterMs,
            newDownloadSampleMbps, newUploadSampleMbps));
    }

    #region Measurement primitives

    private static double? ParseServerTiming(HttpResponseHeaders headers) =>
        headers.TryGetValues("server-timing", out var values) ? SpeedTestCalculator.ParseServerTiming(values) : null;

    /// <summary>
    /// Requests the ISP and the Cloudflare server. Optional: an error is logged and the test continues without them.
    /// </summary>
    /// <returns>Metadata, <c>null</c> if the request failed.</returns>
    private async Task<SpeedTestMetaInfo> FetchMetaAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _client.GetAsync($"{BaseUrl}/meta", cancellationToken)
                .ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<SpeedTestMetaInfo>(json);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Log.Warn("Speed test metadata could not be requested.", ex);
            return null;
        }
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
        response.EnsureSuccessStatusCode();
        await response.Content.CopyToAsync(Stream.Null, cancellationToken).ConfigureAwait(false);

        var serverTime = ParseServerTiming(response.Headers) ?? DefaultEstimatedServerTimeMs;
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
        response.EnsureSuccessStatusCode();
        var serverTime = ParseServerTiming(response.Headers) ?? DefaultEstimatedServerTimeMs;
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
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/__up");
        using var content = new ZeroStreamContent(bytes);
        request.Content = content;

        var sw = Stopwatch.StartNew();
        using var response = await _client
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);
        sw.Stop();
        var ttfb = sw.Elapsed.TotalMilliseconds;
        response.EnsureSuccessStatusCode();
        await response.Content.CopyToAsync(Stream.Null, cancellationToken).ConfigureAwait(false);

        var uploadDurationMs = ttfb;
        if (uploadDurationMs <= 0)
            return (0.0, uploadDurationMs);

        var bps = 8.0 * bytes * EstimatedHeaderFraction / (uploadDurationMs / 1000.0);
        return (bps, uploadDurationMs);
    }

    #endregion

    /// <summary>
    /// Streams <paramref name="length"/> zero bytes as HTTP content without allocating
    /// a full-sized buffer. A single small shared chunk is reused across all writes.
    /// </summary>
    private sealed class ZeroStreamContent : HttpContent
    {
        private readonly int _length;
        private static readonly byte[] _chunk = new byte[81920];

        internal ZeroStreamContent(int length)
        {
            _length = length;
            Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            Headers.ContentLength = length;
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
            SerializeToStreamAsync(stream, context, CancellationToken.None);

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context,
            CancellationToken cancellationToken)
        {
            var remaining = _length;
            while (remaining > 0)
            {
                var toWrite = Math.Min(_chunk.Length, remaining);
                await stream.WriteAsync(_chunk.AsMemory(0, toWrite), cancellationToken).ConfigureAwait(false);
                remaining -= toWrite;
            }
        }

        protected override bool TryComputeLength(out long length)
        {
            length = _length;
            return true;
        }
    }
}
