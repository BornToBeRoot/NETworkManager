using ControlzEx.Theming;
using log4net;
using MahApps.Metro.Theming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace NETworkManager.Settings;

/// <summary>
///     Additional themes (Black, Midnight, Steel, Indigo) and accents (Sky, Ocean, ...) of the application, in addition
///     to the themes (Light, Dark) and accents of MahApps.
///
///     A theme is a copy of a MahApps theme with recolored resources (created when it is used, see
///     <see cref="EnsureTheme" />):
///     <list type="bullet">
///         <item>Theme: the dark / light theme with the same accent, the neutral grays (backgrounds, borders, disabled text) get the
///         background of the theme - dark grays fully, lighter grays less, the text stays neutral. The accent colors are not
///         changed.</item>
///         <item>Accent: the theme with the accent Blue, the colors of the accent (all opacities) and the highlight are
///         replaced.</item>
///     </list>
/// </summary>
public static class ThemeVariants
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(ThemeVariants));

    /// <summary>
    ///     Additional theme: name, background of the boxes, the theme of MahApps it is based on (Dark or Light) and an
    ///     optional color of the frame (otherwise computed from the background, see <see cref="AppearanceManager" />).
    /// </summary>
    public sealed record ThemeVariant(string Name, Color Background, string SourceBaseColorScheme, Color? Frame = null);

    /// <summary>
    ///     Additional themes: dark (Black, Midnight, Steel, Indigo) and dimmed light themes (Mist like the Blue theme
    ///     of Visual Studio, Soft, Paper) with an own, clearly visible frame.
    /// </summary>
    public static readonly IReadOnlyList<ThemeVariant> Themes =
    [
        new("Black", Color.FromRgb(0x14, 0x14, 0x14), ThemeManager.BaseColorDark),
        new("Midnight", Color.FromRgb(0x1C, 0x22, 0x30), ThemeManager.BaseColorDark),
        new("Steel", Color.FromRgb(0x2A, 0x30, 0x38), ThemeManager.BaseColorDark),
        new("Indigo", Color.FromRgb(0x1F, 0x1C, 0x2E), ThemeManager.BaseColorDark),
        new("Mist", Color.FromRgb(0xF2, 0xF4, 0xF9), ThemeManager.BaseColorLight, Color.FromRgb(0xD6, 0xDB, 0xE9)),
        new("Soft", Color.FromRgb(0xEC, 0xEC, 0xEC), ThemeManager.BaseColorLight, Color.FromRgb(0xDA, 0xDA, 0xDA)),
        new("Paper", Color.FromRgb(0xF5, 0xF2, 0xEA), ThemeManager.BaseColorLight, Color.FromRgb(0xE3, 0xDE, 0xD3))
    ];

    /// <summary>
    ///     Additional accents (white text on them, like the accents of MahApps).
    /// </summary>
    public static readonly IReadOnlyList<(string Name, Color Color)> Accents =
    [
        ("Sky", Color.FromRgb(0x2E, 0x9C, 0xDB)),
        ("Ocean", Color.FromRgb(0x0E, 0x7C, 0x86)),
        ("Mint", Color.FromRgb(0x2A, 0xA8, 0x86)),
        ("Rose", Color.FromRgb(0xD9, 0x4F, 0x82)),
        ("Coral", Color.FromRgb(0xE8, 0x65, 0x4E)),
        ("Gold", Color.FromRgb(0xC9, 0x96, 0x0C)),
        ("Lavender", Color.FromRgb(0x8E, 0x72, 0xD6)),
        ("Slate", Color.FromRgb(0x5A, 0x76, 0x8F)),
        // Neutral gray for a colorless look (slightly cool: not treated as a neutral gray of a tinted theme)
        ("Graphite", Color.FromRgb(0x6E, 0x76, 0x81))
    ];

    // Source of an accent (the accent Blue of MahApps)
    private const string SourceColorScheme = "Blue";

    // Neutral gray: difference of the channels up to this value
    private const int NeutralTolerance = 8;

    // Grays from this luminance (light grays, text) are not tinted
    private const double TintEndLuminance = 0xC0;

    /// <summary>
    ///     Creates and registers the theme (e.g. "Midnight.Teal", "Dark.Sky") in the <see cref="ThemeManager" /> if it
    ///     is an additional theme or accent and doesn't exist yet.
    /// </summary>
    /// <param name="name">Theme name as "theme.accent".</param>
    /// <returns><see langword="false" /> if the theme could not be created (the caller falls back to a theme of MahApps).</returns>
    public static bool EnsureTheme(string name)
    {
        try
        {
            CreateTheme(name);

            return true;
        }
        catch (Exception ex)
        {
            Log.Error($"Could not create the theme \"{name}\".", ex);

            return false;
        }
    }

    private static void CreateTheme(string name)
    {
        if (string.IsNullOrEmpty(name) || ThemeManager.Current.GetTheme(name) != null)
            return;

        var parts = name.Split('.');

        if (parts.Length != 2)
            return;

        var (baseColorScheme, colorScheme) = (parts[0], parts[1]);

        var theme = Themes.FirstOrDefault(x => x.Name == baseColorScheme);
        var accent = Accents.FirstOrDefault(x => x.Name == colorScheme);

        var isTheme = theme != null;
        var isAccent = accent.Name != null;

        if (!isTheme && !isAccent)
            return;

        var source = ThemeManager.Current.GetTheme(
            $"{(isTheme ? theme.SourceBaseColorScheme : baseColorScheme)}.{(isAccent ? SourceColorScheme : colorScheme)}");

        if (source == null)
            return;

        var resources = Copy(source);

        if (isAccent)
            RecolorAccent(resources, accent.Color);

        if (isTheme)
            RecolorBase(resources, theme.Background);

        resources["Theme.Name"] = name;
        resources["Theme.DisplayName"] = $"{colorScheme} ({baseColorScheme})";
        resources["Theme.BaseColorScheme"] = baseColorScheme;
        resources["Theme.ColorScheme"] = colorScheme;
        resources["Theme.Origin"] = "NETworkManager";

        ThemeManager.Current.AddLibraryTheme(new LibraryTheme(resources, MahAppsLibraryThemeProvider.DefaultInstance));
    }

    /// <summary>
    ///     Own frame color of an additional theme (e.g. the light blue-gray of Mist), otherwise <see langword="null" />.
    /// </summary>
    public static Color? GetFrame(Theme theme) =>
        Themes.FirstOrDefault(x => x.Name == theme.BaseColorScheme)?.Frame;

    /// <summary>
    ///     <see langword="true" /> if the theme has a dark background (the additional themes are dark, also custom
    ///     themes are detected by their background).
    /// </summary>
    public static bool IsDark(Theme theme) =>
        theme.Resources["MahApps.Colors.ThemeBackground"] is not Color background || IsDark(background);

    /// <summary>
    ///     <see langword="true" /> if the background color is dark (luminance below the middle). The one rule for light /
    ///     dark in the application (themes, overrides, charts).
    /// </summary>
    public static bool IsDark(Color background) => Luminance(background) < 128;

    private static ResourceDictionary Copy(Theme source)
    {
        var copy = new ResourceDictionary();

        foreach (var dictionary in source.GetAllResources())
        foreach (var key in dictionary.Keys)
            copy[key] = dictionary[key];

        return copy;
    }

    private static void RecolorAccent(ResourceDictionary resources, Color accent)
    {
        if (resources["MahApps.Colors.AccentBase"] is not Color sourceAccent)
            return;

        var sourceHighlight = resources["MahApps.Colors.Highlight"] as Color?;

        // Highlight (e.g. pressed): a bit darker than the accent
        var highlight = Blend(accent, Colors.Black, 0.2);

        Recolor(resources, color =>
        {
            if (SameRgb(color, sourceAccent))
                return WithAlpha(accent, color.A);

            if (sourceHighlight != null && SameRgb(color, sourceHighlight.Value))
                return WithAlpha(highlight, color.A);

            return color;
        });
    }

    private static void RecolorBase(ResourceDictionary resources, Color background)
    {
        if (resources["MahApps.Colors.ThemeBackground"] is not Color sourceBackground)
            return;

        var sourceLuminance = Luminance(sourceBackground);

        Recolor(resources, color =>
        {
            if (Math.Max(color.R, Math.Max(color.G, color.B)) - Math.Min(color.R, Math.Min(color.G, color.B)) >
                NeutralTolerance)
                return color;

            var luminance = Luminance(color);

            // Weight of the tint: full at the background, less for darker (black stays black) and lighter grays
            var weight = luminance <= sourceLuminance
                ? sourceLuminance <= 0 ? 1 : luminance / sourceLuminance
                : Math.Clamp(1 - (luminance - sourceLuminance) / (TintEndLuminance - sourceLuminance), 0, 1);

            if (weight <= 0)
                return color;

            return Color.FromArgb(color.A,
                Shift(color.R, background.R - sourceBackground.R, weight),
                Shift(color.G, background.G - sourceBackground.G, weight),
                Shift(color.B, background.B - sourceBackground.B, weight));
        });
    }

    /// <summary>
    ///     Replaces the colors of all colors and brushes (solid and gradient) in the resources.
    /// </summary>
    private static void Recolor(ResourceDictionary resources, Func<Color, Color> map)
    {
        foreach (var key in resources.Keys.Cast<object>().ToList())
        {
            switch (resources[key])
            {
                case Color color:
                    resources[key] = map(color);
                    break;
                case SolidColorBrush brush:
                    var solid = new SolidColorBrush(map(brush.Color)) { Opacity = brush.Opacity };
                    solid.Freeze();
                    resources[key] = solid;
                    break;
                case GradientBrush gradient:
                    var clone = gradient.CloneCurrentValue();

                    foreach (var stop in clone.GradientStops)
                        stop.Color = map(stop.Color);

                    clone.Freeze();
                    resources[key] = clone;
                    break;
            }
        }
    }

    private static byte Shift(byte value, int delta, double weight) =>
        (byte)Math.Clamp(Math.Round(value + delta * weight), 0, 255);

    private static bool SameRgb(Color a, Color b) => a.R == b.R && a.G == b.G && a.B == b.B;

    private static Color WithAlpha(Color color, byte alpha) => Color.FromArgb(alpha, color.R, color.G, color.B);

    /// <summary>
    ///     Relative luminance of a color (0 - 255, Rec. 709 weights), e.g. to sort the themes from light to dark.
    /// </summary>
    public static double Luminance(Color color) => 0.2126 * color.R + 0.7152 * color.G + 0.0722 * color.B;

    /// <summary>
    ///     Mixes a color with a target color.
    /// </summary>
    /// <param name="color">Color to start from.</param>
    /// <param name="target">Color to mix in.</param>
    /// <param name="amount">Share of the target color (0 - 1).</param>
    internal static Color Blend(Color color, Color target, double amount) => Color.FromRgb(
        (byte)Math.Round(color.R + (target.R - color.R) * amount),
        (byte)Math.Round(color.G + (target.G - color.G) * amount),
        (byte)Math.Round(color.B + (target.B - color.B) * amount));
}
