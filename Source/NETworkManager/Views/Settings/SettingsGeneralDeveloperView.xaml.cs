using NETworkManager.ViewModels.Settings;
using System.Windows.Controls;

namespace NETworkManager.Views.Settings
{
    public partial class SettingsGeneralDeveloperView : UserControl
    {
        SettingsGeneralDeveloperViewModel viewModel = new SettingsGeneralDeveloperViewModel();

        public SettingsGeneralDeveloperView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
