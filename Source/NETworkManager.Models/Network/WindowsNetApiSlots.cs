using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network;

/// <summary>
///     Slots for the blocking NetApi requests of the IP Scanner, shared by all scans of the application (tabs, repeated
///     scans): a call that exceeds the timeout keeps its slot until Windows returns - also after its scan is completed -
///     so blocked calls can't pile up. Like a semaphore, but the max. number of slots is passed with every request: a
///     changed setting applies to the next request, also while calls of an earlier scan with another limit are still
///     running (a lower limit lets no new call start until enough of them returned).
/// </summary>
public sealed class WindowsNetApiSlots
{
    /// <summary>
    ///     Slots of all scans of the application.
    /// </summary>
    public static WindowsNetApiSlots Shared { get; } = new();

    private readonly Lock _lock = new();
    private int _used;

    // Completed when a slot is released, then replaced: wakes up all waiting requests, they check the limit again
    private TaskCompletionSource _released = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>
    ///     Number of slots in use (calls that are running, also after their timeout).
    /// </summary>
    public int Used
    {
        get
        {
            lock (_lock)
                return _used;
        }
    }

    /// <summary>
    ///     Waits for a free slot. Release it with <see cref="Release" /> when the call returned.
    /// </summary>
    /// <param name="maxSlots">Max. number of slots in use (incl. this one), at least 1.</param>
    /// <param name="timeout">Max. time to wait.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>True if a slot was taken, false after the timeout.</returns>
    public async Task<bool> WaitAsync(int maxSlots, TimeSpan timeout, CancellationToken cancellationToken)
    {
        maxSlots = Math.Max(1, maxSlots);

        var stopwatch = Stopwatch.StartNew();

        while (true)
        {
            Task released;

            lock (_lock)
            {
                if (_used < maxSlots)
                {
                    _used++;

                    return true;
                }

                released = _released.Task;
            }

            var remaining = timeout - stopwatch.Elapsed;

            if (remaining <= TimeSpan.Zero)
                return false;

            try
            {
                await released.WaitAsync(remaining, cancellationToken).ConfigureAwait(false);
            }
            catch (TimeoutException)
            {
                return false;
            }
        }
    }

    /// <summary>
    ///     Releases a slot taken with <see cref="WaitAsync" />.
    /// </summary>
    public void Release()
    {
        TaskCompletionSource released;

        lock (_lock)
        {
            if (_used == 0)
                throw new InvalidOperationException("No slot is in use.");

            _used--;

            released = _released;
            _released = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        released.SetResult();
    }
}
