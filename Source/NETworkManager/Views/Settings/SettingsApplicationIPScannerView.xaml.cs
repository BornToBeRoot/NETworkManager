using NETworkManager.ViewModels.Settings;
using System.Windows.Controls;

namespace NETworkManager.Views.Settings
{
    public partial class SettingsApplicationIPScannerView : UserControl
    {
        SettingsApplicationIPScannerViewModel viewModel = new SettingsApplicationIPScannerViewModel();

        public SettingsApplicationIPScannerView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
