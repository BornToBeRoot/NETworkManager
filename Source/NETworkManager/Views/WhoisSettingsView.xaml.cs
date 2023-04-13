using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    /// <summary>
    /// Interaction logic for WhoisSettingsViewModel.xaml
    /// </summary>
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
