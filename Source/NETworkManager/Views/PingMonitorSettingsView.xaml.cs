using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class PingMonitorSettingsView
    {
        private readonly PingMonitorSettingsViewModel _viewModel = new PingMonitorSettingsViewModel();

        public PingMonitorSettingsView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }
    }
}
