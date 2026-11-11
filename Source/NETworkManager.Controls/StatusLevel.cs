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

/// <summary>
///     Helpers for <see cref="StatusLevel" />.
/// </summary>
public static class StatusLevelExtensions
{
    /// <summary>
    ///     The more severe of two levels (None &lt; Success &lt; Warning &lt; Error), e.g. when a message is appended to the
    ///     status message: the most severe message determines the icon and color.
    /// </summary>
    public static StatusLevel MostSevere(this StatusLevel level, StatusLevel other) => other > level ? other : level;
}
