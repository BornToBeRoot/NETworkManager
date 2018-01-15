using NETworkManager.ViewModels.Settings;
using System.Windows.Controls;

namespace NETworkManager.Views.Settings
{
    public partial class SettingsApplicationSNMPView : UserControl
    {
        SettingsApplicationSNMPViewModel viewModel = new SettingsApplicationSNMPViewModel();

        public SettingsApplicationSNMPView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
