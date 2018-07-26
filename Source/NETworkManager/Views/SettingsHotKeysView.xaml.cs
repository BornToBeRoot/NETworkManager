using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class SettingsHotKeysView
    {
        private readonly SettingsHotKeysViewModel _viewModel = new SettingsHotKeysViewModel();

        public SettingsHotKeysView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }
    }
}
