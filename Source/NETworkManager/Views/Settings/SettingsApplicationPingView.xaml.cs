using NETworkManager.ViewModels.Settings;
using System.Windows.Controls;

namespace NETworkManager.Views.Settings
{
    public partial class SettingsApplicationPingView : UserControl
    {
        SettingsApplicationPingViewModel viewModel = new SettingsApplicationPingViewModel();

        public SettingsApplicationPingView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
