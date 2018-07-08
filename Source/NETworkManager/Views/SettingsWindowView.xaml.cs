using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class SettingsWindowView
    {
        private readonly SettingsWindowViewModel _viewModel = new SettingsWindowViewModel();

        public SettingsWindowView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }
    }
}
