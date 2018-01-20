using NETworkManager.ViewModels.Settings;
using System.Windows.Controls;

namespace NETworkManager.Views.Settings
{
    public partial class SettingsGeneralUpdateView : UserControl
    {
        SettingsGeneralUpdateViewModel viewModel = new SettingsGeneralUpdateViewModel();

        public SettingsGeneralUpdateView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
