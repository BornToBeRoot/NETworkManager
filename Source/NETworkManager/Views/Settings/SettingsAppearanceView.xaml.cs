using NETworkManager.ViewModels.Settings;
using System.Windows.Controls;

namespace NETworkManager.Views.Settings
{
    public partial class SettingsAppearanceView : UserControl
    {
        SettingsAppearanceViewModel viewModel = new SettingsAppearanceViewModel();

        public SettingsAppearanceView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
