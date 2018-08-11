using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class SettingsLanguageView
    {
        private readonly SettingsLanguageViewModel _viewModel = new SettingsLanguageViewModel();

        public SettingsLanguageView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }
    }
}
