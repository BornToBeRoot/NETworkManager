using NETworkManager.ViewModels.Settings;
using System.Windows.Controls;

namespace NETworkManager.Views.Settings
{
    public partial class SettingsApplicationRemoteDesktopView : UserControl
    {
        SettingsApplicationRemoteDesktopViewModel viewModel = new SettingsApplicationRemoteDesktopViewModel();

        public SettingsApplicationRemoteDesktopView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
