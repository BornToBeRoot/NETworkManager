using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Network;
using SkiaSharp;

namespace NETworkManager.Controls;

public partial class LiveChartsWiFiChannelTooltip : IChartTooltip, INotifyPropertyChanged
{
    private readonly Popup _popup;

    public LiveChartsWiFiChannelTooltip()
    {
        InitializeComponent();
        DataContext = this;
        _popup = new Popup
        {
            AllowsTransparency = true,
            Placement = PlacementMode.MousePoint,
            StaysOpen = true,
            Child = this
        };
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public ObservableCollection<TooltipEntry> TooltipEntries { get; } = [];

    public void Show(IEnumerable<ChartPoint> tooltipPoints, Chart chart)
    {
        // Each network is drawn as a trapezoid (multiple points). List every network once,
        // regardless of which vertex the pointer is closest to, so the tooltip works across the
        // whole trapezoid and not only at its center.
        var seenNetworks = new HashSet<WiFiNetworkInfo>();

        TooltipEntries.Clear();

        foreach (var point in tooltipPoints)
        {
            if (point.Context.DataSource is not WiFiChannelPoint info)
                continue;

            var network = info.Network;

            if (!seenNetworks.Add(network))
                continue;

            var ssid = network.IsHidden ? Strings.HiddenNetwork : network.AvailableNetwork.Ssid;
            var detail =
                $"{network.AvailableNetwork.NetworkRssiInDecibelMilliwatts} dBm · {Strings.Channel} {network.Channel} · {network.ChannelBandwidth} MHz";

            TooltipEntries.Add(new TooltipEntry(SkColorToBrush(GetSeriesColor(point)), ssid, detail));
        }

        if (TooltipEntries.Count == 0)
        {
            _popup.IsOpen = false;
            return;
        }

        _popup.PlacementTarget = chart.View as FrameworkElement;
        _popup.IsOpen = true;
    }

    public void Hide(Chart chart)
    {
        _popup.IsOpen = false;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private static SKColor GetSeriesColor(ChartPoint point)
    {
        if (point.Context.Series is LineSeries<WiFiChannelPoint> ls && ls.Stroke is SolidColorPaint paint)
            return paint.Color;
        return SKColors.Gray;
    }

    private static Brush SkColorToBrush(SKColor color)
        => new SolidColorBrush(Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue));

    public record TooltipEntry(Brush SeriesColor, string Ssid, string Detail);
}
