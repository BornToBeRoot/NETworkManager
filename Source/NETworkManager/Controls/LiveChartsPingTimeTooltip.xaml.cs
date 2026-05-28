using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using NETworkManager.Utilities;
using SkiaSharp;

namespace NETworkManager.Controls;

public partial class LiveChartsPingTimeTooltip : IChartTooltip, INotifyPropertyChanged
{
    private readonly Popup _popup;

    public LiveChartsPingTimeTooltip()
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

    public string HeaderText
    {
        get;
        private set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<TooltipEntry> TooltipEntries { get; } = [];

    public void Show(IEnumerable<ChartPoint> tooltipPoints, Chart chart)
    {
        var points = tooltipPoints.ToList();
        if (points.Count == 0) return;

        if (points[0].Context.DataSource is LvlChartsDefaultInfo firstInfo)
            HeaderText = DateTimeHelper.DateTimeToTimeString(firstInfo.DateTime);

        TooltipEntries.Clear();
        foreach (var point in points)
        {
            var value = point.Context.DataSource is LvlChartsDefaultInfo info
                ? $"{info.Value} ms"
                : "-/-";
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

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private static SKColor GetSeriesColor(ChartPoint point)
    {
        if (point.Context.Series is LineSeries<LvlChartsDefaultInfo> ls && ls.Stroke is SolidColorPaint paint)
            return paint.Color;
        return SKColors.Gray;
    }

    private static Brush SkColorToBrush(SKColor color)
        => new SolidColorBrush(Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue));

    public record TooltipEntry(Brush SeriesColor, string Value, string SeriesName);
}
