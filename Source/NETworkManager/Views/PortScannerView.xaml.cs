using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;
using System.Windows.Controls;

namespace NETworkManager.Views
{
      public partial class PortScannerView : UserControl
    {
        PortScannerViewModel viewModel;

        public PortScannerView(int tabId, string host = null, string ports = null)
        {
            InitializeComponent();

            viewModel = new PortScannerViewModel(tabId, host, ports);

            DataContext = viewModel;

            Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            viewModel.OnLoaded();
        }

        private void Dispatcher_ShutdownStarted(object sender, System.EventArgs e)
        {
            viewModel.OnClose();
        }

        public void CloseTab()
        {
            viewModel.OnClose();
        }

        private void ContextMenu_Opened(object sender, System.Windows.RoutedEventArgs e)
        {
            ContextMenu menu = sender as ContextMenu;
            menu.DataContext = viewModel;
        }
    }
}