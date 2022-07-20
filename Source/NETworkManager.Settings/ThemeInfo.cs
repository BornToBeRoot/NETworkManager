
namespace NETworkManager.Settings
{
    /// <summary>
    /// Class contains informations about MahApps.Metro custom themes.
    /// </summary>
    public class ThemeInfo
    {
        /// <summary>
        /// Name of the MahApps.Metro custom theme.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Display name of the MahApps.Metro custom theme.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Initializes a new instance of the<see cref="ThemeInfo"/> class with properties.
        /// </summary>
        /// <param name="name">Name of of the MahApps.Metro custom theme.</param>
        /// <param name="displayName">Display name of the MahApps.Metro custom theme.</param>
        public ThemeInfo(string name, string displayName)
        {
            Name = name;
            DisplayName = displayName;
        }
    }
}
