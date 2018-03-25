using NETworkManager.ViewModels;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class SNMPSettingsView : UserControl
    {
        SNMPSettingsViewModel viewModel = new SNMPSettingsViewModel();

        public SNMPSettingsView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
