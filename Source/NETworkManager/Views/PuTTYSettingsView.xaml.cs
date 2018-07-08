using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class PuTTYSettingsView
    {
        private readonly PuTTYSettingsViewModel _viewModel = new PuTTYSettingsViewModel(DialogCoordinator.Instance);

        public PuTTYSettingsView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }
    }
}
