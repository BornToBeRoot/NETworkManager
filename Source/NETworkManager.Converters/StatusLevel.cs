using System.Windows.Shapes;

namespace NETworkManager.Converters;

/// <summary>
///     Status of a value in a list (e.g. the action of a firewall rule), shown as colored dot (style <c>StatusDot</c>:
///     <see cref="Ellipse" /> with the level as <c>Tag</c>) in front of the text.
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
