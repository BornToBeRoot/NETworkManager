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
using System.Windows.Threading;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Localization.Resources;
using NETworkManager.Models;
using NETworkManager.Models.Network;
using NETworkManager.Profiles;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;

namespace NETworkManager.ViewModels;

public class PingMonitorHostViewModel : ViewModelBase, IProfileManager
{
    #region Variables

    private readonly IDialogCoordinator _dialogCoordinator;

    private CancellationTokenSource _cancellationTokenSource;

    private readonly DispatcherTimer _searchDispatcherTimer = new();

    private readonly bool _isLoading;
    private bool _isViewActive = true;

    private string _group = Strings.Hosts; // Default group name

    private string _host;

    public string Host
    {
        get => _host;
        set
        {
            if (value == _host)
                return;

            _host = value;
            OnPropertyChanged();
        }
    }

    public ICollectionView HostHistoryView { get; }

    private bool _isRunning;

    public bool IsRunning
    {
        get => _isRunning;
        set
        {
            if (value == _isRunning)
                return;

            _isRunning = value;
            OnPropertyChanged();
        }
    }

    private bool _isCanceling;

    public bool IsCanceling
    {
        get => _isCanceling;
        set
        {
            if (value == _isCanceling)
                return;

            _isCanceling = value;
            OnPropertyChanged();
        }
    }

    private bool _isStatusMessageDisplayed;

    public bool IsStatusMessageDisplayed
    {
        get => _isStatusMessageDisplayed;
        set
        {
            if (value == _isStatusMessageDisplayed)
                return;

            _isStatusMessageDisplayed = value;
            OnPropertyChanged();
        }
    }

    private string _statusMessage;

    public string StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (value == _statusMessage)
                return;

            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<PingMonitorView> _hosts = [];

    public ObservableCollection<PingMonitorView> Hosts
    {
        get => _hosts;
        set
        {
            if (value != null && value == _hosts)
                return;

            _hosts = value;
        }
    }

    public ICollectionView HostsView { get; }

    private PingMonitorView _selectedHost;

    public PingMonitorView SelectedHost
    {
        get => _selectedHost;
        set
        {
            if (value == _selectedHost)
                return;

            _selectedHost = value;
            OnPropertyChanged();
        }
    }

    #region Profiles

    private ICollectionView _profiles;

    public ICollectionView Profiles
    {
        get => _profiles;
        private set
        {
            if (value == _profiles)
                return;

            _profiles = value;
            OnPropertyChanged();
        }
    }

    private ProfileInfo _selectedProfile;

    public ProfileInfo SelectedProfile
    {
        get => _selectedProfile;
        set
        {
            if (value == _selectedProfile)
                return;

            _selectedProfile = value;
            OnPropertyChanged();
        }
    }

    private string _search;

    public string Search
    {
        get => _search;
        set
        {
            if (value == _search)
                return;

            _search = value;

            // Start searching...
            IsSearching = true;
            _searchDispatcherTimer.Start();

            OnPropertyChanged();
        }
    }

    private bool _isSearching;

    public bool IsSearching
    {
        get => _isSearching;
        set
        {
            if (value == _isSearching)
                return;

            _isSearching = value;
            OnPropertyChanged();
        }
    }

    private bool _canProfileWidthChange = true;
    private double _tempProfileWidth;

    private bool _expandProfileView;

    public bool ExpandProfileView
    {
        get => _expandProfileView;
        set
        {
            if (value == _expandProfileView)
                return;

            if (!_isLoading)
                SettingsManager.Current.PingMonitor_ExpandProfileView = value;

            _expandProfileView = value;

            if (_canProfileWidthChange)
                ResizeProfile(false);

            OnPropertyChanged();
        }
    }

    private GridLength _profileWidth;

    public GridLength ProfileWidth
    {
        get => _profileWidth;
        set
        {
            if (value == _profileWidth)
                return;

            if (!_isLoading && Math.Abs(value.Value - GlobalStaticConfiguration.Profile_WidthCollapsed) >
                GlobalStaticConfiguration.Profile_FloatPointFix) // Do not save the size when collapsed
                SettingsManager.Current.PingMonitor_ProfileWidth = value.Value;

            _profileWidth = value;

            if (_canProfileWidthChange)
                ResizeProfile(true);

            OnPropertyChanged();
        }
    }

    #endregion

    #endregion

    #region Constructor, load settings

