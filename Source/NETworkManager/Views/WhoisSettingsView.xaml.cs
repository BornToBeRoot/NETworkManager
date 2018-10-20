using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class WhoisSettingsView
    {
        private readonly WhoisSettingsViewModel _viewModel = new WhoisSettingsViewModel();

        public WhoisSettingsView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }
    }
}
