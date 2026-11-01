namespace NETworkManager.Models;

/// <summary>
///     Status of a notification (e.g. a host of the Ping Monitor, the changes of a repeated scan), shown by its icon,
///     color and sound.
/// </summary>
public enum NotificationStatus
{
    /// <summary>
    ///     Something is up / was added (host is up, new hosts, ports open now).
    /// </summary>
    Up,

    /// <summary>
    ///     Something is down / went away (host is down, hosts gone, ports no longer open).
    /// </summary>
    Down,

    /// <summary>
    ///     Both: something is up and something is down (e.g. a new and a gone host).
    /// </summary>
    UpAndDown,

    /// <summary>
    ///     Another change that is neither up nor down (e.g. a MAC address changed).
    /// </summary>
    Changed
}

/// <summary>
///     Methods for <see cref="NotificationStatus" />.
/// </summary>
public static class NotificationStatusHelper
{
    /// <summary>
    ///     Gets the status of a notification about changes: up if there are only additions, down if there are only
    ///     removals, up and down if there are both, otherwise changed (e.g. only a changed MAC address).
    /// </summary>
    /// <param name="additions">Number of additions (e.g. new hosts, ports open now).</param>
    /// <param name="removals">Number of removals (e.g. gone hosts, ports no longer open).</param>
    /// <param name="otherChanges">Number of other changes (e.g. changed MAC addresses).</param>
    /// <returns>Status of the notification.</returns>
    public static NotificationStatus FromChanges(int additions, int removals, int otherChanges)
    {
        if (additions > 0 && removals > 0)
            return NotificationStatus.UpAndDown;

        if (otherChanges > 0)
            return NotificationStatus.Changed;

        if (additions > 0)
            return NotificationStatus.Up;

        return removals > 0 ? NotificationStatus.Down : NotificationStatus.Changed;
    }
}
