using MahApps.Metro.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models;
using NETworkManager.Models.Network;
using NETworkManager.Profiles;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

/// <summary>
///     ViewModel for the Ping Monitor Host view.
/// </summary>
public class PingMonitorHostViewModel : ProfileHostViewModelBase
{
    #region Variables

    private CancellationTokenSource _cancellationTokenSource;

    private string _group = Strings.Hosts; // Default group name

    /// <summary>
    ///     Gets or sets the host to ping.
    /// </summary>
    public string Host
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Gets the view for the host history.
    /// </summary>
    public ICollectionView HostHistoryView { get; }

    /// <summary>
    ///     Gets or sets a value indicating whether the ping monitor is running.
    /// </summary>
    public bool IsRunning
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the ping monitor is canceling.
    /// </summary>
    public bool IsCanceling
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the status message is displayed.
    /// </summary>
    public bool IsStatusMessageDisplayed
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Gets the status message.
    /// </summary>
    public string StatusMessage
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Gets or sets the list of ping monitor views.
    /// </summary>
    public ObservableCollection<PingMonitorView> Hosts
    {
        get;
        set
        {
            if (value != null && value == field)
                return;

            field = value;
        }
    } = [];

    /// <summary>
    ///     Gets the view for the hosts.
    /// </summary>
    public ICollectionView HostsView { get; }

    #endregion

    #region Constructor, load settings

    /// <summary>
    ///     Initializes a new instance of the <see cref="PingMonitorHostViewModel" /> class.
    /// </summary>
    public PingMonitorHostViewModel()
    {
        // Host history
        HostHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.PingMonitor_HostHistory);

        // Hosts
        HostsView = CollectionViewSource.GetDefaultView(Hosts);
        HostsView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(PingMonitorView.Group)));
        HostsView.SortDescriptions.Add(new SortDescription(nameof(PingMonitorView.Group), ListSortDirection.Ascending));

        InitializeProfileHost();
    }

    #endregion

    #region Profile host

    protected override ApplicationName ApplicationName => ApplicationName.PingMonitor;

    protected override bool IsProfileEnabled(ProfileInfo profile) => profile.PingMonitor_Enabled;

    protected override string GetSearchableField(ProfileInfo profile) => profile.PingMonitor_Host;

    #endregion

    #region ICommands & Actions

    /// <summary>
    ///     Gets the command to start or stop pinging the host.
    /// </summary>
    public ICommand PingCommand => new RelayCommand(_ => PingAction(), Ping_CanExecute);

    private bool Ping_CanExecute(object parameter)
    {
        return Application.Current.MainWindow != null &&
               !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen &&
               !ConfigurationManager.Current.IsChildWindowOpen;
    }

    private void PingAction()
    {
        if (IsRunning)
            Stop();
        else
            _ = Start();
    }

    /// <summary>
    ///     Gets the command to ping the selected profile.
    /// </summary>
    public ICommand PingProfileCommand => new RelayCommand(_ => PingProfileAction(), PingProfile_CanExecute);

    private bool PingProfile_CanExecute(object obj)
    {
        return !IsSearching && SelectedProfile != null;
    }

    private void PingProfileAction()
    {
        if (SetHost(SelectedProfile.PingMonitor_Host, SelectedProfile.Group))
            _ = Start();
    }

    /// <summary>
    ///     Gets the command to close a group of hosts.
    /// </summary>
    public ICommand CloseGroupCommand => new RelayCommand(CloseGroupAction);

    private void CloseGroupAction(object group)
    {
        RemoveGroup(group.ToString());
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Set the host to ping.
    /// </summary>
    /// <param name="host">Host to ping</param>
    /// <param name="group">Group to add the host to</param>
    /// <returns>True if the host was set successfully, otherwise false</returns>
    public bool SetHost(string host, string group = null)
    {
        // Check if it is already running or canceling
        if (IsRunning || IsCanceling)
        {
            DialogHelper.ShowMessageAsync(Application.Current.MainWindow, Strings.Error,
                Strings.CannotSetHostWhileRunningMessage, ChildWindowIcon.Error);

            return false;
        }

        if (group != null)
            _group = group;

        Host = host;

        return true;
    }

    /// <summary>
    ///     Starts the ping monitor.
    /// </summary>
    public async Task Start()
    {
        IsStatusMessageDisplayed = false;
        IsRunning = true;

        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        // Resolve hostnames
        (List<(IPAddress ipAddress, string hostname)> hosts, List<string> hostnamesNotResolved) hosts;

        try
        {
            hosts = await HostRangeHelper.ResolveAsync(HostRangeHelper.CreateListFromInput(Host),
                SettingsManager.Current.Network_ResolveHostnamePreferIPv4, _cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            UserHasCanceled();

            return;
        }

        // Show error message if (some) hostnames could not be resolved
        if (hosts.hostnamesNotResolved.Count > 0)
        {
            StatusMessage =
                $"{Strings.TheFollowingHostnamesCouldNotBeResolved} {string.Join(", ", hosts.hostnamesNotResolved)}";
            IsStatusMessageDisplayed = true;
        }

        // Add host(s) to history
        AddHostToHistory(Host);

        // Add host(s) to list and start the ping
        foreach (var hostView in hosts.hosts.Select(currentHost =>
                     new PingMonitorView(Guid.NewGuid(), RemoveHostByGuid, currentHost, _group)))
        {
            // Check if the user has canceled the operation
            if (_cancellationTokenSource.IsCancellationRequested)
            {
                UserHasCanceled();

                return;
            }

            Hosts.Add(hostView);

            // Start the ping
            hostView.Start();

            // Wait a bit to prevent the UI from freezing
            await Task.Delay(25);
        }

        Host = string.Empty;
        _group = Strings.Hosts; // Reset the group

        IsCanceling = false;
        IsRunning = false;
    }

    private void Stop()
    {
        IsCanceling = true;
        _cancellationTokenSource.Cancel();
    }

    private void RemoveGroup(string group)
    {
        for (var i = Hosts.Count - 1; i >= 0; i--)
        {
            if (!Hosts[i].Group.Equals(group))
                continue;

            Hosts[i].Stop();
            Hosts[i].Cleanup();
            Hosts.RemoveAt(i);
        }
    }

    private void RemoveHostByGuid(Guid hostId)
    {
        var i = -1;

        foreach (var host in Hosts)
            if (host.HostId.Equals(hostId))
                i = Hosts.IndexOf(host);

        if (i == -1)
            return;

        Hosts[i].Stop();
        Hosts[i].Cleanup();
        Hosts.RemoveAt(i);
    }

    private void AddHostToHistory(string host)
    {
        // Create the new list
        var list = ListHelper.Modify([.. SettingsManager.Current.PingMonitor_HostHistory], host,
            SettingsManager.Current.General_HistoryListEntries);

        // Clear the old items
        SettingsManager.Current.PingMonitor_HostHistory.Clear();
        OnPropertyChanged(nameof(Host)); // Raise property changed again, after the collection has been cleared

        // Fill with the new items
        list.ForEach(SettingsManager.Current.PingMonitor_HostHistory.Add);
    }

    #endregion

    #region Event

    private void UserHasCanceled()
    {
        StatusMessage = Strings.CanceledByUserMessage;
        IsStatusMessageDisplayed = true;

        IsCanceling = false;
        IsRunning = false;
    }

    #endregion
}
