using NETworkManager.ViewModels.Settings;
using System.Windows.Controls;

namespace NETworkManager.Views.Settings
{
    public partial class SettingsApplicationDNSLookupView : UserControl
    {
        SettingsApplicationDNSLookupViewModel viewModel = new SettingsApplicationDNSLookupViewModel();

        public SettingsApplicationDNSLookupView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
