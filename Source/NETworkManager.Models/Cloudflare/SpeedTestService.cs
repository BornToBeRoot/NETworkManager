using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.ExceptionServices;
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

    /// <summary>
    /// Host of the speed test.
    /// </summary>
    public const string Host = "speed.cloudflare.com";

    private const string BaseUrl = "https://" + Host;
    private const string Origin = "https://speed.cloudflare.com";

    /// <summary>
    /// Server time if the response has no <c>server-timing</c> header (<c>estimatedServerTime</c> of the official library).
    /// </summary>
    private const double DefaultEstimatedServerTimeMs = 0.0;
    private const double BandwidthFinishRequestDurationMs = 1000.0;
    private const double BandwidthMinRequestDurationMs = 10.0;
    /// <summary>
    /// Bytes on the wire per payload byte (HTTP headers), like the official library.
    /// </summary>
    private const double EstimatedHeaderFraction = 1.005;

    /// <summary>
    /// A request fails if nothing happens for this time (no response, no data received or sent). Not a limit of the
    /// whole request: a large download / upload on a slow connection takes longer.
    /// </summary>
    public static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(15);

    /// <summary>
    /// A request that fails with HTTP 429 (too many requests) is repeated up to this number of times after the delay of
    /// the server (<c>Retry-After</c>), like the official library.
    /// </summary>
    private const int MaxRetries = 3;

    /// <summary>
    /// A failed request is skipped and the test continues (like the official library). After this number of requests in
    /// a row without a connection (no response, e.g. the connection is lost), the test ends with the error. Requests
    /// rejected by the server / a proxy (HTTP error) don't count: the connection works.
    /// </summary>
    private const int MaxConsecutiveFailedRequests = 3;

    /// <summary>
    /// Minimum gap between mid-stream live throughput emissions, in ms.
    /// </summary>
    private const double LiveProgressIntervalMs = 250.0;

    private enum MeasurementType
    {
        Latency,
        Download,
        Upload
    }

    /// <summary>
    /// A set of requests of the same type and size.
    /// </summary>
    /// <param name="Type">Latency probes (empty download), download or upload.</param>
    /// <param name="Count">Number of requests.</param>
    /// <param name="Bytes">Size of the download / upload.</param>
    /// <param name="BypassMinDuration">The set doesn't decide whether the direction is finished.</param>
    private record Measurement(MeasurementType Type, int Count, int Bytes = 0, bool BypassMinDuration = false);

    /// <summary>
    /// Measurements of <c>defaultConfig</c> of the official cloudflare/speedtest library, in this order. A direction
    /// (download / upload) is finished after a set whose fastest request took longer than
    /// <see cref="BandwidthFinishRequestDurationMs" />: its larger sets are skipped, the latency probes run anyway.
    /// </summary>
    private static readonly Measurement[] Measurements =
    [
        new(MeasurementType.Latency, 2),
        new(MeasurementType.Download, 1, 100_000, BypassMinDuration: true),
        new(MeasurementType.Latency, 20),
        new(MeasurementType.Download, 9, 100_000),
        new(MeasurementType.Latency, 2),
        new(MeasurementType.Download, 8, 1_000_000),
        new(MeasurementType.Latency, 2),
        new(MeasurementType.Upload, 8, 100_000),
        new(MeasurementType.Latency, 2),
        new(MeasurementType.Upload, 6, 1_000_000),
        new(MeasurementType.Latency, 2),
        new(MeasurementType.Download, 6, 10_000_000),
        new(MeasurementType.Latency, 2),
        new(MeasurementType.Upload, 4, 10_000_000),
        new(MeasurementType.Latency, 2),
        new(MeasurementType.Download, 4, 25_000_000),
        new(MeasurementType.Latency, 2),
        new(MeasurementType.Upload, 4, 25_000_000),
        new(MeasurementType.Latency, 2),
        new(MeasurementType.Download, 3, 100_000_000),
        new(MeasurementType.Latency, 2),
        new(MeasurementType.Upload, 3, 50_000_000),
        new(MeasurementType.Latency, 2),
        new(MeasurementType.Download, 2, 250_000_000)
    ];

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
        Emit(progress, SpeedTestPhase.FetchingMetadata, pings, downloadBps, uploadBps, null);
        var meta = await FetchMetaAsync(cancellationToken).ConfigureAwait(false);
        progress?.Report(new SpeedTestProgress(SpeedTestPhase.FetchingMetadata, Meta: meta));
        
        // 2. Latency, download and upload (official cloudflare/speedtest sequence)
        var failures = new RequestFailures();
        var finished = new HashSet<MeasurementType>();

        // Duration of all requests per direction and size, decides whether a direction is finished
        var durations = new Dictionary<(MeasurementType, int), List<double>>();

        // The latency probes between the download / upload sets are short: keep showing the download / upload
        var bandwidthStarted = false;
        var currentPhase = SpeedTestPhase.FetchingMetadata;

        foreach (var measurement in Measurements)
        {
            if (measurement.Type != MeasurementType.Latency && finished.Contains(measurement.Type))
                continue;

            if (measurement.Type != MeasurementType.Latency && !measurement.BypassMinDuration)
                bandwidthStarted = true;

            var phase = measurement.Type switch
            {
                MeasurementType.Download => SpeedTestPhase.MeasuringDownload,
                MeasurementType.Upload => SpeedTestPhase.MeasuringUpload,
                _ => bandwidthStarted ? currentPhase : SpeedTestPhase.MeasuringLatency
            };

            if (phase != currentPhase)
            {
                currentPhase = phase;
                Emit(progress, currentPhase, pings, downloadBps, uploadBps, null);
            }

            if (measurement.Type == MeasurementType.Latency)
            {
                for (var i = 0; i < measurement.Count; i++)
                {
                    await TryRequestAsync(() => MeasureLatencyAsync(pings, cancellationToken), failures,
                        cancellationToken).ConfigureAwait(false);
                    Emit(progress, currentPhase, pings, downloadBps, uploadBps, null);
                }

                continue;
            }

            var isDownload = measurement.Type == MeasurementType.Download;

            if (!durations.TryGetValue((measurement.Type, measurement.Bytes), out var setDurations))
            {
                setDurations = [];
                durations[(measurement.Type, measurement.Bytes)] = setDurations;
            }

            var setSucceeded = false;
            var setSizeFailed = false;

            for (var i = 0; i < measurement.Count; i++)
            {
                var (success, sizeFailed, (bps, durationMs)) = await TryRequestAsync(() => isDownload
                    ? MeasureDownloadAsync(measurement.Bytes, cancellationToken,
                        liveBps => Emit(progress, SpeedTestPhase.MeasuringDownload,
                            pings, downloadBps, uploadBps, liveBps))
                    : MeasureUploadAsync(measurement.Bytes, cancellationToken), failures, cancellationToken,
                    () => IsServerReachableAsync(cancellationToken)).ConfigureAwait(false);

                // The server answers small requests, but requests of this size fail without a response: the remaining
                // requests of the set would each wait for the timeout too
                if (sizeFailed)
                {
                    setSizeFailed = true;
                    break;
                }

                if (!success)
                    continue;

                setSucceeded = true;

                // Requests without a speed are discarded (like the official library)
                if (bps <= 0 || double.IsInfinity(bps))
                    continue;

                setDurations.Add(durationMs);

                // Very short requests are too inaccurate for the result
                if (durationMs < BandwidthMinRequestDurationMs)
                    continue;

                if (isDownload)
                {
                    downloadBps.Add(bps);
                    Emit(progress, currentPhase, pings, downloadBps, uploadBps, null,
                        newDownloadSampleBps: bps);
                }
                else
                {
                    uploadBps.Add(bps);
                    Emit(progress, currentPhase, pings, downloadBps, uploadBps, null,
                        newUploadSampleBps: bps);
                }
            }

            if (measurement.BypassMinDuration)
                continue;

            // All requests of the set failed (e.g. a proxy rejects uploads of this size), or requests of this size fail
            // without a response: the larger sizes would fail too
            if (!setSucceeded || setSizeFailed ||
                SpeedTestCalculator.IsDirectionFinished(setDurations, BandwidthFinishRequestDurationMs))
                finished.Add(measurement.Type);
        }

        // Every request was rejected (e.g. a proxy answers HTTP 403): the error of the test, not an empty result with a
        // warning (requests without a response end the test earlier)
        if (pings.Count == 0 && downloadBps.Count == 0 && uploadBps.Count == 0 && failures.LastError != null)
            ExceptionDispatchInfo.Capture(failures.LastError).Throw();

        // 3. Result
        return new SpeedTestResult
        {
            DownloadMbps = downloadBps.Count > 0 ? SpeedTestCalculator.Percentile(downloadBps, 0.9) / 1_000_000.0 : null,
            UploadMbps   = uploadBps.Count   > 0 ? SpeedTestCalculator.Percentile(uploadBps,   0.9) / 1_000_000.0 : null,
            LatencyMs    = pings.Count        > 0 ? SpeedTestCalculator.Percentile(pings, 0.5)                     : null,
            JitterMs     = pings.Count       >= 2 ? SpeedTestCalculator.Jitter(pings)                              : null,
            Isp = meta?.AsOrganization,
            ServerCity = meta?.Colo?.City,
            ServerCountry = meta?.Colo?.Cca2,
            ServerIata = meta?.Colo?.Iata,
            FailedRequests = failures.Total
        };
    }

    /// <summary>
    /// Builds and emits a <see cref="SpeedTestProgress"/> using current
    /// sample lists. <paramref name="liveDownloadBpsOverride"/> lets the running
    /// download report an instantaneous estimate before the first completed
    /// sample is in the list (uploads only report completed samples).
    /// </summary>
    private static void Emit(IProgress<SpeedTestProgress> progress, SpeedTestPhase phase,
        List<double> pings, List<double> downloadBps, List<double> uploadBps,
        double? liveDownloadBpsOverride,
        double? newDownloadSampleBps = null, double? newUploadSampleBps = null)
    {
        if (progress == null)
            return;

        var downloadMbps = SpeedTestCalculator.CurrentBandwidth(downloadBps, liveDownloadBpsOverride) / 1_000_000.0;
        var uploadMbps = SpeedTestCalculator.CurrentBandwidth(uploadBps, null) / 1_000_000.0;

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
            return await WithTimeoutAsync(async timeout =>
            {
                using var response = await _client.GetAsync($"{BaseUrl}/meta", timeout.Token)
                    .ConfigureAwait(false);
                EnsureSuccess(response);
                var json = await response.Content.ReadAsStringAsync(timeout.Token).ConfigureAwait(false);
                return JsonConvert.DeserializeObject<SpeedTestMetaInfo>(json);
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Log.Warn("Speed test metadata could not be requested.", ex);
            return null;
        }
    }

    /// <summary>
    /// Runs a request that fails with a <see cref="TimeoutException" /> if nothing happens for
    /// <see cref="RequestTimeout" />. The request restarts the timeout (<c>CancelAfter</c>) whenever data is received or
    /// sent.
    /// </summary>
    private static async Task<T> WithTimeoutAsync<T>(Func<CancellationTokenSource, Task<T>> request,
        CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(RequestTimeout);

        try
        {
            return await request(timeout).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException(
                $"No response from {BaseUrl} within {RequestTimeout.TotalSeconds} seconds.");
        }
    }

    /// <summary>
    /// Failed requests of a test.
    /// </summary>
    private sealed class RequestFailures
    {
        public int Total { get; set; }
        public int Consecutive { get; set; }

        /// <summary>
        /// Error of the last failed request (thrown if no request of the test succeeded).
        /// </summary>
        public Exception LastError { get; set; }
    }

    /// <summary>
    /// The server asks to repeat the request later (HTTP 429).
    /// </summary>
    private sealed class TooManyRequestsException(TimeSpan retryAfter)
        : HttpRequestException("Too many requests.", null, HttpStatusCode.TooManyRequests)
    {
        public TimeSpan RetryAfter { get; } = retryAfter;
    }

    private static void EnsureSuccess(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.TooManyRequests)
            throw new TooManyRequestsException(SpeedTestCalculator.GetRetryDelay(response.Headers.RetryAfter?.Delta,
                response.Headers.RetryAfter?.Date, DateTimeOffset.UtcNow));

        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Runs a request of the test like the official library: HTTP 429 is repeated after the delay of the server (up to
    /// <see cref="MaxRetries" /> times), another error skips the request. After <see cref="MaxConsecutiveFailedRequests" />
    /// failed requests in a row without a response the error is thrown (the test ends) - unless the server still
    /// answers a small request: then only this size fails (e.g. a proxy that closes the connection of large uploads
    /// instead of answering HTTP 413), the rest of the set is skipped and the direction ends.
    /// </summary>
    /// <param name="isServerReachable">Checks whether the server answers (download / upload), <c>null</c> for the
    ///     latency probes (they are small requests themselves).</param>
    /// <returns><c>Success</c> <c>false</c> if the request failed and was skipped, <c>SizeFailed</c> <c>true</c> if
    ///     requests of this size fail while the server answers small requests (skip the rest of the set).</returns>
    private static async Task<(bool Success, bool SizeFailed, T Value)> TryRequestAsync<T>(Func<Task<T>> request,
        RequestFailures failures, CancellationToken cancellationToken, Func<Task<bool>> isServerReachable = null)
    {
        for (var retry = 0;; retry++)
        {
            try
            {
                var value = await request().ConfigureAwait(false);
                failures.Consecutive = 0;

                return (true, false, value);
            }
            catch (TooManyRequestsException ex) when (retry < MaxRetries)
            {
                Log.Warn($"Speed test request rate limited (HTTP 429), retry in {ex.RetryAfter.TotalSeconds} s.");
                await Task.Delay(ex.RetryAfter, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is HttpRequestException or IOException or TimeoutException)
            {
                // Canceled by the user while data was transferred: the transport may report it as an IOException
                cancellationToken.ThrowIfCancellationRequested();

                failures.Total++;
                failures.LastError = ex;

                if (!SpeedTestCalculator.IsConnectionFailure(ex))
                {
                    failures.Consecutive = 0;
                }
                else if (++failures.Consecutive >= MaxConsecutiveFailedRequests)
                {
                    if (isServerReachable == null || !await isServerReachable().ConfigureAwait(false))
                        throw;

                    // The connection works, only requests of this size fail: skip them (the set fails)
                    Log.Warn("Speed test requests of this size fail, but the server answers small requests.", ex);
                    failures.Consecutive = 0;

                    return (false, true, default);
                }

                Log.Warn("Speed test request failed and is skipped.", ex);

                return (false, false, default);
            }
        }
    }

    /// <summary>
    /// Checks whether the server answers a small request (an empty download, not measured).
    /// </summary>
    /// <returns><c>false</c> if there is no response (e.g. the connection was lost).</returns>
    private async Task<bool> IsServerReachableAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await WithTimeoutAsync(async timeout =>
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/__down?bytes=0");
                using var response = await _client
                    .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token)
                    .ConfigureAwait(false);

                return true;
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is HttpRequestException or IOException or TimeoutException)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return false;
        }
    }

    private Task<bool> MeasureLatencyAsync(List<double> pings, CancellationToken cancellationToken)
    {
        return WithTimeoutAsync(async timeout =>
        {
            var sw = Stopwatch.StartNew();
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/__down?bytes=0");
            using var response = await _client
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token)
                .ConfigureAwait(false);
            sw.Stop();
            var ttfb = sw.Elapsed.TotalMilliseconds;
            EnsureSuccess(response);
            await response.Content.CopyToAsync(Stream.Null, timeout.Token).ConfigureAwait(false);

            var serverTime = ParseServerTiming(response.Headers) ?? DefaultEstimatedServerTimeMs;
            var ping = ttfb - serverTime;
            if (ping < 0) ping = 0;
            pings.Add(ping);

            return true;
        }, cancellationToken);
    }

    private Task<(double Bps, double DurationMs)> MeasureDownloadAsync(int bytes,
        CancellationToken cancellationToken, Action<double> onLiveBps)
    {
        return WithTimeoutAsync(async timeout =>
        {
            var sw1 = Stopwatch.StartNew();
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/__down?bytes={bytes}");
            using var response = await _client
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token)
                .ConfigureAwait(false);
            sw1.Stop();
            var ttfb = sw1.Elapsed.TotalMilliseconds;
            EnsureSuccess(response);
            var serverTime = ParseServerTiming(response.Headers) ?? DefaultEstimatedServerTimeMs;
            var ping = ttfb - serverTime;
            if (ping < 0) ping = 0;

            var sw2 = Stopwatch.StartNew();
            long bodyBytes;
            double lastProgressMs = 0;
            long lastProgressBytes = 0;
            await using (var stream = await response.Content
                             .ReadAsStreamAsync(timeout.Token).ConfigureAwait(false))
            {
                var buffer = new byte[81920];
                long total = 0;
                int read;
                while ((read = await stream.ReadAsync(buffer, timeout.Token)
                           .ConfigureAwait(false)) > 0)
                {
                    total += read;
                    timeout.CancelAfter(RequestTimeout);
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

            var downloadDurationMs = ping + bodyMs;
            if (downloadDurationMs <= 0)
                return (0.0, downloadDurationMs);

            // Received bytes incl. an estimate of the headers (the official library uses the transfer size of the browser)
            var bps = 8.0 * bodyBytes * EstimatedHeaderFraction / (downloadDurationMs / 1000.0);
            return (bps, downloadDurationMs);
        }, cancellationToken);
    }

    private Task<(double Bps, double DurationMs)> MeasureUploadAsync(int bytes,
        CancellationToken cancellationToken)
    {
        return WithTimeoutAsync(async timeout =>
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/__up");
            using var content = new ZeroStreamContent(bytes, () => timeout.CancelAfter(RequestTimeout));
            request.Content = content;

            var sw = Stopwatch.StartNew();
            using var response = await _client
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token)
                .ConfigureAwait(false);
            sw.Stop();
            var ttfb = sw.Elapsed.TotalMilliseconds;
            EnsureSuccess(response);
            await response.Content.CopyToAsync(Stream.Null, timeout.Token).ConfigureAwait(false);

            var uploadDurationMs = ttfb;
            if (uploadDurationMs <= 0)
                return (0.0, uploadDurationMs);

            var bps = 8.0 * bytes * EstimatedHeaderFraction / (uploadDurationMs / 1000.0);
            return (bps, uploadDurationMs);
        }, cancellationToken);
    }

    #endregion

    /// <summary>
    /// Streams <paramref name="length"/> zero bytes as HTTP content without allocating
    /// a full-sized buffer. A single small shared chunk is reused across all writes.
    /// </summary>
    private sealed class ZeroStreamContent : HttpContent
    {
        private readonly int _length;
        private readonly Action _onWritten;
        private static readonly byte[] _chunk = new byte[81920];

        /// <param name="length">Number of bytes.</param>
        /// <param name="onWritten">Called after each written chunk (e.g. to restart a timeout).</param>
        internal ZeroStreamContent(int length, Action onWritten)
        {
            _length = length;
            _onWritten = onWritten;
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
                _onWritten?.Invoke();
            }
        }

        protected override bool TryComputeLength(out long length)
        {
            length = _length;
            return true;
        }
    }
}
