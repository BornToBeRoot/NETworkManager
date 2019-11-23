using MahApps.Metro;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.IO;
using System.Windows;

namespace NETworkManager.Models.Settings
{
    public static class AppearanceManager
    {
        private static readonly string ThemesFilePath = Path.Combine(ConfigurationManager.Current.ExecutionPath, "Themes");

        private const string CostomThemeFileExtension = @".Theme.xaml";
        private const string CostomAccentFileExtension = @".Accent.xaml";

        public static MetroDialogSettings MetroDialog = new MetroDialogSettings();

        public static void Load()
        {
            // Add custom themes
            foreach (var file in Directory.GetFiles(ThemesFilePath))
            {
                var fileName = Path.GetFileName(file);

                // Theme
                if (fileName.EndsWith(CostomThemeFileExtension))
                    ThemeManager.AddAppTheme(fileName.Substring(0, fileName.Length - CostomThemeFileExtension.Length), new Uri(file));

                // Accent
                if (fileName.EndsWith(CostomAccentFileExtension))
                    ThemeManager.AddAccent(fileName.Substring(0, fileName.Length - CostomAccentFileExtension.Length), new Uri(file));
            }

            // Change the AppTheme if it is not empty and different from the currently loaded
            var appThemeName = SettingsManager.Current.Appearance_AppTheme;

            if (!string.IsNullOrEmpty(appThemeName) && appThemeName != ThemeManager.DetectAppStyle().Item1.Name)
                ChangeAppTheme(appThemeName);

            // Change the Accent if it is not empty and different from the currently loaded
            var accentName = SettingsManager.Current.Appearance_Accent;

            if (!string.IsNullOrEmpty(accentName) && accentName != ThemeManager.DetectAppStyle().Item2.Name)
                ChangeAccent(accentName);

            MetroDialog.CustomResourceDictionary = new ResourceDictionary
            {
                Source = new Uri("NETworkManager;component/Resources/Styles/MetroDialogStyles.xaml", UriKind.RelativeOrAbsolute)
            };
        }

        /// <summary>
        /// Change the AppTheme
        /// </summary>
        /// <param name="name">Name of the AppTheme</param>
        public static void ChangeAppTheme(string name)
        {
            var theme = ThemeManager.GetAppTheme(name);

            // If user has renamed / removed a custom theme --> fallback default
            if (theme != null)
                ThemeManager.ChangeAppTheme(Application.Current, name);
        }

        /// <summary>
        /// Change the Accent
        /// </summary>
        /// <param name="name">Name of the Accent</param>
        public static void ChangeAccent(string name)
        {
            var appStyle = ThemeManager.DetectAppStyle(Application.Current);
            var accent = ThemeManager.GetAccent(name);

            // If user has renamed / removed a custom theme --> fallback default
            if (accent != null)
                ThemeManager.ChangeAppStyle(Application.Current, accent, appStyle.Item1);
        }
    }
}
