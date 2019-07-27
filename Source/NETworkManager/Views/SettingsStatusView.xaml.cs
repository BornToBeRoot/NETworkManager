using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class SettingsStatusView
    {
        private readonly SettingsStatusViewModel _viewModel = new SettingsStatusViewModel();

        public SettingsStatusView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }
    }
}
