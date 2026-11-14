using System.Collections.Generic;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System.Windows.Media;
using NETworkManager.Utilities;

namespace NETworkManager.Controls;

/// <summary>
///     One channel chart of the WiFi tool (2.4 GHz, 5 GHz, 6 GHz lower / upper): the axes and signal-quality sections
///     (fixed) and the series and legend of the networks (replaced on every scan).
/// </summary>
/// <param name="xAxes">X axis (channels of the band).</param>
/// <param name="yAxes">Y axis (signal strength).</param>
/// <param name="sections">Signal-quality bands in the background.</param>
public class WiFiChannelChart(Axis[] xAxes, Axis[] yAxes, RectangularSection[] sections) : PropertyChangedBase
{
    public Axis[] XAxes { get; } = xAxes;

    public Axis[] YAxes { get; } = yAxes;

    public RectangularSection[] Sections { get; } = sections;

    /// <summary>
    ///     Gets one series per network (a trapezoid over its channel width).
    /// </summary>
    public ISeries[] Series
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    /// <summary>
    ///     Gets the legend below the chart (color, SSID and BSSID of each network).
    /// </summary>
    public WiFiChannelLegendEntry[] Legend
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    /// <summary>
    ///     Shows the networks of a scan.
    /// </summary>
    /// <param name="series">One series per network.</param>
    /// <param name="legend">Legend entry of each series (same order).</param>
    public void Update(IEnumerable<ISeries> series, IEnumerable<WiFiChannelLegendEntry> legend)
    {
        Series = [.. series];
        Legend = [.. legend];
    }

    /// <summary>
    ///     Removes the networks.
    /// </summary>
    public void Clear()
    {
        Update([], []);
    }
}

/// <summary>
///     Legend entry of a network below a channel chart.
/// </summary>
/// <param name="Color">Color of the series of the network.</param>
/// <param name="Ssid">Shown SSID ("Hidden Network" for a hidden network).</param>
/// <param name="Bssid">BSSID of the network.</param>
/// <param name="ColorIndex">Index of the color in the palette (to set the color again after a theme change).</param>
public record WiFiChannelLegendEntry(SolidColorBrush Color, string Ssid, string Bssid, int ColorIndex);
