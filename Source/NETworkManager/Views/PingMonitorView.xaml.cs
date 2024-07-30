using System;
using System.Net;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class PingMonitorView
{
    private readonly PingMonitorViewModel _viewModel;

    public PingMonitorView(Guid hostId, Action<Guid> removeHostByGuid, (IPAddress ipAddress, string hostname) host,
        string group)
    {
        InitializeComponent();

        _viewModel = new PingMonitorViewModel(DialogCoordinator.Instance, hostId, removeHostByGuid, host, group);

        DataContext = _viewModel;

        Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
    }

    public Guid HostId => _viewModel.HostId;

    public string Group => _viewModel.Group;

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