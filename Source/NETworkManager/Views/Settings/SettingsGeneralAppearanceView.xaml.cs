using NETworkManager.ViewModels.Settings;
using System.Windows.Controls;

namespace NETworkManager.Views.Settings
{
    public partial class SettingsGeneralAppearanceView : UserControl
    {
        SettingsGeneralAppearanceViewModel viewModel = new SettingsGeneralAppearanceViewModel();

        public SettingsGeneralAppearanceView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
