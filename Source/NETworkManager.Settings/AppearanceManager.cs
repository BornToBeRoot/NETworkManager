using ControlzEx.Theming;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace NETworkManager.Settings
{
    /// <summary>
    /// Class provides static variables and methods to change the theme and accent of the application.
    /// </summary>
    public static class AppearanceManager
    {
        /// <summary>
        /// List who contains all MahApps.Metro themes.
        /// </summary>
        public static List<ThemeColorInfo> Themes { get; set; }

        /// <summary>
        /// List who contains all MahApps.Metro accents.
        /// </summary>
        public static List<AccentColorInfo> Accents { get; set; }

        /// <summary>
        /// Containes the default settings for a new <see cref="BaseMetroDialog"/> 
        /// </summary>
        public static MetroDialogSettings MetroDialog = new MetroDialogSettings();

        /// <summary>
        /// Load the MahApps.Metro themes and accents when needed.
        /// </summary>
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

            MetroDialog.CustomResourceDictionary = new ResourceDictionary
            {
                Source = new Uri("NETworkManager;component/Resources/Styles/MetroDialogStyles.xaml", UriKind.RelativeOrAbsolute),
            };

            MetroDialog.DialogButtonFontSize = 14;
            MetroDialog.DialogMessageFontSize = 14;
        }
       
        /// <summary>
        /// Change the appearance based on the user settings. This method should be called once, when starting the application.
        /// </summary>
        public static void Load()
        {
            ChangeTheme(SettingsManager.Current.Appearance_Theme);
            ChangeAccent(SettingsManager.Current.Appearance_Accent);
        }

        /// <summary>
        /// Method to change the application theme.
        /// </summary>
        /// <param name="name">Name of the theme.</param>
        public static void ChangeTheme(string name)
        {
            ThemeManager.Current.ChangeThemeBaseColor(System.Windows.Application.Current, name);
        }

        /// <summary>
        /// Method to change the application accent.
        /// </summary>
        /// <param name="name">Name of the accent.</param>
        public static void ChangeAccent(string name)
        {
            ThemeManager.Current.ChangeThemeColorScheme(System.Windows.Application.Current, name);
        }
    }
}
