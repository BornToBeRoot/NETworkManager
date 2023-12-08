using NETworkManager.ViewModels;
using System;
using MahApps.Metro.Controls.Dialogs;
using System.Net;

namespace NETworkManager.Views;

public partial class PingMonitorView
{
    private readonly PingMonitorViewModel _viewModel;

    public Guid HostId => _viewModel.HostId;

    public PingMonitorView(Guid hostId, Action<Guid> removeHostByGuid, (IPAddress ipAddress, string hostname) host)
    {
        InitializeComponent();

        _viewModel = new PingMonitorViewModel(DialogCoordinator.Instance, hostId, removeHostByGuid, host);

        DataContext = _viewModel;

        Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
    }

    public void Start()
    {
        _viewModel.Start();
    }
    public void Stop()
    {
        _viewModel.Stop();
    }

    public void Export()
    {
        _viewModel.Export().ConfigureAwait(false);
    }

    private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
    {
        Stop();
    }
}
