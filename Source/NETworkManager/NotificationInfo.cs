using NETworkManager.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace NETworkManager;

/// <summary>
///     A notification of a tool (see <see cref="NotificationManager.Show" />): the same design for every tool - the tool
///     in the header, the status as icon (and sound), the target as title and one line per change.
/// </summary>
/// <param name="Application">Tool that sends the notification (icon and name in the header).</param>
/// <param name="Status">Status (icon, color and sound).</param>
/// <param name="Title">Target, e.g. the host or the scanned input.</param>
/// <param name="Lines">What happened, one line per change (e.g. "10.0.0.1 · Up"), see <see cref="NotificationLine" />.</param>
/// <param name="CloseTimeSeconds">Seconds before the popup closes automatically.</param>
public sealed record NotificationInfo(
    ApplicationName Application,
    NotificationStatus Status,
    string Title,
    IReadOnlyList<NotificationLine> Lines,
    int CloseTimeSeconds)
{
    /// <summary>
    ///     Opened when the notification is clicked (e.g. the tab of the scan, see <see cref="ShowTabRequest" />), null
    ///     to open the tool.
    /// </summary>
    public ApplicationRequest Request { get; init; }
}

/// <summary>
///     A line of a <see cref="NotificationInfo" />: what changed and its status, shown as "10.0.0.5 (server) · New" with
///     the status in its color (like the status icon). The same syntax for every tool: the address first, then the
///     details (e.g. "22/tcp (ssh)"), the status last.
/// </summary>
/// <param name="Text">What changed, e.g. "10.0.0.5 (server)", null for a line with only the status (e.g. "5 New").</param>
/// <param name="Status">Status text, e.g. "Up", "New" or "No longer open".</param>
/// <param name="StatusKind">Status (color of the status text): up, down or changed.</param>
public sealed record NotificationLine(string Text, string Status, NotificationStatus StatusKind)
{
    /// <summary>
    ///     Separator between the parts of a line (the address, the details and the status), like the counters of the
    ///     tools.
    /// </summary>
    public const string Separator = " · ";

    /// <summary>
    ///     Joins the parts of the text of a line (empty parts are skipped), e.g. "10.0.0.5 (server)" and "22/tcp (ssh)".
    /// </summary>
    /// <param name="parts">Parts of the text.</param>
    /// <returns>Text of the line.</returns>
    public static string JoinText(params string[] parts)
    {
        return string.Join(Separator, parts.Where(x => !string.IsNullOrEmpty(x)));
    }

    /// <summary>
    ///     Formats an IP address with its hostname, if any, e.g. "10.0.0.5 (server)".
    /// </summary>
    /// <param name="ipAddress">IP address.</param>
    /// <param name="hostname">Hostname, null or empty if unknown.</param>
    /// <returns>IP address with the hostname.</returns>
    public static string FormatHost(IPAddress ipAddress, string hostname)
    {
        return string.IsNullOrEmpty(hostname) ? $"{ipAddress}" : $"{ipAddress} ({hostname})";
    }
}
