using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels.Settings;
using System.Windows.Controls;

namespace NETworkManager.Views.Settings
{
    public partial class SettingsGeneralAutostartView : UserControl
    {
        SettingsGeneralAutostartViewModel viewModel = new SettingsGeneralAutostartViewModel(DialogCoordinator.Instance);

        public SettingsGeneralAutostartView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
