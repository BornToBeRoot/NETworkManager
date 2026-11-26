using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
