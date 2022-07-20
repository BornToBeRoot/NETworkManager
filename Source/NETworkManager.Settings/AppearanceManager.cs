using ControlzEx.Theming;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.Theming;
using System;
using System.Collections.Generic;
using System.IO;
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
        /// Name of the folder inside the application directory where the custom themes are stored.
        /// </summary>
        private const string ThemeFolderName = "Themes";

        /// <summary>
        /// List who contains all MahApps.Metro themes.
        /// </summary>
        public static List<ThemeColorInfo> Themes { get; set; }

        /// <summary>
        /// List who contains all MahApps.Metro custom themes.
        /// </summary>
        public static List<ThemeInfo> CustomThemes { get; set; }

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

            LoadCustomThemes();

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
            if (SettingsManager.Current.Appearance_UseCustomTheme && CustomThemes.Count > 0)
            {
                ChangeTheme(CustomThemes.FirstOrDefault(x => x.Name == SettingsManager.Current.Appearance_CustomThemeName) ?? CustomThemes.First());
            }
            else
            {
                ChangeTheme(SettingsManager.Current.Appearance_Theme);
                ChangeAccent(SettingsManager.Current.Appearance_Accent);
            }
        }

        /// <summary>
        /// Method to load the custom themes from a folder into the <see cref="ThemeManager"/>.
        /// </summary>
        private static void LoadCustomThemes()
        {
            List<ThemeInfo> customThemes = new();

            foreach (var file in Directory.GetFiles(Path.Combine(ConfigurationManager.Current.ExecutionPath, ThemeFolderName)))
            {
                LibraryTheme libraryTheme = new LibraryTheme(new Uri(file), MahAppsLibraryThemeProvider.DefaultInstance);

                customThemes.Add(new ThemeInfo(libraryTheme.Name, libraryTheme.DisplayName));

                ThemeManager.Current.AddLibraryTheme(libraryTheme);
            }

            CustomThemes = customThemes;
        }

        /// <summary>
        /// Method to change the application theme.
        /// </summary>
        /// <param name="name">Name of the MahApps theme base color.</param>
        public static void ChangeTheme(string name)
        {
            ThemeManager.Current.ChangeThemeBaseColor(System.Windows.Application.Current, name);
        }

        /// <summary>
        /// Method to change the application accent.
        /// </summary>
        /// <param name="name">Name of the MahApps theme accent color.</param>
        public static void ChangeAccent(string name)
        {
            ThemeManager.Current.ChangeThemeColorScheme(System.Windows.Application.Current, name);
        }

        /// <summary>
        /// Method to changer the application theme based on the name in <see cref="ThemeInfo"/>.
        /// </summary>
        /// <param name="themeInfo">Theme as <see cref="ThemeInfo"/> to apply.</param>
        public static void ChangeTheme(ThemeInfo themeInfo)
        {
            ThemeManager.Current.ChangeTheme(System.Windows.Application.Current, themeInfo.Name);
        }
    }
}
