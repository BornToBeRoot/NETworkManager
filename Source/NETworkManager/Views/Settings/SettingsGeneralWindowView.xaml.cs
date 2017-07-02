using NETworkManager.ViewModels.Settings;
using System.Windows.Controls;

namespace NETworkManager.Views.Settings
{
    public partial class SettingsGeneralWindowView : UserControl
    {
        SettingsGeneralWindowViewModel viewModel = new SettingsGeneralWindowViewModel();

        public SettingsGeneralWindowView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
