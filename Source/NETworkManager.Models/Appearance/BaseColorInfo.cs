using System.Windows.Media;

namespace NETworkManager.Models.Appearance;

/// <summary>
///     Base class for MahApps.Metro theme/accent information's.
/// </summary>
public abstract class BaseColorInfo
{
    /// <summary>
    ///     Name of the theme/accent.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Color of the theme/accent.
    /// </summary>
    public Brush Color { get; set; }
}