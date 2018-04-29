using System;
using System.Windows.Controls;
using NETworkManager.ViewModels;
using MahApps.Metro.Controls.Dialogs;

namespace NETworkManager.Views
{
    public partial class IPScannerView : UserControl
    {
        IPScannerViewModel viewModel;

        public IPScannerView(int tabId, string ipRange = null)
        {
            InitializeComponent();

            viewModel = new IPScannerViewModel(tabId, ipRange);

            DataContext = viewModel;

            Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            viewModel.OnLoaded();
        }

        private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
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
