using NETworkManager.ViewModels;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class IPScannerSettingsView : UserControl
    {
        IPScannerSettingsViewModel viewModel = new IPScannerSettingsViewModel();

        public IPScannerSettingsView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
