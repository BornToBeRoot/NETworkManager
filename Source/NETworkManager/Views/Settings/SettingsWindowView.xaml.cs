using NETworkManager.ViewModels.Settings;
using System.Windows.Controls;

namespace NETworkManager.Views.Settings
{
    public partial class SettingsWindowView : UserControl
    {
        SettingsWindowViewModel viewModel = new SettingsWindowViewModel();

        public SettingsWindowView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
