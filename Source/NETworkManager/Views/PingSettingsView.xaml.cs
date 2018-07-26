using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class PingSettingsView
    {
        private readonly PingSettingsViewModel _viewModel = new PingSettingsViewModel();

        public PingSettingsView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }
    }
}
