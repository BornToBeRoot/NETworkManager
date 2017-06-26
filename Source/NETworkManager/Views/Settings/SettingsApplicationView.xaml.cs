using NETworkManager.ViewModels.Settings;
using System.Windows.Controls;

namespace NETworkManager.Views.Settings
{
    public partial class SettingsApplicationView : UserControl
    {
        SettingsApplicationViewModel viewModel = new SettingsApplicationViewModel();

        public SettingsApplicationView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
