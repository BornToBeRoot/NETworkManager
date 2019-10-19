using NETworkManager.ViewModels;
using System;
using NETworkManager.Models.Network;

namespace NETworkManager.Views
{
    public partial class PingMonitorHostView
    {
        private readonly PingMonitorHostViewModel _viewModel;

        public PingMonitorHostView(int hostId, PingMonitorOptions options)
        {
            InitializeComponent();

            _viewModel = new PingMonitorHostViewModel(hostId, options);

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
    }
}
