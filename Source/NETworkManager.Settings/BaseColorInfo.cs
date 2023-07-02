using System.Windows.Media;

namespace NETworkManager.Settings;

/// <summary>
/// Base class for MahApps.Metro theme/accent informations.
/// </summary>
public abstract class BaseColorInfo
{
    /// <summary>
    /// Gets or sets the name of the theme/accent.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the color of the theme/accent.
    /// </summary>
    public Brush Color { get; set; }
}
