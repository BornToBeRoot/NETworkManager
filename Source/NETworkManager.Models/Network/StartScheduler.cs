using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network;

/// <summary>
///     Spreads the start of concurrent operations over time (at most one start per interval) instead of starting
///     them all at the same moment. Used by <see cref="IPScanner" /> and <see cref="PortScanner" />: without it, all
///     hosts / ports of a "wave" start (and, because of the same timeouts, also finish) at once. That sends hundreds of
///     connection requests within a few milliseconds and makes the progress jump in large steps.
/// </summary>
/// <remarks>
///     Only the ramp-up is spread: the first <c>count</c> starts (the number of concurrent operations), where every
///     caller reserves the next free start time (one interval after the previous one) and only waits until then, so
///     the waits don't add up. After that, all slots are busy and an operation only starts when another one has
///     finished. These starts are spread like the ramp-up (the operations started at different times) and are not
///     delayed, so fast operations (e.g. a refused connection) are not limited to one per interval.
///     <see cref="Task.Delay(TimeSpan, CancellationToken)" /> fires on the system timer (~15.6 ms on Windows), so an
///     interval below that starts a few operations per timer tick instead of exactly one per interval.
/// </remarks>
/// <param name="interval">Minimum time between two starts during the ramp-up.</param>
/// <param name="count">Number of starts that are spread (the number of concurrent operations).</param>
internal sealed class StartScheduler(TimeSpan interval, int count)
{
    /// <summary>
    ///     Gets the interval between two starts: <paramref name="maxInterval" />, but at most
    ///     <paramref name="duration" /> / <paramref name="concurrency" />, so the ramp-up is never longer than
    ///     one operation (e.g. with short timeouts and many threads).
    /// </summary>
    /// <param name="maxInterval">Interval that should be used, if the ramp-up isn't longer than one operation.</param>
    /// <param name="duration">Maximum duration of one operation (e.g. the timeout).</param>
    /// <param name="concurrency">Maximum number of concurrent operations.</param>
    /// <returns>Interval between two starts.</returns>
    public static TimeSpan GetInterval(TimeSpan maxInterval, TimeSpan duration, int concurrency)
    {
        return concurrency <= 0
            ? maxInterval
            : TimeSpan.FromTicks(Math.Min(maxInterval.Ticks, duration.Ticks / concurrency));
    }

    private readonly Lock _lock = new();
    private readonly long _intervalTicks = (long)(interval.TotalSeconds * Stopwatch.Frequency);
    private long _nextStartTimestamp;
    private int _starts;

    /// <summary>
    ///     Waits until the next free start time (only during the ramp-up, see <see cref="StartScheduler" />).
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    public Task WaitAsync(CancellationToken cancellationToken)
    {
        if (_intervalTicks <= 0)
            return Task.CompletedTask;

        var now = Stopwatch.GetTimestamp();
        long start;

        lock (_lock)
        {
            // Ramp-up is done - this operation starts because another one has finished
            if (_starts >= count)
                return Task.CompletedTask;

            _starts++;

            start = Math.Max(now, _nextStartTimestamp);
            _nextStartTimestamp = start + _intervalTicks;
        }

        var delay = Stopwatch.GetElapsedTime(now, start);

        return delay > TimeSpan.Zero ? Task.Delay(delay, cancellationToken) : Task.CompletedTask;
    }
}
