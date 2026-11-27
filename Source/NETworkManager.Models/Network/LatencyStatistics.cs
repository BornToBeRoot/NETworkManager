using System;
using System.Collections.Generic;
using System.Linq;

namespace NETworkManager.Models.Network;

/// <summary>
///     Result of one ping of a latency target (internet latency widget of the dashboard).
/// </summary>
/// <param name="Timestamp">Time of the ping (UTC: a clock change doesn't move the time window).</param>
/// <param name="IsSuccess">The target replied.</param>
/// <param name="Time">Round-trip time in milliseconds (0 without a reply).</param>
public readonly record struct LatencySample(DateTime Timestamp, bool IsSuccess, long Time);

/// <summary>
///     State of a latency target, shown as the status dot of the widget.
/// </summary>
public enum LatencyState
{
    /// <summary>
    ///     Not pinged yet.
    /// </summary>
    None,

    /// <summary>
    ///     The last ping was answered and no ping was lost in the time window.
    /// </summary>
    Ok,

    /// <summary>
    ///     The last ping was answered, but a ping was lost in the time window.
    /// </summary>
    Lost,

    /// <summary>
    ///     The last ping was not answered (e.g. timeout, unreachable).
    /// </summary>
    Failed
}

/// <summary>
///     Statistics of the pings of a latency target in the time window.
/// </summary>
/// <param name="Count">Number of pings.</param>
/// <param name="Lost">Pings without a reply.</param>
/// <param name="Minimum">Lowest round-trip time, <c>null</c> without a reply.</param>
/// <param name="Average">Average round-trip time of the replies, <c>null</c> without a reply.</param>
/// <param name="Maximum">Highest round-trip time, <c>null</c> without a reply.</param>
public sealed record LatencyStatisticsInfo(int Count, int Lost, long? Minimum, double? Average, long? Maximum)
{
    /// <summary>
    ///     Pings without a reply in percent (0 without pings).
    /// </summary>
    public double PacketLoss => Count == 0 ? 0 : (double)Lost / Count * 100;
}

/// <summary>
///     Statistics and state of the pings of a latency target.
/// </summary>
public static class LatencyStatistics
{
    /// <summary>
    ///     Calculates the statistics of the pings.
    /// </summary>
    /// <param name="samples">Pings in the time window.</param>
    /// <returns>Statistics, all values <c>null</c> without a reply.</returns>
    public static LatencyStatisticsInfo Calculate(IReadOnlyCollection<LatencySample> samples)
    {
        var replies = samples.Where(x => x.IsSuccess).Select(x => x.Time).ToList();

        return replies.Count == 0
            ? new LatencyStatisticsInfo(samples.Count, samples.Count, null, null, null)
            : new LatencyStatisticsInfo(samples.Count, samples.Count - replies.Count, replies.Min(), replies.Average(),
                replies.Max());
    }

    /// <summary>
    ///     State of the target: the last ping decides whether it is reachable, a lost ping in the time window is a
    ///     warning.
    /// </summary>
    /// <param name="samples">Pings in the time window, the last one is the newest.</param>
    /// <returns>State of the target.</returns>
    public static LatencyState GetState(IReadOnlyList<LatencySample> samples)
    {
        if (samples.Count == 0)
            return LatencyState.None;

        if (!samples[^1].IsSuccess)
            return LatencyState.Failed;

        return samples.Any(x => !x.IsSuccess) ? LatencyState.Lost : LatencyState.Ok;
    }

    /// <summary>
    ///     Removes the pings older than the time window (the list is ordered by time).
    /// </summary>
    /// <param name="samples">Pings, the oldest first.</param>
    /// <param name="now">Current time.</param>
    /// <param name="window">Time window, e.g. 60 seconds.</param>
    public static void RemoveOutdated(List<LatencySample> samples, DateTime now, TimeSpan window)
    {
        var cutoff = now - window;
        var count = 0;

        while (count < samples.Count && samples[count].Timestamp < cutoff)
            count++;

        samples.RemoveRange(0, count);
    }
}
