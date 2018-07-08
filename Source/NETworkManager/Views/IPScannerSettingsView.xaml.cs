using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class IPScannerSettingsView
    {
        private readonly IPScannerSettingsViewModel _viewModel = new IPScannerSettingsViewModel();

        public IPScannerSettingsView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }
    }
}
