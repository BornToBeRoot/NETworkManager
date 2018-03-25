using NETworkManager.ViewModels;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class PortScannerSettingsView : UserControl
    {
        PortScannerSettingsViewModel viewModel = new PortScannerSettingsViewModel();

        public PortScannerSettingsView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
