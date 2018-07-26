using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class SettingsUpdateView
    {
        private readonly SettingsUpdateViewModel _viewModel = new SettingsUpdateViewModel();

        public SettingsUpdateView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }
    }
}
