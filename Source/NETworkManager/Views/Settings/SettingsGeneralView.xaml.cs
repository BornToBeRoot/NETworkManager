using NETworkManager.ViewModels.Settings;
using System.Windows.Controls;

namespace NETworkManager.Views.Settings
{
    public partial class SettingsGeneralView : UserControl
    {
        SettingsGeneralViewModel viewModel = new SettingsGeneralViewModel();

        public SettingsGeneralView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
