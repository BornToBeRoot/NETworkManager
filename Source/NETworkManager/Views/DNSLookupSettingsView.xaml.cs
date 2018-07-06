using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class DNSLookupSettingsView
    {
        private readonly DNSLookupSettingsViewModel _viewModel = new DNSLookupSettingsViewModel();

        public DNSLookupSettingsView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }
    }
}
