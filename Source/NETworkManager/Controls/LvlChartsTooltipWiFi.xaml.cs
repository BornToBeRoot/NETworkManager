using LiveCharts;
using LiveCharts.Wpf;
using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace NETworkManager.Controls
{
    /// <summary>
    /// Interaction logic for LvlChartsTooltipWiFi.xaml
    /// </summary>
    public partial class LvlChartsTooltipWiFi : IChartTooltip
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
      
        private TooltipData _data;

        public TooltipData Data
        {
            get { return _data; }
            set
            {
                _data = value;                                
                OnPropertyChanged();
            }
        }

        public TooltipSelectionMode? SelectionMode { get; set; }
                
        public LvlChartsTooltipWiFi()
        {
            InitializeComponent();

            DataContext = this;
        }        
    }
}
