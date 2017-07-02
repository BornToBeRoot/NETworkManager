using NETworkManager.ViewModels.Settings;
using System.Windows.Controls;

namespace NETworkManager.Views.Settings
{
    public partial class SettingsGeneralGeneralView : UserControl
    {
        SettingsGeneralGeneralViewModel viewModel = new SettingsGeneralGeneralViewModel();

        public SettingsGeneralGeneralView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
