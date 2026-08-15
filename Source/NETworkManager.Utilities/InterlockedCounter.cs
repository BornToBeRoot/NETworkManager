using System.Threading;

namespace NETworkManager.Utilities;

/// <summary>
///     A simple counter that can be incremented from any thread and read from any other thread
///     without locking.
/// </summary>
public sealed class InterlockedCounter
{
    private int _value;

    /// <summary>
    ///     Gets the current value.
    /// </summary>
    public int Value => Volatile.Read(ref _value);

    /// <summary>
    ///     Increments the value by one.
    /// </summary>
    public void Increment()
    {
        Interlocked.Increment(ref _value);
    }
}
