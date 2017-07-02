using NETworkManager.ViewModels.Settings;
using System.Windows.Controls;

namespace NETworkManager.Views.Settings
{
    public partial class SettingsGeneralHotKeysView : UserControl
    {
        SettingsGeneralHotKeysViewModel viewModel = new SettingsGeneralHotKeysViewModel();

        public SettingsGeneralHotKeysView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
