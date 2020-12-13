using NETworkManager.ViewModels;
using System;
using NETworkManager.Models.Network;
using MahApps.Metro.Controls.Dialogs;

namespace NETworkManager.Views
{
    public partial class PingMonitorView
    {
        private readonly PingMonitorViewModel _viewModel;

        public int HostId => _viewModel.HostId;

        public PingMonitorView(int hostId, Action<int> closeCallback, PingMonitorOptions options)
        {
            InitializeComponent();

            _viewModel = new PingMonitorViewModel(DialogCoordinator.Instance, hostId, closeCallback, options);

            DataContext = _viewModel;

            Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            _viewModel.OnLoaded();
        }

        public void Export()
        {
            _viewModel.Export();
        }

        private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
        {
            _viewModel.OnClose();
        }

        public void CloseView()
        {
            _viewModel.OnClose();
        }
    }
}
