using System.ComponentModel;
using System.Runtime.CompilerServices;
using LiveCharts;
using LiveCharts.Wpf;

namespace NETworkManager.Controls;

public partial class LvlChartsBandwidthTooltip : IChartTooltip
{
    private TooltipData _data;

    public LvlChartsBandwidthTooltip()
    {
        InitializeComponent();

        DataContext = this;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public TooltipData Data
    {
        get => _data;
        set
        {
            _data = value;
            OnPropertyChanged();
        }
    }

    public TooltipSelectionMode? SelectionMode { get; set; }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}