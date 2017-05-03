using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModel.Applications;
using System.Windows;
using System.Windows.Controls;

namespace NETworkManager.View.Applications
{
    public partial class WakeOnLANUserControl : UserControl
    {
        private WakeOnLanViewModel viewModel = new WakeOnLanViewModel(DialogCoordinator.Instance);

        public WakeOnLANUserControl()
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
