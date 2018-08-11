using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class WakeOnLANSettingsView
    {
        private readonly WakeOnLANSettingsViewModel _viewModel = new WakeOnLANSettingsViewModel();

        public WakeOnLANSettingsView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }
    }
}
