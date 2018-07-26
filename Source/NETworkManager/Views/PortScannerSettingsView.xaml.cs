using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class PortScannerSettingsView
    {
        private readonly PortScannerSettingsViewModel _viewModel = new PortScannerSettingsViewModel();

        public PortScannerSettingsView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }
    }
}
