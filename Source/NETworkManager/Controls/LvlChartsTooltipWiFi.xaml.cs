using LiveCharts;
using LiveCharts.Wpf;
using NETworkManager.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
