using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class WebConsoleSettingsView
    {
        private readonly WebConsoleSettingsViewModel _viewModel = new WebConsoleSettingsViewModel();

        public WebConsoleSettingsView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }
    }
}
