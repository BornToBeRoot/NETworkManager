using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class SettingsAutostartView : UserControl
    {
        SettingsAutostartViewModel viewModel = new SettingsAutostartViewModel(DialogCoordinator.Instance);

        public SettingsAutostartView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
