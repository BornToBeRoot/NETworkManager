using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class SettingsNetworkView
    {
        private readonly SettingsNetworkViewModel _viewModel = new();

        public SettingsNetworkView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }
    }
}
