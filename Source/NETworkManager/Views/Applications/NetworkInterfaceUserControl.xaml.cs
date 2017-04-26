using System.Windows.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Windows;
using NETworkManager.ViewModels.Applications;

namespace NETworkManager.Views.Applications
{
    public partial class NetworkInterfaceUserControl : UserControl
    {
        NetworkInterfaceViewModel viewModel = new NetworkInterfaceViewModel(DialogCoordinator.Instance);

        public NetworkInterfaceUserControl()
        {
            InitializeComponent();
            DataContext = viewModel;

            Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
        }

        private void Dispatcher_ShutdownStarted(object sender, System.EventArgs e)
        {
            viewModel.OnShutdown();
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = sender as ContextMenu;
            menu.DataContext = viewModel;
        }
    }
}
