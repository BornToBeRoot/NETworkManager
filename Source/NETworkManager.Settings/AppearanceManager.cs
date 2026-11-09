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
    ///     Resource key of the background of a control (e.g. button, ComboBox) when the mouse is over it.
    /// </summary>
    public const string ControlMouseOverBrushKey = "Brushes.Control.MouseOver";

    /// <summary>
    ///     Resource key of the background of a control (e.g. button, ComboBox) while it is pressed.
    /// </summary>
    public const string ControlPressedBrushKey = "Brushes.Control.Pressed";

    /// <summary>
    ///     Resource key of the background of a (secondary) button, e.g. "Export...", "Cancel".
    /// </summary>
    public const string ButtonBackgroundBrushKey = "Brushes.Button.Background";

    /// <summary>
    ///     Resource key of the background of a (secondary) button when the mouse is over it.
    /// </summary>
    public const string ButtonMouseOverBrushKey = "Brushes.Button.MouseOver";

    /// <summary>
    ///     Resource key of the background of a (secondary) button while it is pressed.
    /// </summary>
    public const string ButtonPressedBrushKey = "Brushes.Button.Pressed";

    /// <summary>
    ///     Resource key of the background of a card (e.g. Dashboard widget, Ping Monitor host).
    /// </summary>
    public const string SurfaceCardBrushKey = "Brushes.Surface.Card";

    /// <summary>
    ///     Resource key of the background of the main window frame around and between the boxes (navigation, main view,
    ///     profiles).
    /// </summary>
    public const string SurfaceFrameBrushKey = "Brushes.Surface.Frame";

    /// <summary>
    ///     Resource key of the background of a selected row (data grid) when the mouse is over it.
    /// </summary>
    public const string DataGridSelectionMouseOverBrushKey = "Brushes.DataGrid.Selection.MouseOver";

    /// <summary>
    ///     Resource key of the border of an input (e.g. TextBox, ComboBox) when the mouse is over it.
    /// </summary>
    public const string ControlBorderMouseOverBrushKey = "Brushes.Control.Border.MouseOver";

    /// <summary>
    ///     Resource key of the status color "success" (e.g. host up, port open, firewall rule allows).
    /// </summary>
    public const string StatusSuccessBrushKey = "Brushes.Status.Success";

    /// <summary>
    ///     Resource key of the status color "warning" (e.g. changed since the last scan, warning icon).
    /// </summary>
    public const string StatusWarningBrushKey = "Brushes.Status.Warning";

    /// <summary>
    ///     Resource key of the status color "error" (e.g. host down, port closed, error icon, validation error).
    /// </summary>
    public const string StatusErrorBrushKey = "Brushes.Status.Error";

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
    ///     Set the backgrounds of controls (e.g. icon buttons, ComboBox) for mouse over and pressed, the backgrounds of
    ///     (secondary) buttons and cards and the border of inputs for mouse over: the foreground color of
    ///     the theme with a low opacity, a subtle tint in every theme (also custom themes) instead of the light gray of
    ///     MahApps (Gray9, Gray7, Gray5), which stands out in the dark theme.
    /// </summary>
    /// <param name="theme">Theme that was applied.</param>
    private static void ApplyControlStateBrushes(Theme theme)
    {
        var foreground = theme.Resources["MahApps.Colors.ThemeForeground"] is Color color ? color : Colors.Gray;

        var appResources = System.Windows.Application.Current.Resources;

        appResources[ControlMouseOverBrushKey] = CreateFrozenBrush(foreground, 0.08);
        appResources[ControlPressedBrushKey] = CreateFrozenBrush(foreground, 0.12);
        appResources[ControlBorderMouseOverBrushKey] = CreateFrozenBrush(foreground, 0.35);

        appResources[ButtonBackgroundBrushKey] = CreateFrozenBrush(foreground, 0.06);
        appResources[ButtonMouseOverBrushKey] = CreateFrozenBrush(foreground, 0.10);
        appResources[ButtonPressedBrushKey] = CreateFrozenBrush(foreground, 0.14);

        appResources[SurfaceCardBrushKey] = CreateFrozenBrush(foreground, 0.03);

        // Opaque (window background): 4 % of the foreground on the background of the theme
        var background = theme.Resources["MahApps.Colors.ThemeBackground"] is Color backgroundColor
            ? backgroundColor
            : Colors.White;

        appResources[SurfaceFrameBrushKey] = CreateFrozenBrush(Blend(background, foreground, 0.04), 1);

        ApplyDataGridBrushes(theme, foreground);
    }

    /// <summary>
    ///     Override the selection brushes of the data grids (MahApps uses them via DynamicResource): a tint of the accent
    ///     color with the normal text instead of the accent color with white text, a neutral tint on mouse over instead
    ///     of the accent color and no accent border around the selected row.
    /// </summary>
    /// <param name="theme">Theme that was applied.</param>
    /// <param name="foreground">Foreground color of the theme.</param>
    private static void ApplyDataGridBrushes(Theme theme, Color foreground)
    {
        var accent = theme.Resources["MahApps.Colors.Accent"] is Color color ? color : Colors.Gray;

        var appResources = System.Windows.Application.Current.Resources;

        var foregroundBrush = CreateFrozenBrush(foreground, 1);
        var transparentBrush = CreateFrozenBrush(Colors.Transparent, 1);

        appResources["MahApps.Brushes.DataGrid.Selection.Background"] = CreateFrozenBrush(accent, 0.30);
        appResources["MahApps.Brushes.DataGrid.Selection.Background.Inactive"] = CreateFrozenBrush(accent, 0.20);
        appResources["MahApps.Brushes.DataGrid.Selection.Background.MouseOver"] = CreateFrozenBrush(foreground, 0.08);
        appResources[DataGridSelectionMouseOverBrushKey] = CreateFrozenBrush(accent, 0.38);

        appResources["MahApps.Brushes.DataGrid.Selection.BorderBrush"] = transparentBrush;
        appResources["MahApps.Brushes.DataGrid.Selection.BorderBrush.Inactive"] = transparentBrush;
        appResources["MahApps.Brushes.DataGrid.Selection.BorderBrush.MouseOver"] = transparentBrush;

        appResources["MahApps.Brushes.DataGrid.Selection.Text"] = foregroundBrush;
        appResources["MahApps.Brushes.DataGrid.Selection.Text.Inactive"] = foregroundBrush;
        appResources["MahApps.Brushes.DataGrid.Selection.Text.MouseOver"] = foregroundBrush;
    }

    /// <summary>
    ///     Set the status colors (success, warning, error) for a dark or light theme (also custom themes, based on their
    ///     base color scheme). The dark colors are the original colors of the application, the light ones are darker
    ///     to be readable on a white background (contrast ≥ 4:1).
    /// </summary>
    /// <param name="theme">Theme that was applied.</param>
    private static void ApplyStatusBrushes(Theme theme)
    {
        var isDark = theme.BaseColorScheme == ThemeManager.BaseColorDark;

        var appResources = System.Windows.Application.Current.Resources;

        appResources[StatusSuccessBrushKey] = CreateFrozenBrush(isDark ? "#badc58" : "#558b16");
        appResources[StatusWarningBrushKey] = CreateFrozenBrush(isDark ? "#f1b856" : "#b06d00");
        appResources[StatusErrorBrushKey] = CreateFrozenBrush("#b95353");
    }

    /// <summary>
    ///     Blend two colors (opaque result), e.g. a tint of the foreground on the background.
    /// </summary>
    /// <param name="background">Background color.</param>
    /// <param name="foreground">Foreground color.</param>
    /// <param name="amount">Amount of the foreground (0 - 1).</param>
    /// <returns>Blended color.</returns>
    private static Color Blend(Color background, Color foreground, double amount)
    {
        return Color.FromRgb(
            (byte)Math.Round(background.R + (foreground.R - background.R) * amount),
            (byte)Math.Round(background.G + (foreground.G - background.G) * amount),
            (byte)Math.Round(background.B + (foreground.B - background.B) * amount));
    }

    /// <summary>
    ///     Create a frozen brush from a color string (e.g. "#badc58").
    /// </summary>
    /// <param name="color">Color as string.</param>
    /// <returns>Frozen brush.</returns>
    private static SolidColorBrush CreateFrozenBrush(string color)
    {
        return CreateFrozenBrush((Color)ColorConverter.ConvertFromString(color), 1);
    }

    /// <summary>
    ///     Create a frozen brush with the given color and opacity.
    /// </summary>
    /// <param name="color">Color of the brush.</param>
    /// <param name="opacity">Opacity of the brush (0 - 1).</param>
    /// <returns>Frozen brush.</returns>
    private static SolidColorBrush CreateFrozenBrush(Color color, double opacity)
    {
        var brush = new SolidColorBrush(color) { Opacity = opacity };
        brush.Freeze();

        return brush;
    }

    /// <summary>
    ///     Apply the brush overrides of the built-in light / dark theme (not for custom themes).
    /// </summary>
    /// <param name="theme">Theme name as "theme.accent".</param>
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
        ApplyControlStateBrushes(e.NewTheme);
        ApplyStatusBrushes(e.NewTheme);
        ApplyCustomThemeFixes(e.NewTheme.Name);
    }
    #endregion
}
