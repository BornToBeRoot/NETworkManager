using System;
using System.Threading.Tasks;

namespace NETworkManager.Utilities;

/// <summary>
///     Runs an action that takes at least a minimum time, e.g. a manual refresh: the refresh button stays disabled (and
///     its icon spins) for the minimum time, so a refresh can't be spammed and the user sees that something happened.
///     The minimum runs at the same time as the action, a slower action isn't delayed further.
/// </summary>
public static class TaskHelper
{
    /// <summary>
    ///     Runs the action and returns its result when it is finished and the minimum time has passed.
    /// </summary>
    /// <param name="action">Action to run.</param>
    /// <param name="minimumMilliseconds">Minimum time in milliseconds.</param>
    /// <returns>Result of the action. An exception of the action is thrown after the minimum time too.</returns>
    public static async Task<T> WithMinimumDurationAsync<T>(Func<Task<T>> action, int minimumMilliseconds)
    {
        var minimum = Task.Delay(minimumMilliseconds);

        try
        {
            return await action();
        }
        finally
        {
            await minimum;
        }
    }

    /// <summary>
    ///     Runs the action and returns when it is finished and the minimum time has passed.
    /// </summary>
    /// <param name="action">Action to run.</param>
    /// <param name="minimumMilliseconds">Minimum time in milliseconds.</param>
    /// <returns>Task of the action. An exception of the action is thrown after the minimum time too.</returns>
    public static Task WithMinimumDurationAsync(Func<Task> action, int minimumMilliseconds)
    {
        return WithMinimumDurationAsync(async () =>
        {
            await action();
            return true;
        }, minimumMilliseconds);
    }
}
