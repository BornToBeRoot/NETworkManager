using NETworkManager.ViewModels;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class RemoteDesktopSettingsView : UserControl
    {
        RemoteDesktopSettingsViewModel viewModel = new RemoteDesktopSettingsViewModel();

        public RemoteDesktopSettingsView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
