using System;
using System.Windows.Controls;
using NETworkManager.ViewModels;
using MahApps.Metro.Controls.Dialogs;

namespace NETworkManager.Views
{
    public partial class IPScannerView : UserControl
    {
        IPScannerViewModel viewModel = new IPScannerViewModel(DialogCoordinator.Instance);

        public IPScannerView()
        {
            InitializeComponent();
            DataContext = viewModel;

            Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;            
        }

        private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
        {
            viewModel.OnShutdown();
        }

        private void ContextMenu_Opened(object sender, System.Windows.RoutedEventArgs e)
        {
            ContextMenu menu = sender as ContextMenu;
            menu.DataContext = viewModel;
        }
    }
}
