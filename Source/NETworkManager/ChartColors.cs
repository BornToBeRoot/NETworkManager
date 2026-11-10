using System.Windows;
using System.Windows.Media;
using LiveChartsCore.Painting;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace NETworkManager;

/// <summary>
/// Colors of the charts and maps (Ping Monitor, bandwidth of the Network Interface, speed test, WiFi channels,
/// cities of the traceroute map) for the current theme: the same hues in both themes, a bit brighter on the dark and
/// darker on the light background for contrast. Read when a chart is created.
/// </summary>
public static class ChartColors
{
    // Dark theme: brighter than the former fixed colors (#1ba1e2, #7fba00); light theme: darker for the white
    // background
    private static readonly SKColor PrimaryDark = SKColor.Parse("#3aaee8");
    private static readonly SKColor PrimaryLight = SKColor.Parse("#0a76b5");
    private static readonly SKColor SecondaryDark = SKColor.Parse("#8cc63f");
    private static readonly SKColor SecondaryLight = SKColor.Parse("#4a8a00");

    // One color per network (WiFi channels), distinct hues
    private static readonly SKColor[] PaletteDark =
    [
        SKColor.Parse("#3aaee8"), SKColor.Parse("#b5d335"), SKColor.Parse("#f5ad2a"),
        SKColor.Parse("#ff5c4d"), SKColor.Parse("#8f5cff"), SKColor.Parse("#1ec2bf"),
        SKColor.Parse("#ff4fa3"), SKColor.Parse("#7ccf3a"), SKColor.Parse("#ff8a3d"),
        SKColor.Parse("#5a8cff"), SKColor.Parse("#c25cff"), SKColor.Parse("#c08a52")
    ];

    private static readonly SKColor[] PaletteLight =
    [
        SKColor.Parse("#0a76b5"), SKColor.Parse("#6f8a00"), SKColor.Parse("#b8740a"),
        SKColor.Parse("#c41200"), SKColor.Parse("#5a00d6"), SKColor.Parse("#008a88"),
        SKColor.Parse("#b8005f"), SKColor.Parse("#3f8a10"), SKColor.Parse("#c95300"),
        SKColor.Parse("#0042c4"), SKColor.Parse("#8a00cc"), SKColor.Parse("#6b4824")
    ];

    /// <summary>
    /// <see langword="true"/> if the current theme has a dark background.
    /// </summary>
    private static bool IsDark =>
        Application.Current?.TryFindResource("MahApps.Colors.ThemeBackground") is not Color background ||
        0.2126 * background.R + 0.7152 * background.G + 0.0722 * background.B < 128;

    /// <summary>
    /// Main series (ping time, download, cities of the traceroute map).
    /// </summary>
    public static SKColor Primary => IsDark ? PrimaryDark : PrimaryLight;

    /// <summary>
    /// Second series (upload).
    /// </summary>
    public static SKColor Secondary => IsDark ? SecondaryDark : SecondaryLight;

    /// <summary>
    /// Color of the series with the index (e.g. a WiFi network), repeats after 12 colors.
    /// </summary>
    public static SKColor GetPaletteColor(int index)
    {
        var palette = IsDark ? PaletteDark : PaletteLight;

        return palette[index % palette.Length];
    }

    /// <summary>
    /// <see cref="Primary"/> as WPF color (e.g. the traceroute map).
    /// </summary>
    public static Color PrimaryWpf => Color.FromRgb(Primary.Red, Primary.Green, Primary.Blue);

    /// <summary>
    /// Fill below a line: from 33 % of the line color at the top to transparent at the bottom.
    /// </summary>
    public static Paint AreaFill(SKColor color) =>
        new LinearGradientPaint([color.WithAlpha(0x55), color.WithAlpha(0x00)], new SKPoint(0.5f, 0),
            new SKPoint(0.5f, 1));
}
