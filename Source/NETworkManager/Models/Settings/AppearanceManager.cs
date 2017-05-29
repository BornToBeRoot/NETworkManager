using MahApps.Metro;
using System;
using System.Windows;

namespace NETworkManager.Models.Settings
{
    public static class AppearanceManager
    {
        /// <summary>
        /// Load Appearance (AppTheme and Accent) from the user settings.
        /// </summary>
        public static void Load()
        {
            // Change the AppTheme if it is not empty and different from the currently loaded
            string appThemeName = SettingsManager.Current.Appearance_AppTheme;

            if (!string.IsNullOrEmpty(appThemeName) && appThemeName != ThemeManager.DetectAppStyle().Item1.Name)
                ChangeAppTheme(appThemeName);

            // Change the Accent if it is not empty and different from the currently loaded
            string accentName = SettingsManager.Current.Appearance_Accent;

            if (!string.IsNullOrEmpty(accentName) && accentName != ThemeManager.DetectAppStyle().Item2.Name)
                ChangeAccent(accentName);
        }

        /// <summary>
        /// Change the AppTheme
        /// </summary>
        /// <param name="name">Name of the AppTheme</param>
        public static void ChangeAppTheme(string name)
        {
            ThemeManager.ChangeAppTheme(Application.Current, name);
        }

        /// <summary>
        /// Change the Accent
        /// </summary>
        /// <param name="name">Name of the Accent</param>
        public static void ChangeAccent(string name)
        {
            Tuple<AppTheme, Accent> appStyle = ThemeManager.DetectAppStyle(Application.Current);
            Accent accent = ThemeManager.GetAccent(name);

            ThemeManager.ChangeAppStyle(Application.Current, accent, appStyle.Item1);
        }
    }
}
