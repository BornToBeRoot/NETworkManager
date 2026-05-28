using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace NETworkManager.Controls;

public partial class LiveChartsSpeedTestTooltip : IChartTooltip
{
    private readonly Popup _popup;

    public LiveChartsSpeedTestTooltip()
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

    public ObservableCollection<TooltipEntry> TooltipEntries { get; } = [];

    public void Show(IEnumerable<ChartPoint> tooltipPoints, Chart chart)
    {
        var points = tooltipPoints.ToList();
        if (points.Count == 0) return;

        TooltipEntries.Clear();
        foreach (var point in points)
        {
            var value = point.Context.DataSource is double mbps ? $"{mbps:F1} Mbps" : "-/-";
            TooltipEntries.Add(new TooltipEntry(
                SkColorToBrush(GetSeriesColor(point)),
                value,
                point.Context.Series.Name ?? string.Empty));
        }

        _popup.PlacementTarget = chart.View as FrameworkElement;
        _popup.IsOpen = true;
    }

    public void Hide(Chart chart)
    {
        _popup.IsOpen = false;
    }

    private static SKColor GetSeriesColor(ChartPoint point)
    {
        return point.Context.Series is LineSeries<double> { Stroke: SolidColorPaint paint } ? paint.Color : SKColors.Gray;
    }

    private static Brush SkColorToBrush(SKColor color) => new SolidColorBrush(Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue));

    public record TooltipEntry(Brush SeriesColor, string Value, string SeriesName);
}
