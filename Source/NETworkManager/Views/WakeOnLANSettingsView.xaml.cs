using NETworkManager.ViewModels;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class WakeOnLANSettingsView : UserControl
    {
        WakeOnLANSettingsViewModel viewModel = new WakeOnLANSettingsViewModel();

        public WakeOnLANSettingsView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
