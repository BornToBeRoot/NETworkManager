using System;
using System.Net;
using System.Windows.Controls;
using NETworkManager.Models.Network;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class PingMonitorView : IPingMonitorHostStatus
{
    private readonly PingMonitorViewModel _viewModel;

    public PingMonitorView(Guid hostId, Action<Guid> removeHostByGuid, (IPAddress ipAddress, string hostname) host,
        string group)
    {
        InitializeComponent();

        _viewModel = new PingMonitorViewModel(hostId, removeHostByGuid, host, group);

        DataContext = _viewModel;

        Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
    }

    public Guid HostId => _viewModel.HostId;

    public string Group => _viewModel.Group;

    /// <summary>
    /// The underlying view model, exposed so <see cref="ViewModels.PingMonitorHostViewModel"/> can
    /// subscribe to <see cref="PingMonitorViewModel.PropertyChanged"/> for the group up/down
    /// summary. Prefer <see cref="IsReachable"/>/<see cref="IsRunning"/> (via
    /// <see cref="IPingMonitorHostStatus"/>) to just read the current status.
    /// </summary>
    public PingMonitorViewModel ViewModel => _viewModel;

    /// <inheritdoc />
    public bool IsReachable => _viewModel.IsReachable;

    /// <inheritdoc />
    public bool IsRunning => _viewModel.IsRunning;

    public void Start()
    {
        _viewModel.Start();
    }

    public void Stop()
    {
        _viewModel.Stop();
    }

    /// <summary>
    /// Releases resources held by the view model. Must be called when the host is removed.
    /// </summary>
    public void Cleanup()
    {
        // Release the app-lifetime subscription so this transient per-host view (and its
        // view model) is not kept alive until the application shuts down.
        Dispatcher.ShutdownStarted -= Dispatcher_ShutdownStarted;

        _viewModel.Cleanup();
    }

    private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
    {
        Stop();
    }

    /// <summary>
    /// Suppresses the host context menu while the pointer is over the chart, so a
    /// right-click drag can be used to zoom into a section instead of opening the menu.
    /// </summary>
    private void Chart_ContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        e.Handled = true;
    }
}