﻿using Dragablz;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models;
using NETworkManager.Models.RemoteDesktop;
using NETworkManager.Profiles;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using RemoteDesktop = NETworkManager.Profiles.Application.RemoteDesktop;

namespace NETworkManager.ViewModels;

public class RemoteDesktopHostViewModel : ViewModelBase, IProfileManager
{
    #region Variables

    private readonly IDialogCoordinator _dialogCoordinator;

    private readonly DispatcherTimer _searchDispatcherTimer = new();
    private bool _searchDisabled;

    public IInterTabClient InterTabClient { get; }

    private string _interTabPartition;

    public string InterTabPartition
    {
        get => _interTabPartition;
        set
        {
            if (value == _interTabPartition)
                return;

            _interTabPartition = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<DragablzTabItem> TabItems { get; }

    private readonly bool _isLoading;
    private bool _isViewActive = true;

    private int _selectedTabIndex;

    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set
        {
            if (value == _selectedTabIndex)
                return;

            _selectedTabIndex = value;
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

    private ProfileInfo _selectedProfile = new();

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
            if (!_searchDisabled)
            {
                IsSearching = true;
                _searchDispatcherTimer.Start();
            }

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

    private bool _profileFilterIsOpen;

    public bool ProfileFilterIsOpen
    {
        get => _profileFilterIsOpen;
        set
        {
            if (value == _profileFilterIsOpen)
                return;

            _profileFilterIsOpen = value;
            OnPropertyChanged();
        }
    }

    public ICollectionView ProfileFilterTagsView { get; }

    private ObservableCollection<ProfileFilterTagsInfo> ProfileFilterTags { get; } = [];

    private bool _profileFilterTagsMatchAny = GlobalStaticConfiguration.Profile_TagsMatchAny;

    public bool ProfileFilterTagsMatchAny
    {
        get => _profileFilterTagsMatchAny;
        set
        {
            if (value == _profileFilterTagsMatchAny)
                return;

            _profileFilterTagsMatchAny = value;
            OnPropertyChanged();
        }
    }

    private bool _profileFilterTagsMatchAll;

    public bool ProfileFilterTagsMatchAll
    {
        get => _profileFilterTagsMatchAll;
        set
        {
            if (value == _profileFilterTagsMatchAll)
                return;

            _profileFilterTagsMatchAll = value;
            OnPropertyChanged();
        }
    }

    private bool _isProfileFilterSet;

    public bool IsProfileFilterSet
    {
        get => _isProfileFilterSet;
        set
        {
            if (value == _isProfileFilterSet)
                return;

            _isProfileFilterSet = value;
            OnPropertyChanged();
        }
    }

    private readonly GroupExpanderStateStore _groupExpanderStateStore = new();
    public GroupExpanderStateStore GroupExpanderStateStore => _groupExpanderStateStore;

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
                SettingsManager.Current.RemoteDesktop_ExpandProfileView = value;

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
                SettingsManager.Current.RemoteDesktop_ProfileWidth = value.Value;

            _profileWidth = value;

            if (_canProfileWidthChange)
                ResizeProfile(true);

            OnPropertyChanged();
        }
    }

    #endregion

    #endregion

    #region Constructor, load settings

    public RemoteDesktopHostViewModel(IDialogCoordinator instance)
    {
        _isLoading = true;

        _dialogCoordinator = instance;

        InterTabClient = new DragablzInterTabClient(ApplicationName.RemoteDesktop);
        InterTabPartition = nameof(ApplicationName.RemoteDesktop);

        TabItems = [];

        // Profiles
        CreateTags();

        ProfileFilterTagsView = CollectionViewSource.GetDefaultView(ProfileFilterTags);
        ProfileFilterTagsView.SortDescriptions.Add(new SortDescription(nameof(ProfileFilterTagsInfo.Name),
            ListSortDirection.Ascending));

        SetProfilesView(new ProfileFilterInfo());

        ProfileManager.OnProfilesUpdated += ProfileManager_OnProfilesUpdated;

        _searchDispatcherTimer.Interval = GlobalStaticConfiguration.SearchDispatcherTimerTimeSpan;
        _searchDispatcherTimer.Tick += SearchDispatcherTimer_Tick;

        LoadSettings();

        _isLoading = false;
    }

    private void LoadSettings()
    {
        ExpandProfileView = SettingsManager.Current.RemoteDesktop_ExpandProfileView;

        ProfileWidth = ExpandProfileView
            ? new GridLength(SettingsManager.Current.RemoteDesktop_ProfileWidth)
            : new GridLength(GlobalStaticConfiguration.Profile_WidthCollapsed);

        _tempProfileWidth = SettingsManager.Current.RemoteDesktop_ProfileWidth;
    }

    #endregion

    #region ICommand & Actions

    public ICommand ConnectCommand => new RelayCommand(_ => ConnectAction());

    private void ConnectAction()
    {
        Connect().ConfigureAwait(false);
    }

    private bool IsConnected_CanExecute(object view)
    {
        if (view is RemoteDesktopControl control)
            return control.IsConnected;

        return false;
    }

    private bool IsDisconnected_CanExecute(object view)
    {
        if (view is RemoteDesktopControl control)
            return !control.IsConnected && !control.IsConnecting;

        return false;
    }

    public ICommand ReconnectCommand => new RelayCommand(ReconnectAction, IsDisconnected_CanExecute);

    private void ReconnectAction(object view)
    {
        if (view is RemoteDesktopControl control)
            if (control.ReconnectCommand.CanExecute(null))
                control.ReconnectCommand.Execute(null);
    }

    public ICommand DisconnectCommand => new RelayCommand(DisconnectAction, IsConnected_CanExecute);

    private void DisconnectAction(object view)
    {
        if (view is RemoteDesktopControl control)
            if (control.DisconnectCommand.CanExecute(null))
                control.DisconnectCommand.Execute(null);
    }

    public ICommand FullscreenCommand => new RelayCommand(FullscreenAction, IsConnected_CanExecute);

    private void FullscreenAction(object view)
    {
        if (view is RemoteDesktopControl control)
            control.FullScreen();
    }

    public ICommand AdjustScreenCommand => new RelayCommand(AdjustScreenAction, IsConnected_CanExecute);

    private void AdjustScreenAction(object view)
    {
        if (view is RemoteDesktopControl control)
            control.AdjustScreen(force: true);
    }

    public ICommand SendCtrlAltDelCommand => new RelayCommand(SendCtrlAltDelAction, IsConnected_CanExecute);

    private async void SendCtrlAltDelAction(object view)
    {
        if (view is not RemoteDesktopControl control)
            return;

        try
        {
            control.SendKey(Keystroke.CtrlAltDel);
        }
        catch (Exception ex)
        {
            ConfigurationManager.OnDialogOpen();

            await _dialogCoordinator.ShowMessageAsync(this, Strings.Error,
                $"{Strings.CouldNotSendKeystroke}\n\nMessage:\n{ex.Message}");

            ConfigurationManager.OnDialogClose();
        }
    }

    public ICommand ConnectProfileCommand => new RelayCommand(_ => ConnectProfileAction(), ConnectProfile_CanExecute);

    private bool ConnectProfile_CanExecute(object obj)
    {
        return !IsSearching && SelectedProfile != null;
    }

    private void ConnectProfileAction()
    {
        ConnectProfile();
    }

    public ICommand ConnectProfileAsCommand => new RelayCommand(_ => ConnectProfileAsAction());

    private void ConnectProfileAsAction()
    {
        ConnectProfileAs().ConfigureAwait(false);
    }

    public ICommand ConnectProfileExternalCommand => new RelayCommand(_ => ConnectProfileExternalAction());

    private void ConnectProfileExternalAction()
    {
        Process.Start("mstsc.exe", $"/V:{SelectedProfile.RemoteDesktop_Host}");
    }

    public ICommand AddProfileCommand => new RelayCommand(_ => AddProfileAction());

    private void AddProfileAction()
    {
        ProfileDialogManager
            .ShowAddProfileDialog(Application.Current.MainWindow, this, null, null, ApplicationName.RemoteDesktop)
            .ConfigureAwait(false);
    }

    private bool ModifyProfile_CanExecute(object obj)
    {
        return SelectedProfile is { IsDynamic: false };
    }

    public ICommand EditProfileCommand => new RelayCommand(_ => EditProfileAction(), ModifyProfile_CanExecute);

    private void EditProfileAction()
    {
        ProfileDialogManager.ShowEditProfileDialog(Application.Current.MainWindow, this, SelectedProfile)
            .ConfigureAwait(false);
    }

    public ICommand CopyAsProfileCommand => new RelayCommand(_ => CopyAsProfileAction(), ModifyProfile_CanExecute);

    private void CopyAsProfileAction()
    {
        ProfileDialogManager.ShowCopyAsProfileDialog(Application.Current.MainWindow, this, SelectedProfile)
            .ConfigureAwait(false);
    }

    public ICommand DeleteProfileCommand => new RelayCommand(_ => DeleteProfileAction(), ModifyProfile_CanExecute);

    private void DeleteProfileAction()
    {
        ProfileDialogManager
            .ShowDeleteProfileDialog(Application.Current.MainWindow, this, new List<ProfileInfo> { SelectedProfile })
            .ConfigureAwait(false);
    }

    public ICommand EditGroupCommand => new RelayCommand(EditGroupAction);

    private void EditGroupAction(object group)
    {
        ProfileDialogManager
            .ShowEditGroupDialog(Application.Current.MainWindow, this, ProfileManager.GetGroupByName($"{group}"))
            .ConfigureAwait(false);
    }

    public ICommand OpenProfileFilterCommand => new RelayCommand(_ => OpenProfileFilterAction());

    private void OpenProfileFilterAction()
    {
        ProfileFilterIsOpen = true;
    }

    public ICommand ApplyProfileFilterCommand => new RelayCommand(_ => ApplyProfileFilterAction());

    private void ApplyProfileFilterAction()
    {
        RefreshProfiles();

        ProfileFilterIsOpen = false;
    }

    public ICommand ClearProfileFilterCommand => new RelayCommand(_ => ClearProfileFilterAction());

    private void ClearProfileFilterAction()
    {
        _searchDisabled = true;
        Search = string.Empty;
        _searchDisabled = false;

        foreach (var tag in ProfileFilterTags)
            tag.IsSelected = false;

        RefreshProfiles();

        IsProfileFilterSet = false;
        ProfileFilterIsOpen = false;
    }

    public ICommand ExpandAllProfileGroupsCommand => new RelayCommand(_ => ExpandAllProfileGroupsAction());

    private void ExpandAllProfileGroupsAction()
    {
        SetIsExpandedForAllProfileGroups(true);
    }

    public ICommand CollapseAllProfileGroupsCommand => new RelayCommand(_ => CollapseAllProfileGroupsAction());

    private void CollapseAllProfileGroupsAction()
    {
        SetIsExpandedForAllProfileGroups(false);
    }

    public ItemActionCallback CloseItemCommand => CloseItemAction;

    private static void CloseItemAction(ItemActionCallbackArgs<TabablzControl> args)
    {
        ((args.DragablzItem.Content as DragablzTabItem)?.View as RemoteDesktopControl)?.CloseTab();
    }

    #endregion

    #region Methods

    // Connect via Dialog
    private async Task Connect(string host = null)
    {
        var customDialog = new CustomDialog
        {
            Title = Strings.Connect
        };

        var remoteDesktopConnectViewModel = new RemoteDesktopConnectViewModel(async instance =>
        {
            await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            ConfigurationManager.OnDialogClose();

            // Create new session info with default settings
            var sessionInfo = RemoteDesktop.CreateSessionInfo();

            if (instance.Host.Contains(':'))
            {
                // Validate input via UI
                sessionInfo.Hostname = instance.Host.Split(':')[0];
                sessionInfo.Port = int.Parse(instance.Host.Split(':')[1]);
            }
            else
            {
                sessionInfo.Hostname = instance.Host;
            }

            if (instance.UseCredentials)
            {
                sessionInfo.UseCredentials = true;

                sessionInfo.Username = instance.Username;
                sessionInfo.Domain = instance.Domain;
                sessionInfo.Password = instance.Password;
            }

            sessionInfo.AdminSession = instance.AdminSession;

            // Add to history
            // Note: The history can only be updated after the values have been read.
            //       Otherwise, in some cases, incorrect values are taken over.
            AddHostToHistory(instance.Host);

            Connect(sessionInfo);
        }, async _ =>
        {
            await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            ConfigurationManager.OnDialogClose();
        })
        {
            Host = host
        };

        customDialog.Content = new RemoteDesktopConnectDialog
        {
            DataContext = remoteDesktopConnectViewModel
        };

        ConfigurationManager.OnDialogOpen();
        await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
    }

    // Connect via Profile
    private void ConnectProfile()
    {
        var profileInfo = SelectedProfile;

        var sessionInfo = RemoteDesktop.CreateSessionInfo(profileInfo);

        Connect(sessionInfo, profileInfo.Name);
    }

    // Connect via Profile with Credentials
    private async Task ConnectProfileAs()
    {
        var profileInfo = SelectedProfile;

        var sessionInfo = RemoteDesktop.CreateSessionInfo(profileInfo);

        var customDialog = new CustomDialog
        {
            Title = Strings.ConnectAs
        };

        var remoteDesktopConnectViewModel = new RemoteDesktopConnectViewModel(async instance =>
        {
            await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            ConfigurationManager.OnDialogClose();

            if (instance.UseCredentials)
            {
                sessionInfo.UseCredentials = true;

                sessionInfo.Username = instance.Username;
                sessionInfo.Domain = instance.Domain;
                sessionInfo.Password = instance.Password;
            }

            sessionInfo.AdminSession = instance.AdminSession;

            Connect(sessionInfo, instance.Name);
        }, async _ =>
        {
            await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            ConfigurationManager.OnDialogClose();
        },
        (
            profileInfo.Name,
            profileInfo.RemoteDesktop_Host,
            true
        ));

        customDialog.Content = new RemoteDesktopConnectDialog
        {
            DataContext = remoteDesktopConnectViewModel
        };

        ConfigurationManager.OnDialogOpen();
        await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
    }

    private void Connect(RemoteDesktopSessionInfo sessionInfo, string header = null)
    {
        var tabId = Guid.NewGuid();

        TabItems.Add(new DragablzTabItem(header ?? sessionInfo.Hostname, new RemoteDesktopControl(tabId, sessionInfo),
            tabId));

        SelectedTabIndex = TabItems.Count - 1;
    }

    public void AddTab(string host)
    {
        Connect(host).ConfigureAwait(false);
    }

    // Modify history list
    private static void AddHostToHistory(string host)
    {
        if (string.IsNullOrEmpty(host))
            return;

        SettingsManager.Current.RemoteDesktop_HostHistory = new ObservableCollection<string>(
            ListHelper.Modify(SettingsManager.Current.RemoteDesktop_HostHistory.ToList(), host,
                SettingsManager.Current.General_HistoryListEntries));
    }

    private void SetIsExpandedForAllProfileGroups(bool isExpanded)
    {
        foreach (var group in Profiles.Groups.Cast<CollectionViewGroup>())
            GroupExpanderStateStore[group.Name.ToString()] = isExpanded;
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

    private void CreateTags()
    {
        var tags = ProfileManager.Groups.SelectMany(x => x.Profiles).Where(x => x.RemoteDesktop_Enabled)
            .SelectMany(x => x.TagsCollection).Distinct().ToList();

        var tagSet = new HashSet<string>(tags);

        for (var i = ProfileFilterTags.Count - 1; i >= 0; i--)
        {
            if (!tagSet.Contains(ProfileFilterTags[i].Name))
                ProfileFilterTags.RemoveAt(i);
        }

        var existingTagNames = new HashSet<string>(ProfileFilterTags.Select(ft => ft.Name));

        foreach (var tag in tags.Where(tag => !existingTagNames.Contains(tag)))
        {
            ProfileFilterTags.Add(new ProfileFilterTagsInfo(false, tag));
        }
    }

    private void SetProfilesView(ProfileFilterInfo filter, ProfileInfo profile = null)
    {
        Profiles = new CollectionViewSource
        {
            Source = ProfileManager.Groups.SelectMany(x => x.Profiles).Where(x => x.RemoteDesktop_Enabled && (
                    string.IsNullOrEmpty(filter.Search) ||
                    x.Name.IndexOf(filter.Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                    x.RemoteDesktop_Host.IndexOf(filter.Search, StringComparison.OrdinalIgnoreCase) > -1) && (
                    // If no tags are selected, show all profiles
                    (!filter.Tags.Any()) ||
                    // Any tag can match
                    (filter.TagsFilterMatch == ProfileFilterTagsMatch.Any &&
                     filter.Tags.Any(tag => x.TagsCollection.Contains(tag))) ||
                    // All tags must match
                    (filter.TagsFilterMatch == ProfileFilterTagsMatch.All &&
                     filter.Tags.All(tag => x.TagsCollection.Contains(tag))))
            ).OrderBy(x => x.Group).ThenBy(x => x.Name)
        }.View;

        Profiles.GroupDescriptions.Add(new PropertyGroupDescription(nameof(ProfileInfo.Group)));

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

        var filter = new ProfileFilterInfo
        {
            Search = Search,
            Tags = [.. ProfileFilterTags.Where(x => x.IsSelected).Select(x => x.Name)],
            TagsFilterMatch = ProfileFilterTagsMatchAny ? ProfileFilterTagsMatch.Any : ProfileFilterTagsMatch.All
        };

        SetProfilesView(filter, SelectedProfile);

        IsProfileFilterSet = !string.IsNullOrEmpty(filter.Search) || filter.Tags.Any();
    }

    public void OnProfileManagerDialogOpen()
    {
        ConfigurationManager.OnDialogOpen();
    }

    public void OnProfileManagerDialogClose()
    {
        ConfigurationManager.OnDialogClose();
    }

    #endregion

    #region Event

    private void ProfileManager_OnProfilesUpdated(object sender, EventArgs e)
    {
        CreateTags();

        RefreshProfiles();
    }

    private void SearchDispatcherTimer_Tick(object sender, EventArgs e)
    {
        _searchDispatcherTimer.Stop();

        RefreshProfiles();

        IsSearching = false;
    }

    #endregion
}