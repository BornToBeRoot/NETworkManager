using System.ComponentModel;
using System.Runtime.CompilerServices;
using LiveCharts;
using LiveCharts.Wpf;

namespace NETworkManager.Controls;

public partial class LvlChartsPingTimeTooltip : IChartTooltip
{
    public LvlChartsPingTimeTooltip()
    {
        InitializeComponent();

        DataContext = this;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public TooltipData Data
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public TooltipSelectionMode? SelectionMode { get; set; }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}