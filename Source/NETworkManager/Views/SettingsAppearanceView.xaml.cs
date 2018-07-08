using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class SettingsAppearanceView
    {
        private readonly SettingsAppearanceViewModel _viewModel = new SettingsAppearanceViewModel();

        public SettingsAppearanceView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }
    }
}
