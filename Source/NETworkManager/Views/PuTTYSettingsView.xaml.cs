using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class PuTTYSettingsView : UserControl
    {
        PuTTYSettingsViewModel viewModel = new PuTTYSettingsViewModel(DialogCoordinator.Instance);

        public PuTTYSettingsView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
