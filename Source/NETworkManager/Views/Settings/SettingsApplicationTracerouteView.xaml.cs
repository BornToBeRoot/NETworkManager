using NETworkManager.ViewModels.Settings;
using System.Windows.Controls;

namespace NETworkManager.Views.Settings
{
    public partial class SettingsApplicationTracerouteView : UserControl
    {
        SettingsApplicationTracerouteViewModel viewModel = new SettingsApplicationTracerouteViewModel();

        public SettingsApplicationTracerouteView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
