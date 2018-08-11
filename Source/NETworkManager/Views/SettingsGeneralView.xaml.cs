using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class SettingsGeneralView
    {
        private readonly SettingsGeneralViewModel _viewModel = new SettingsGeneralViewModel();

        public SettingsGeneralView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }
    }
}
