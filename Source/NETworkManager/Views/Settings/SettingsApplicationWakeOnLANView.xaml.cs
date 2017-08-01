using NETworkManager.ViewModels.Settings;
using System.Windows.Controls;

namespace NETworkManager.Views.Settings
{
    public partial class SettingsApplicationWakeOnLANView : UserControl
    {
        SettingsApplicationWakeOnLANViewModel viewModel = new SettingsApplicationWakeOnLANViewModel();

        public SettingsApplicationWakeOnLANView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
