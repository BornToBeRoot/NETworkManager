using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class SNMPSettingsView
    {
        private readonly SNMPSettingsViewModel _viewModel = new SNMPSettingsViewModel();

        public SNMPSettingsView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }
    }
}
