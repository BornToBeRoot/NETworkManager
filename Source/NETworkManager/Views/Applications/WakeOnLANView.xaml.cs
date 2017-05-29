using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels.Applications;
using System.Windows;
using System.Windows.Controls;

namespace NETworkManager.Views.Applications
{
    public partial class WakeOnLANView : UserControl
    {
        private WakeOnLanViewModel viewModel = new WakeOnLanViewModel(DialogCoordinator.Instance);

        public WakeOnLANView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = sender as ContextMenu;
            menu.DataContext = viewModel;
        }
    }
}
