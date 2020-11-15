using System.Windows.Media;

namespace NETworkManager.Models.Appearance
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
