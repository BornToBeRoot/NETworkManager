using NETworkManager.ViewModels.Settings;
using System.Windows.Controls;

namespace NETworkManager.Views.Settings
{
    public partial class SettingsApplicationPuTTYView : UserControl
    {
        SettingsApplicationPuTTYViewModel viewModel = new SettingsApplicationPuTTYViewModel();

        public SettingsApplicationPuTTYView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
