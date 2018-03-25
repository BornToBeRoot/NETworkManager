using NETworkManager.ViewModels;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class PingSettingsView : UserControl
    {
        PingSettingsViewModel viewModel = new PingSettingsViewModel();

        public PingSettingsView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