    public PingMonitorHostViewModel(IDialogCoordinator instance)
    {
        _isLoading = true;

        _dialogCoordinator = instance;

        // Host history
        HostHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.PingMonitor_HostHistory);

        // Hosts
        HostsView = CollectionViewSource.GetDefaultView(Hosts);
        HostsView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(PingMonitorView.Group)));
        HostsView.SortDescriptions.Add(new SortDescription(nameof(PingMonitorView.Group), ListSortDirection.Ascending));

        // Profiles
        SetProfilesView();

        ProfileManager.OnProfilesUpdated += ProfileManager_OnProfilesUpdated;

        _searchDispatcherTimer.Interval = GlobalStaticConfiguration.SearchDispatcherTimerTimeSpan;
        _searchDispatcherTimer.Tick += SearchDispatcherTimer_Tick;

        LoadSettings();

        _isLoading = false;
    }

    private void LoadSettings()
    {
        ExpandProfileView = SettingsManager.Current.PingMonitor_ExpandProfileView;

        ProfileWidth = ExpandProfileView
            ? new GridLength(SettingsManager.Current.PingMonitor_ProfileWidth)
            : new GridLength(GlobalStaticConfiguration.Profile_WidthCollapsed);

        _tempProfileWidth = SettingsManager.Current.PingMonitor_ProfileWidth;
    }

    #endregion

    #region ICommands & Actions

    public ICommand PingCommand => new RelayCommand(_ => PingAction(), Ping_CanExecute);

    private bool Ping_CanExecute(object parameter)
    {
        return Application.Current.MainWindow != null && !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen;
    }

    private void PingAction()
    {
        if (IsRunning)
            Stop();
        else
            Start().ConfigureAwait(false);
    }

    public ICommand PingProfileCommand => new RelayCommand(_ => PingProfileAction(), PingProfile_CanExecute);

    private bool PingProfile_CanExecute(object obj)
    {
        return !IsSearching && SelectedProfile != null;
    }

    private void PingProfileAction()
    {
        if (SetHost(SelectedProfile.PingMonitor_Host, SelectedProfile.Group))
            Start().ConfigureAwait(false);
    }

    public ICommand CloseGroupCommand => new RelayCommand(CloseGroupAction);

    private void CloseGroupAction(object group)
    {
        RemoveGroup(group.ToString());
    }

    public ICommand ExportCommand => new RelayCommand(_ => ExportAction());

    private void ExportAction()
    {
        SelectedHost?.Export();
    }

    public ICommand AddProfileCommand => new RelayCommand(_ => AddProfileAction());

    private void AddProfileAction()
    {
        ProfileDialogManager
            .ShowAddProfileDialog(this, this, _dialogCoordinator, null, null, ApplicationName.PingMonitor)
            .ConfigureAwait(false);
    }

    private bool ModifyProfile_CanExecute(object obj)
    {
        return SelectedProfile is { IsDynamic: false };
    }

    public ICommand EditProfileCommand => new RelayCommand(_ => EditProfileAction(), ModifyProfile_CanExecute);

    private void EditProfileAction()
    {
        ProfileDialogManager.ShowEditProfileDialog(this, _dialogCoordinator, SelectedProfile).ConfigureAwait(false);
    }

    public ICommand CopyAsProfileCommand => new RelayCommand(_ => CopyAsProfileAction(), ModifyProfile_CanExecute);

    private void CopyAsProfileAction()
    {
        ProfileDialogManager.ShowCopyAsProfileDialog(this, _dialogCoordinator, SelectedProfile).ConfigureAwait(false);
    }

    public ICommand DeleteProfileCommand => new RelayCommand(_ => DeleteProfileAction(), ModifyProfile_CanExecute);

    private void DeleteProfileAction()
    {
        ProfileDialogManager
            .ShowDeleteProfileDialog(this, _dialogCoordinator, new List<ProfileInfo> { SelectedProfile })
            .ConfigureAwait(false);
    }

    public ICommand EditGroupCommand => new RelayCommand(EditGroupAction);

    private void EditGroupAction(object group)
    {
        ProfileDialogManager.ShowEditGroupDialog(this, _dialogCoordinator, ProfileManager.GetGroup(group.ToString()))
            .ConfigureAwait(false);
    }

    public ICommand ClearSearchCommand => new RelayCommand(_ => ClearSearchAction());

    private void ClearSearchAction()
    {
        Search = string.Empty;
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
            _dialogCoordinator.ShowMessageAsync(this, Strings.Error,
                Strings.CannotSetHostWhileRunningMessage);

            return false;
        }

        if (group != null)
            _group = group;

        Host = host;

        return true;
    }

    public async Task Start()
    {
        IsStatusMessageDisplayed = false;
        IsRunning = true;

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
        Hosts.RemoveAt(i);
    }

    private void AddHostToHistory(string host)
    {
        // Create the new list
        var list = ListHelper.Modify(SettingsManager.Current.PingMonitor_HostHistory.ToList(), host,
            SettingsManager.Current.General_HistoryListEntries);

        // Clear the old items
        SettingsManager.Current.PingMonitor_HostHistory.Clear();
        OnPropertyChanged(nameof(Host)); // Raise property changed again, after the collection has been cleared

        // Fill with the new items
        list.ForEach(x => SettingsManager.Current.PingMonitor_HostHistory.Add(x));
    }

    private void ResizeProfile(bool dueToChangedSize)
    {
        _canProfileWidthChange = false;

        if (dueToChangedSize)
        {
            ExpandProfileView = Math.Abs(ProfileWidth.Value - GlobalStaticConfiguration.Profile_WidthCollapsed) >
                                GlobalStaticConfiguration.Profile_FloatPointFix;
        }
        else
        {
            if (ExpandProfileView)
            {
                ProfileWidth =
                    Math.Abs(_tempProfileWidth - GlobalStaticConfiguration.Profile_WidthCollapsed) <
                    GlobalStaticConfiguration.Profile_FloatPointFix
                        ? new GridLength(GlobalStaticConfiguration.Profile_DefaultWidthExpanded)
                        : new GridLength(_tempProfileWidth);
            }
            else
            {
                _tempProfileWidth = ProfileWidth.Value;
                ProfileWidth = new GridLength(GlobalStaticConfiguration.Profile_WidthCollapsed);
            }
        }

        _canProfileWidthChange = true;
    }

    public void OnViewVisible()
    {
        _isViewActive = true;

        RefreshProfiles();
    }

    public void OnViewHide()
    {
        _isViewActive = false;
    }

    private void SetProfilesView(ProfileInfo profile = null)
    {
        Profiles = new CollectionViewSource
        {
            Source = ProfileManager.Groups.SelectMany(x => x.Profiles).Where(x => x.PingMonitor_Enabled)
                .OrderBy(x => x.Group).ThenBy(x => x.Name)
        }.View;

        Profiles.GroupDescriptions.Add(new PropertyGroupDescription(nameof(ProfileInfo.Group)));

        Profiles.Filter = o =>
        {
            if (o is not ProfileInfo info)
                return false;

            if (string.IsNullOrEmpty(Search))
                return true;

            var search = Search.Trim();

            // Search by: Tag=xxx (exact match, ignore case)
            /*
            if (search.StartsWith(ProfileManager.TagIdentifier, StringComparison.OrdinalIgnoreCase))
                return !string.IsNullOrEmpty(info.Tags) && info.PingMonitor_Enabled && info.Tags.Replace(" ", "").Split(';').Any(str => search.Substring(ProfileManager.TagIdentifier.Length, search.Length - ProfileManager.TagIdentifier.Length).Equals(str, StringComparison.OrdinalIgnoreCase));
            */

            // Search by: Name, PingMonitor_Host
            return info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   info.PingMonitor_Host.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1;
        };

        // Set specific profile or first if null
        SelectedProfile = null;

        if (profile != null)
            SelectedProfile = Profiles.Cast<ProfileInfo>().FirstOrDefault(x => x.Equals(profile)) ??
                              Profiles.Cast<ProfileInfo>().FirstOrDefault();
        else
            SelectedProfile = Profiles.Cast<ProfileInfo>().FirstOrDefault();
    }

    private void RefreshProfiles()
    {
        if (!_isViewActive)
            return;

        SetProfilesView(SelectedProfile);
    }

    #endregion

    #region Event

    private void ProfileManager_OnProfilesUpdated(object sender, EventArgs e)
    {
        RefreshProfiles();
    }

    private void SearchDispatcherTimer_Tick(object sender, EventArgs e)
    {
        _searchDispatcherTimer.Stop();

        RefreshProfiles();

        IsSearching = false;
    }

    private void UserHasCanceled()
    {
        StatusMessage = Strings.CanceledByUserMessage;
        IsStatusMessageDisplayed = true;

        IsCanceling = false;
        IsRunning = false;
    }

    #endregion
}