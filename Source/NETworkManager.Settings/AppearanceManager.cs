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
    ///     Key of the layer that fades in on mouse over above the background of a (secondary) button.
    /// </summary>
    public const string ButtonMouseOverOverlayBrushKey = "Brushes.Button.MouseOverOverlay";

    /// <summary>
    ///     Key of the layer that fades in while a button / control is pressed (above the mouse over layer).
    /// </summary>
    public const string ControlPressedOverlayBrushKey = "Brushes.Control.PressedOverlay";

    /// <summary>
    ///     Resource key of the background of the main window frame around and between the boxes (navigation, main view,
    ///     profiles).
    /// </summary>
    public const string SurfaceFrameBrushKey = "Brushes.Surface.Frame";

    /// <summary>
    ///     Resource key of the border of the boxes (main view, profiles, ...) and cards (e.g. Dashboard widgets).
    /// </summary>
    public const string SurfaceBorderBrushKey = "Brushes.Surface.Border";

    /// <summary>
    ///     Resource key of the background of the selected item of the navigation (tint of the accent).
    /// </summary>
    public const string NavigationSelectedBrushKey = "Brushes.Navigation.Selected";

    /// <summary>
    ///     Resource key of the background of the windows with a frame (FrameWindow: main window, tab window, status
    ///     window; window, title bar, navigation): the frame color, with
    ///     <see cref="SettingsInfo.Appearance_AccentGlowIntensity" /> a subtle glow of the accent in the top left corner.
    /// </summary>
    public const string SurfaceWindowBrushKey = "Brushes.Surface.Window";

    /// <summary>
    ///     Resource key of the background of an inactive window (half of the glow), see <see cref="SurfaceWindowBrushKey" />.
    /// </summary>
    public const string SurfaceWindowInactiveBrushKey = "Brushes.Surface.Window.Inactive";

    /// <summary>
    ///     Resource key of the background of the run command (with the accent glow a glow along the top edge).
    /// </summary>
    public const string CommandPaletteBackgroundBrushKey = "Brushes.CommandPalette.Background";

    private static Color _frameColor = Colors.White;
    private static Color _backgroundColor = Colors.White;
    private static Color _accentColor = Colors.Gray;

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
    /// <summary>
    ///     Whether animations are shown (e.g. when a dialog is opened or closed): the system-wide policy
    ///     (<see cref="PolicyInfo.Appearance_Animations" />), otherwise the setting; <see cref="AnimationMode.System" />
    ///     follows the Windows setting "Animation effects". Read each time an animation starts, changes apply
    ///     immediately.
    /// </summary>
    public static bool AnimationsEnabled =>
        PolicyManager.Current?.Appearance_Animations ?? SettingsManager.Current.Appearance_Animations switch
        {
            AnimationMode.On => true,
            AnimationMode.Off => false,
            _ => SystemParameters.ClientAreaAnimation
        };

    static AppearanceManager()
    {
        // Themes of MahApps (Light, Dark) and the additional themes (Black, Midnight, ...), created when used; sorted
        // from light to dark (by the brightness of the background), so the light and dark themes are next to each other
        Themes = [.. ThemeManager.Current.Themes
            .GroupBy(x => x.BaseColorScheme)
            .Select(x => x.First())
            .Select(x => new ThemeColorInfo
            { Name = x.BaseColorScheme, Color = x.Resources["MahApps.Brushes.ThemeBackground"] as Brush })
            .Concat(ThemeVariants.Themes.Select(x => new ThemeColorInfo { Name = x.Name, Color = CreateFrozenBrush(x.Background, 1) }))
            .OrderByDescending(x => x.Color is SolidColorBrush { Color: var color }
                ? 0.2126 * color.R + 0.7152 * color.G + 0.0722 * color.B
                : 0)];

        // Accents of MahApps and the additional accents (Sky, Ocean, ...), sorted by name
        Accents = [.. ThemeManager.Current.Themes
            .GroupBy(x => x.ColorScheme)
            .Select(x => new AccentColorInfo { Name = x.Key, Color = x.First().ShowcaseBrush })
            .Concat(ThemeVariants.Accents.Select(x => new AccentColorInfo { Name = x.Name, Color = CreateFrozenBrush(x.Color, 1) }))
            .OrderBy(x => x.Name)];

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
    ///     Sets the background of the main window (<see cref="SurfaceWindowBrushKey" />): the frame color, or with the
    ///     accent glow (Settings > Appearance) a radial gradient from 6 / 10 / 15 % of the accent (intensity) in the top
    ///     left corner to the frame color (about 700 x 500 px). Absolute coordinates: the window, the title bar and the navigation (all
    ///     starting near the top left corner of the window) show one continuous gradient. Called on every theme change
    ///     and when the setting is changed.
    /// </summary>
    /// <summary>
    ///     Amount of the accent in the glow (0 = off).
    /// </summary>
    private static double GetAccentGlowAmount() => SettingsManager.Current?.Appearance_AccentGlowIntensity switch
    {
        AccentGlowIntensity.Subtle => 0.06,
        AccentGlowIntensity.Medium => 0.10,
        AccentGlowIntensity.Strong => 0.15,
        _ => 0
    };

    public static void ApplyWindowBrush()
    {
        var amount = GetAccentGlowAmount();

        var resources = System.Windows.Application.Current.Resources;

        resources[SurfaceWindowBrushKey] = CreateWindowBrush(amount);

        // Inactive window: half of the glow (like the title bar of an inactive window in Windows)
        resources[SurfaceWindowInactiveBrushKey] = CreateWindowBrush(amount / 2);

        resources[CommandPaletteBackgroundBrushKey] = CreateCommandPaletteBrush(amount);
    }

    /// <summary>
    ///     Background of the run command (command palette): the background of the dialogs, with the accent glow a soft
    ///     glow of the accent along the top edge (2.5 x the amount of the glow, e.g. 25 % with Medium) that fades out
    ///     within 40 px (absolute, independent of the height of the dialog).
    /// </summary>
    private static Brush CreateCommandPaletteBrush(double amount)
    {
        if (amount <= 0)
            return CreateFrozenBrush(_backgroundColor, 1);

        var gradient = new LinearGradientBrush
        {
            MappingMode = BrushMappingMode.Absolute,
            StartPoint = new System.Windows.Point(0, 0),
            EndPoint = new System.Windows.Point(0, 40)
        };

        gradient.GradientStops.Add(new GradientStop(Blend(_backgroundColor, _accentColor, Math.Min(1, amount * 2.5)), 0));
        gradient.GradientStops.Add(new GradientStop(_backgroundColor, 1));
        gradient.Freeze();

        return gradient;
    }

    /// <summary>
    ///     The frame color, with a glow of the accent (amount > 0) in the top left corner.
    /// </summary>
    private static Brush CreateWindowBrush(double amount)
    {
        if (amount <= 0)
            return CreateFrozenBrush(_frameColor, 1);

        var gradient = new RadialGradientBrush
        {
            MappingMode = BrushMappingMode.Absolute,
            Center = new System.Windows.Point(0, 0),
            GradientOrigin = new System.Windows.Point(0, 0),
            RadiusX = 700,
            RadiusY = 500
        };

        gradient.GradientStops.Add(new GradientStop(Blend(_frameColor, _accentColor, amount), 0));
        gradient.GradientStops.Add(new GradientStop(_frameColor, 1));
        gradient.Freeze();

        return gradient;
    }

    /// <summary>
    ///     Method to change the application theme (and accent).
    /// </summary>
    /// <param name="name">Theme name as "theme.accent" to apply.</param>
    public static void ChangeTheme(string name)
    {
        // Additional themes / accents are created when they are used; if that fails (or the theme is unknown), fall back
        // to the default theme instead of an exception
        if (!ThemeVariants.EnsureTheme(name) || ThemeManager.Current.GetTheme(name) == null)
            name = $"{GlobalStaticConfiguration.Appearance_Theme}.{GlobalStaticConfiguration.Appearance_Accent}";

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
    ///     (secondary) buttons, the frame of the window and the border of inputs for mouse over: the foreground color of
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

        // Layers above the background of the buttons that fade in (DefaultButton): on the button background (6 %) the
        // mouse over layer gives 10 %, with the pressed layer 14 % (on a transparent icon button with the 8 % mouse
        // over layer about 12 %) - the same colors as the backgrounds above
        appResources[ButtonMouseOverOverlayBrushKey] = CreateFrozenBrush(foreground, 1 - 0.90 / 0.94);
        appResources[ControlPressedOverlayBrushKey] = CreateFrozenBrush(foreground, 1 - 0.86 / 0.90);

        // Opaque (window background) around the boxes (theme background): the boxes look raised on the frame like in
        // Visual Studio / Windows 11. Dark: the background 25 % darker (MahApps dark: #1C1C1C frame, #252525 box);
        // light: 4 % of the foreground on the background (light gray frame, white boxes).
        var background = theme.Resources["MahApps.Colors.ThemeBackground"] is Color backgroundColor
            ? backgroundColor
            : Colors.White;

        // Additional themes can have an own frame (e.g. the light blue-gray of Mist)
        var frame = ThemeVariants.GetFrame(theme) ?? (ThemeVariants.IsDark(theme)
            ? Blend(background, Colors.Black, 0.25)
            : Blend(background, foreground, 0.04));

        appResources[SurfaceFrameBrushKey] = CreateFrozenBrush(frame, 1);

        // Border of the boxes and cards: a low opacity of the foreground (softer than Gray8 in the light theme, the
        // contrast comes from the frame)
        appResources[SurfaceBorderBrushKey] = CreateFrozenBrush(foreground, 0.10);

        // Selected item of the navigation: a light tint of the accent (mouse over stays the neutral tint)
        var accent = theme.Resources["MahApps.Colors.Accent"] is Color accentColor ? accentColor : Colors.Gray;
        appResources[NavigationSelectedBrushKey] = CreateFrozenBrush(accent, 0.15);

        _frameColor = frame;
        _backgroundColor = background;
        _accentColor = accent;
        ApplyWindowBrush();

        ApplyDataGridBrushes(theme, foreground);
        ApplyToggleSwitchDisabledBrushes(theme);
    }

    /// <summary>
    ///     Disabled toggle switches keep their normal colors (MahApps: gray) and are dimmed by the style
    ///     (<c>DefaultToggleSwitch</c>), like the inputs and buttons. The template of MahApps uses the disabled brushes
    ///     via DynamicResource, so they are replaced by the normal brushes of the theme.
    /// </summary>
    /// <param name="theme">Theme that was applied.</param>
    private static void ApplyToggleSwitchDisabledBrushes(Theme theme)
    {
        var appResources = System.Windows.Application.Current.Resources;

        foreach (var name in ToggleSwitchBrushNames)
        {
            var key = $"MahApps.Brushes.ToggleSwitch.{name}";

            if (theme.Resources[key] is Brush brush)
                appResources[$"{key}Disabled"] = brush;
        }
    }

    /// <summary>
    ///     Brushes of the toggle switch (MahApps) that have a disabled variant (<c>...Disabled</c>).
    /// </summary>
    private static readonly string[] ToggleSwitchBrushNames =
    [
        "ContainerBackground", "ContentForeground", "HeaderForeground", "FillOff", "FillOn", "StrokeOff", "StrokeOn",
        "KnobFillOff", "KnobFillOn"
    ];

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
        var isDark = ThemeVariants.IsDark(theme);

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
