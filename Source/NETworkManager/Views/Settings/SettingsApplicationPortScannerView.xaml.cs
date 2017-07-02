using NETworkManager.ViewModels.Settings;
using System.Windows.Controls;

namespace NETworkManager.Views.Settings
{
    public partial class SettingsApplicationPortScannerView : UserControl
    {
        SettingsApplicationPortScannerViewModel viewModel = new SettingsApplicationPortScannerViewModel();

        public SettingsApplicationPortScannerView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
