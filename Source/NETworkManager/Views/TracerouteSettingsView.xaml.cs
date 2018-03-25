using NETworkManager.ViewModels;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class TracerouteSettingsView : UserControl
    {
        TracerouteSettingsViewModel viewModel = new TracerouteSettingsViewModel();

        public TracerouteSettingsView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
