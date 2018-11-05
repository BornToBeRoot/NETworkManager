using NETworkManager.ViewModels;
using System;
using System.Windows.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace NETworkManager.Views
{
    public partial class PingView
    {
        private readonly PingViewModel _viewModel;

        public PingView(int tabId, string host = null)
        {
            InitializeComponent();

            _viewModel = new PingViewModel(DialogCoordinator.Instance,tabId, host);

            DataContext = _viewModel;

            Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            _viewModel.OnLoaded();
        }

        private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
        {
            _viewModel.OnClose();
        }

        public void CloseTab()
        {
            _viewModel.OnClose();
        }

        private void ContextMenu_Opened(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is ContextMenu menu)
                menu.DataContext = _viewModel;
        }
    }
}
