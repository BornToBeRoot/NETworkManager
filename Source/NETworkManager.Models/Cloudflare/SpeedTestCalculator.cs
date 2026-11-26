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
    private static readonly Regex ServerTimingRegex = new(@"(?:^|;)\s*dur=([0-9.]+)", RegexOptions.Compiled);

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
    /// Time the server needed to process the request, from the <c>server-timing</c> header.
    /// </summary>
    /// <param name="values">Values of the <c>server-timing</c> header.</param>
    /// <returns>Server time in milliseconds, <c>null</c> if the header has no duration.</returns>
    public static double? ParseServerTiming(IEnumerable<string> values)
    {
        if (values == null)
            return null;

        foreach (var value in values)
        {
            var match = ServerTimingRegex.Match(value);

            if (match.Success && double.TryParse(match.Groups[1].Value, NumberStyles.Float,
                    CultureInfo.InvariantCulture, out var duration))
                return duration;
        }

        return null;
    }
}
