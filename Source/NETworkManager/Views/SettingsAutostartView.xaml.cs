using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class SettingsAutostartView
    {
        private readonly SettingsAutostartViewModel _viewModel = new SettingsAutostartViewModel(DialogCoordinator.Instance);

        public SettingsAutostartView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }
    }
}
