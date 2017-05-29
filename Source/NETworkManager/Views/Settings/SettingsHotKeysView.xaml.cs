using NETworkManager.ViewModels.Settings;
using System.Windows.Controls;

namespace NETworkManager.Views.Settings
{
    public partial class SettingsHotKeysView : UserControl
    {
        SettingsHotKeysViewModel viewModel = new SettingsHotKeysViewModel();

        public SettingsHotKeysView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
