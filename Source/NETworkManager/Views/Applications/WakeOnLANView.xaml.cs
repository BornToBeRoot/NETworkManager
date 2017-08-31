using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels.Applications;
using System.Windows;
using System.Windows.Controls;

namespace NETworkManager.Views.Applications
{
    public partial class WakeOnLANView : UserControl
    {
        private WakeOnLANViewModel viewModel = new WakeOnLANViewModel(DialogCoordinator.Instance);

        public WakeOnLANView()
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
