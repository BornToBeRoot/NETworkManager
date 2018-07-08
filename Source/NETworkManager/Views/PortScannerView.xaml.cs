using NETworkManager.ViewModels;
using System.Windows.Controls;

namespace NETworkManager.Views
{
      public partial class PortScannerView
      {
          private readonly PortScannerViewModel _viewModel;

        public PortScannerView(int tabId, string host = null, string ports = null)
        {
            InitializeComponent();

            _viewModel = new PortScannerViewModel(tabId, host, ports);

            DataContext = _viewModel;

            Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            _viewModel.OnLoaded();
        }

        private void Dispatcher_ShutdownStarted(object sender, System.EventArgs e)
        {
            _viewModel.OnClose();
        }

        public void CloseTab()
        {
            _viewModel.OnClose();
        }

        private void ContextMenu_Opened(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is ContextMenu menu) menu.DataContext = _viewModel;
        }
    }
}