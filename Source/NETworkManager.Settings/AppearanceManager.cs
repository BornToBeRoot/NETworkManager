using ControlzEx.Theming;
using MahApps.Metro.Theming;
using NETworkManager.Models.Appearance;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace NETworkManager.Settings;

/// <summary>
///     Class provides static variables and methods to change the theme and accent of the application.
/// </summary>
public static class AppearanceManager
{
    #region Variables
    /// <summary>
    ///     Dictionary to override some brushes for the light theme.
    /// </summary>
    private static readonly ResourceDictionary LightThemeOverrideDictionary = new()
    {
        //["MahApps.Brushes.ThemeBackground"] = new SolidColorBrush(Color.FromRgb(248, 248, 255)),
        //["MahApps.Brushes.Control.Background"] = new SolidColorBrush(Color.FromRgb(248, 248, 255)),
        ["MahApps.Brushes.Gray3"] = new SolidColorBrush(Color.FromRgb(104, 104, 104)),
        ["MahApps.Brushes.Gray5"] = new SolidColorBrush(Color.FromRgb(138, 138, 138)),
        ["MahApps.Brushes.Gray8"] = new SolidColorBrush(Color.FromRgb(190, 190, 190)),
    };

    /// <summary>
    ///     Dictionary to override some brushes for the dark theme.
    /// </summary>
    private static readonly ResourceDictionary DarkThemeOverrideDictionary = new()
    {

    };

    /// <summary>
    ///     Name of the folder inside the application directory where the custom themes are stored.
    /// </summary>
    private const string ThemeFolderName = "Themes";

    /// <summary>
    ///     List who contains all MahApps.Metro themes.
    /// </summary>
    public static List<ThemeColorInfo> Themes { get; set; }

    /// <summary>
    ///     List who contains all MahApps.Metro custom themes.
    /// </summary>
    public static List<ThemeInfo> CustomThemes { get; private set; }

    /// <summary>
    ///     List who contains all MahApps.Metro accents.
    /// </summary>
    public static List<AccentColorInfo> Accents { get; set; }
    #endregion

    #region Constructor
    /// <summary>
    ///     Load the MahApps.Metro themes and accents when needed.
    /// </summary>
    static AppearanceManager()
    {
        Themes = [.. ThemeManager.Current.Themes
            .GroupBy(x => x.BaseColorScheme)
            .Select(x => x.First())
            .Select(x => new ThemeColorInfo
            { Name = x.BaseColorScheme, Color = x.Resources["MahApps.Brushes.ThemeBackground"] as Brush })];

        Accents = [.. ThemeManager.Current.Themes
            .GroupBy(x => x.ColorScheme)
            .OrderBy(x => x.Key)
            .Select(x => new AccentColorInfo { Name = x.Key, Color = x.First().ShowcaseBrush })];

        ThemeManager.Current.ThemeChanged += Current_ThemeChanged;

        LoadCustomThemes();
    }
    #endregion

    #region Methods
    /// <summary>
    ///     Change the appearance based on the user settings. This method should be called once, when starting the application.
    /// </summary>
    public static void Load()
    {
        if (SettingsManager.Current.Appearance_UseCustomTheme && CustomThemes.Count > 0)
        {
            ChangeTheme(
                (CustomThemes.FirstOrDefault(x => x.Name == SettingsManager.Current.Appearance_CustomThemeName) ??
                CustomThemes.First()).Name);

            return;
        }

        ChangeTheme(SettingsManager.Current.Appearance_Theme, SettingsManager.Current.Appearance_Accent);
    }

    /// <summary>
    ///     Method to load the custom themes from a folder into the <see cref="ThemeManager" />.
    /// </summary>
    private static void LoadCustomThemes()
    {
        List<ThemeInfo> customThemes = [];

        foreach (var file in Directory.GetFiles(Path.Combine(ConfigurationManager.Current.ExecutionPath,
                     ThemeFolderName)))
        {
            LibraryTheme libraryTheme = new(new Uri(file), MahAppsLibraryThemeProvider.DefaultInstance);

            customThemes.Add(new ThemeInfo(libraryTheme.Name, libraryTheme.DisplayName));

            ThemeManager.Current.AddLibraryTheme(libraryTheme);
        }

        CustomThemes = customThemes;
    }

    /// <summary>
    ///     Method to change the application theme (and accent).
    /// </summary>
    /// <param name="name">Theme name as "theme.accent" to apply.</param>
    public static void ChangeTheme(string name)
    {
        ThemeManager.Current.ChangeTheme(System.Windows.Application.Current, name);
    }

    /// <summary>
    ///     Method to change the application theme (and accent).
    /// </summary>
    /// <param name="theme">Theme name to apply.</param>
    /// <param name="accent">Accent name to apply.</param>
    public static void ChangeTheme(string theme, string accent)
    {
        ChangeTheme($"{theme}.{accent}");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="theme"></param>
    private static void ApplyCustomThemeFixes(string theme)
    {
        // Don't apply on custom themes (only built-in light/dark themes)
        if (CustomThemes.Any(x => x.Name.Equals(theme, StringComparison.OrdinalIgnoreCase)))
            return;

        // Get application resources
        var appResources = System.Windows.Application.Current.Resources;

        // Remove any existing theme overrides
        appResources.MergedDictionaries.Remove(LightThemeOverrideDictionary);
        appResources.MergedDictionaries.Remove(DarkThemeOverrideDictionary);


        // Theme name is in format "theme.accent"
        switch (theme.ToLowerInvariant().Split('.')[0])
        {
            case "light":
                appResources.MergedDictionaries.Add(LightThemeOverrideDictionary);
                break;

            case "dark":
                appResources.MergedDictionaries.Add(DarkThemeOverrideDictionary);
                break;
        }

        // Refresh UI
        foreach (Window window in System.Windows.Application.Current.Windows)
        {
            if (window.IsLoaded)
                window.InvalidateVisual();
        }
    }

    #endregion

    #region Events
    private static void Current_ThemeChanged(object sender, ThemeChangedEventArgs e)
    {
        ApplyCustomThemeFixes(e.NewTheme.Name);
    }
    #endregion
}
