using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace NETworkManager.Models.Cloudflare;

/// <summary>
/// Calculations of the speed test, like the official
/// <a href="https://github.com/cloudflare/speedtest">cloudflare/speedtest</a> library.
/// </summary>
public static class SpeedTestCalculator
{
    /// <summary>
    /// Server time of the request, e.g. <c>cfRequestDuration;dur=12.3</c>.
    /// </summary>
    private static readonly Regex RequestDurationRegex = new(@"(?:^|,\s*)cfReq(?:uest)?Dur(?:ation)?;\s*dur=([0-9.]+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Parts of the server time (summed up if there is no request duration), e.g. <c>cfSpeedXyz;dur=1.2</c>.
    /// </summary>
    private static readonly Regex SpeedDurationRegex = new(@"(?:^|,\s*)cfSpeed[a-zA-Z]*;\s*dur=([0-9.]+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Server times up to this value (ms) are ignored.
    /// </summary>
    private const double ServerTimeMinDuration = 0.01;

    /// <summary>
    /// Longest delay before a request that failed with HTTP 429 is repeated.
    /// </summary>
    private static readonly TimeSpan MaxRetryDelay = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Percentile of the values with linear interpolation between the two closest ranks.
    /// </summary>
    /// <param name="values">Values (unsorted).</param>
    /// <param name="p">Percentile between 0 and 1 (e.g. 0.9).</param>
    /// <returns>Percentile, 0 if there are no values.</returns>
    public static double Percentile(IReadOnlyCollection<double> values, double p)
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

    /// <summary>
    /// Bandwidth shown while the test runs: the 90th percentile of the completed requests (like the result). Before the
    /// first request has completed, the throughput of the running request.
    /// </summary>
    /// <param name="samplesBps">Bandwidth of the completed requests (bit/s).</param>
    /// <param name="liveBps">Throughput of the running request (bit/s), <c>null</c> if none.</param>
    /// <returns>Bandwidth in bit/s, <c>null</c> if there is no value yet.</returns>
    public static double? CurrentBandwidth(IReadOnlyCollection<double> samplesBps, double? liveBps)
    {
        return samplesBps.Count > 0 ? Percentile(samplesBps, 0.9) : liveBps;
    }

    /// <summary>
    /// A direction (download / upload) is finished when even the fastest request of a set took longer than the finish
    /// duration: the larger sets would take too long (<c>bandwidthFinishRequestDuration</c> of the official library).
    /// </summary>
    /// <param name="durationsMs">Duration of the requests of the set (ms).</param>
    /// <param name="finishRequestDurationMs">Finish duration (ms).</param>
    /// <returns><c>true</c> if the larger sets of the direction are skipped.</returns>
    public static bool IsDirectionFinished(IReadOnlyCollection<double> durationsMs, double finishRequestDurationMs)
    {
        return durationsMs.Count > 0 && durationsMs.Min() > finishRequestDurationMs;
    }

    /// <summary>
    /// Delay before a request that failed with HTTP 429 is repeated, from the <c>Retry-After</c> header (like
    /// <c>getRetryDelay</c> of the official library).
    /// </summary>
    /// <param name="delta">Delay of the header (seconds).</param>
    /// <param name="date">Date of the header.</param>
    /// <param name="now">Current time.</param>
    /// <returns>Delay, 5 seconds without a header, at most 60 seconds (the test should not wait endlessly).</returns>
    public static TimeSpan GetRetryDelay(TimeSpan? delta, DateTimeOffset? date, DateTimeOffset now)
    {
        var delay = delta ?? (date.HasValue ? date.Value - now : TimeSpan.FromSeconds(5));

        if (delay < TimeSpan.Zero)
            return TimeSpan.Zero;

        return delay > MaxRetryDelay ? MaxRetryDelay : delay;
    }

    /// <summary>
    /// A request failed without a connection to the server (no response, connection lost, timeout), not because the
    /// server or a proxy rejected it with an HTTP error.
    /// </summary>
    /// <param name="ex">Error of the request.</param>
    /// <returns><c>true</c> if there was no response.</returns>
    public static bool IsConnectionFailure(Exception ex)
    {
        return ex is not HttpRequestException { StatusCode: not null };
    }

    /// <summary>
    /// Jitter: average absolute difference between consecutive latency samples.
    /// </summary>
    /// <param name="pings">Latency samples in the order they were measured.</param>
    /// <returns>Jitter, 0 if there are fewer than two samples.</returns>
    public static double Jitter(IReadOnlyList<double> pings)
    {
        if (pings == null || pings.Count < 2)
            return 0.0;

        double sum = 0;

        for (var i = 1; i < pings.Count; i++)
            sum += Math.Abs(pings[i] - pings[i - 1]);

        return sum / (pings.Count - 1);
    }

    /// <summary>
    /// Time the server needed to process the request, from the <c>server-timing</c> header: the request duration
    /// (<c>cfRequestDuration</c>), otherwise the sum of the <c>cfSpeed*</c> durations (like <c>cfGetServerTime</c> of the
    /// official library).
    /// </summary>
    /// <param name="values">Values of the <c>server-timing</c> header.</param>
    /// <returns>Server time in milliseconds, <c>null</c> if the header has no server time.</returns>
    public static double? ParseServerTiming(IEnumerable<string> values)
    {
        if (values == null)
            return null;

        var header = string.Join(", ", values);

        if (header.Length == 0)
            return null;

        var match = RequestDurationRegex.Match(header);

        if (match.Success && TryParseDuration(match, out var requestDuration) &&
            requestDuration > ServerTimeMinDuration)
            return requestDuration;

        double sum = 0;

        foreach (Match speedMatch in SpeedDurationRegex.Matches(header))
        {
            if (TryParseDuration(speedMatch, out var duration))
                sum += duration;
        }

        return sum > ServerTimeMinDuration ? sum : null;
    }

    private static bool TryParseDuration(Match match, out double duration) =>
        double.TryParse(match.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out duration);
}
