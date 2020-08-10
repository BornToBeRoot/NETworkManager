using ControlzEx.Theming;
using MahApps.Metro.Controls.Dialogs;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace NETworkManager.Settings
{
    public static class AppearanceManager
    {
        public static MetroDialogSettings MetroDialog = new MetroDialogSettings();
        public static List<AccentColorInfo> Accents { get; set; }

        public static List<ThemeColorInfo> Themes { get; set; }

        static AppearanceManager()
        {
            Themes = ThemeManager.Current.Themes
                .GroupBy(x => x.BaseColorScheme)
                .Select(x => x.First())
                .Select(x => new ThemeColorInfo { Name = x.BaseColorScheme, Color = x.Resources["MahApps.Brushes.ThemeBackground"] as Brush })
                .ToList();

            Accents = ThemeManager.Current.Themes
                .GroupBy(x => x.ColorScheme)
                .OrderBy(x => x.Key)
                .Select(x => new AccentColorInfo { Name = x.Key, Color = x.First().ShowcaseBrush })
                .ToList();
        }

        public static void Load()
        {
            ChangeTheme(SettingsManager.Current.Appearance_Theme);
            ChangeAccent(SettingsManager.Current.Appearance_Accent);
        }

        public static void ChangeTheme(string name)
        {
            ThemeManager.Current.ChangeThemeBaseColor(System.Windows.Application.Current, name);
        }

        public static void ChangeAccent(string name)
        {
            ThemeManager.Current.ChangeThemeColorScheme(System.Windows.Application.Current, name);
        }
    }
}
