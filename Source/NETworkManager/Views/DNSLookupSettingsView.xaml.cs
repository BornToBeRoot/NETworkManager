using NETworkManager.ViewModels;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class DNSLookupSettingsView : UserControl
    {
        DNSLookupSettingsViewModel viewModel = new DNSLookupSettingsViewModel();

        public DNSLookupSettingsView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
