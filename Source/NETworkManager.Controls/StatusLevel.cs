namespace NETworkManager.Controls;

/// <summary>
///     Status of a value or message: colored dot in front of a value in a list (style <c>StatusDot</c>, the level as
///     <c>Tag</c>, e.g. the action of a firewall rule) or icon + color of a status message (<see cref="StatusMessageText" />).
/// </summary>
public enum StatusLevel
{
    /// <summary>
    ///     Neutral (gray).
    /// </summary>
    None,

    /// <summary>
    ///     Good, e.g. allowed, established, reachable (<c>Brushes.Status.Success</c>).
    /// </summary>
    Success,

    /// <summary>
    ///     In between, e.g. connecting, stale (<c>Brushes.Status.Warning</c>).
    /// </summary>
    Warning,

    /// <summary>
    ///     Bad, e.g. blocked, unreachable (<c>Brushes.Status.Error</c>).
    /// </summary>
    Error
}
