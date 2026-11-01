using NETworkManager.Models;

namespace NETworkManager;

/// <summary>
///     A notification of a tool (see <see cref="NotificationManager.Show" />): the same design for every tool - the tool
///     in the header, the status as icon (and sound), the target as title and the message.
/// </summary>
/// <param name="Application">Tool that sends the notification (icon and name in the header).</param>
/// <param name="Status">Status (icon, color and sound).</param>
/// <param name="Title">Target, e.g. the host or the scanned input.</param>
/// <param name="Message">What happened, e.g. "Host is up" or the changes of a scan.</param>
/// <param name="CloseTimeSeconds">Seconds before the popup closes automatically.</param>
public sealed record NotificationInfo(
    ApplicationName Application,
    NotificationStatus Status,
    string Title,
    string Message,
    int CloseTimeSeconds);
