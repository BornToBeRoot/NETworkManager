using System.Windows.Media;

namespace NETworkManager.Settings
{
    /// <summary>
    /// Base class for MahApps.Metro theme/accent informations.
    /// </summary>
    public abstract class BaseColorInfo
    {
        public string Name { get; set; }
        public Brush Color { get; set; }
    }
}
