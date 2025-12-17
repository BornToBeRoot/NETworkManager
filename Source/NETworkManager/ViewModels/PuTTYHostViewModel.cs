using Dragablz;
using log4net;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models;
using NETworkManager.Models.EventSystem;
using NETworkManager.Models.PuTTY;
using NETworkManager.Profiles;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using PuTTYProfile = NETworkManager.Settings.Application.PuTTY;

namespace NETworkManager.ViewModels;

public class PuTTYHostViewModel : ViewModelBase, IProfileManager
{
    #region Variables

    private static readonly ILog Log = LogManager.GetLogger(typeof(PuTTYHostViewModel));

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

    private bool _isExecutableConfigured;

    public bool IsExecutableConfigured
    {
        get => _isExecutableConfigured;
        set
        {
            if (value == _isExecutableConfigured)
                return;

            _isExecutableConfigured = value;
            OnPropertyChanged();
        }
    }

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

    private bool _headerContextMenuIsOpen;

    public bool HeaderContextMenuIsOpen
    {
        get => _headerContextMenuIsOpen;
        set
        {
            if (value == _headerContextMenuIsOpen)
                return;

            _headerContextMenuIsOpen = value;
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

    private bool _textBoxSearchIsFocused;

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
                SettingsManager.Current.PuTTY_ExpandProfileView = value;

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
                SettingsManager.Current.PuTTY_ProfileWidth = value.Value;

            _profileWidth = value;

            if (_canProfileWidthChange)
                ResizeProfile(true);

            OnPropertyChanged();
        }
    }

    private bool _profileContextMenuIsOpen;

    public bool ProfileContextMenuIsOpen
    {
        get => _profileContextMenuIsOpen;
        set
        {
            if (value == _profileContextMenuIsOpen)
                return;

            _profileContextMenuIsOpen = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #endregion

    #region Constructor, load settings

    public PuTTYHostViewModel()
    {
        _isLoading = true;

        // Check if PuTTY executable is configured
        CheckExecutable();

        // Try to find PuTTY executable
        if (!IsExecutableConfigured)
            TryFindExecutable();

        WriteDefaultProfileToRegistry();

        InterTabClient = new DragablzInterTabClient(ApplicationName.PuTTY);
        InterTabPartition = nameof(ApplicationName.PuTTY);

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

        SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;

        _isLoading = false;
    }

    private void LoadSettings()
    {
        ExpandProfileView = SettingsManager.Current.PuTTY_ExpandProfileView;

        ProfileWidth = ExpandProfileView
            ? new GridLength(SettingsManager.Current.PuTTY_ProfileWidth)
            : new GridLength(GlobalStaticConfiguration.Profile_WidthCollapsed);

        _tempProfileWidth = SettingsManager.Current.PuTTY_ProfileWidth;
    }

    #endregion

    #region ICommand & Actions

    public ItemActionCallback CloseItemCommand => CloseItemAction;

    private void CloseItemAction(ItemActionCallbackArgs<TabablzControl> args)
    {
        ((args.DragablzItem.Content as DragablzTabItem)?.View as PuTTYControl)?.CloseTab();
    }

    private bool Connect_CanExecute(object obj)
    {
        return IsExecutableConfigured;
    }

    public ICommand ConnectCommand => new RelayCommand(_ => ConnectAction(), Connect_CanExecute);

    private void ConnectAction()
    {
        Connect().ConfigureAwait(false);
    }

    private bool IsConnected_CanExecute(object view)
    {
        if (view is PuTTYControl control)
            return control.IsConnected;

        return false;
    }

    public ICommand ReconnectCommand => new RelayCommand(ReconnectAction);

    private void ReconnectAction(object view)
    {
        if (view is not PuTTYControl control)
            return;

        if (control.ReconnectCommand.CanExecute(null))
            control.ReconnectCommand.Execute(null);
    }

    public ICommand ResizeWindowCommand => new RelayCommand(ResizeWindowAction, IsConnected_CanExecute);

    private void ResizeWindowAction(object view)
    {
        if (view is PuTTYControl control)
            control.ResizeEmbeddedWindow();
    }

    public ICommand RestartSessionCommand => new RelayCommand(RestartSessionAction, IsConnected_CanExecute);

    private void RestartSessionAction(object view)
    {
        if (view is PuTTYControl control)
            control.RestartSession();
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

    public ICommand ConnectProfileExternalCommand => new RelayCommand(_ => ConnectProfileExternalAction());

    private void ConnectProfileExternalAction()
    {
        ConnectProfileExternal();
    }

    public ICommand AddProfileCommand => new RelayCommand(_ => AddProfileAction());

    private void AddProfileAction()
    {
        ProfileDialogManager
            .ShowAddProfileDialog(Application.Current.MainWindow, this, null, null, ApplicationName.PuTTY)
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

    public ICommand TextBoxSearchGotFocusCommand
    {
        get { return new RelayCommand(_ => _textBoxSearchIsFocused = true); }
    }

    public ICommand TextBoxSearchLostFocusCommand
    {
        get { return new RelayCommand(_ => _textBoxSearchIsFocused = false); }
    }

    public ICommand OpenProfileFilterCommand => new RelayCommand(_ => OpenProfileFilterAction());

    private void OpenProfileFilterAction()
    {
        ConfigurationManager.Current.IsProfileFilterPopupOpen = true;

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

    public ICommand OpenSettingsCommand => new RelayCommand(_ => OpenSettingsAction());

    private static void OpenSettingsAction()
    {
        EventSystem.RedirectToSettings();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Check if executable is configured and exists.
    /// </summary>
    private void CheckExecutable()
    {
        IsExecutableConfigured = !string.IsNullOrEmpty(SettingsManager.Current.PuTTY_ApplicationFilePath) &&
                                 File.Exists(SettingsManager.Current.PuTTY_ApplicationFilePath);

        if (IsExecutableConfigured)
            Log.Info($"PuTTY executable configured: \"{SettingsManager.Current.PuTTY_ApplicationFilePath}\"");
        else
            Log.Warn("PuTTY executable not found!");
    }

    /// <summary>
    /// Try to find executable.
    /// </summary>
    private void TryFindExecutable()
    {
        Log.Info("Try to find PuTTY executable...");

        SettingsManager.Current.PuTTY_ApplicationFilePath = ApplicationHelper.Find(Models.PuTTY.PuTTY.FileName);

        CheckExecutable();

        if (!IsExecutableConfigured)
            Log.Warn("Install PuTTY or configure the path in the settings.");
    }

    private Task Connect(string host = null)
    {
        var childWindow = new PuTTYConnectChildWindow();

        var childWindowViewModel = new PuTTYConnectViewModel(instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            ConfigurationManager.OnDialogClose();

            // Create profile info
            var sessionInfo = new PuTTYSessionInfo
            {
                HostOrSerialLine = instance.ConnectionMode == ConnectionMode.Serial
                    ? instance.SerialLine
                    : instance.Host,
                Mode = instance.ConnectionMode,
                PortOrBaud = instance.ConnectionMode == ConnectionMode.Serial ? instance.Baud : instance.Port,
                Username = instance.Username,
                PrivateKey = instance.PrivateKeyFile,
                Profile = instance.Profile,
                EnableLog = SettingsManager.Current.PuTTY_EnableSessionLog,
                LogMode = SettingsManager.Current.PuTTY_LogMode,
                LogFileName = SettingsManager.Current.PuTTY_LogFileName,
                LogPath = PuTTYProfile.LogPath,
                AdditionalCommandLine = instance.AdditionalCommandLine
            };

            // Add to history
            // Note: The history can only be updated after the values have been read.
            //       Otherwise, in some cases, incorrect values are taken over.
            AddHostToHistory(instance.Host);
            AddSerialLineToHistory(instance.SerialLine);
            AddPortToHistory(instance.Port);
            AddBaudToHistory(instance.Baud);
            AddUsernameToHistory(instance.Username);
            AddPrivateKeyToHistory(instance.PrivateKeyFile);
            AddProfileToHistory(instance.Profile);

            Connect(sessionInfo);
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            ConfigurationManager.OnDialogClose();
        }, host);

        childWindow.Title = Strings.Connect;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        ConfigurationManager.OnDialogOpen();

        return Application.Current.MainWindow.ShowChildWindowAsync(childWindow);
    }

    private void ConnectProfile()
    {
        Connect(NETworkManager.Profiles.Application.PuTTY.CreateSessionInfo(SelectedProfile), SelectedProfile.Name);
    }

    private void ConnectProfileExternal()
    {
        // Create log path
        DirectoryHelper.CreateWithEnvironmentVariables(PuTTYProfile.LogPath);

        var sessionInfo = NETworkManager.Profiles.Application.PuTTY.CreateSessionInfo(SelectedProfile);

        ProcessStartInfo info = new()
        {
            FileName = SettingsManager.Current.PuTTY_ApplicationFilePath,
            Arguments = PuTTY.BuildCommandLine(sessionInfo)
        };

        Process.Start(info);
    }

    private void Connect(PuTTYSessionInfo sessionInfo, string header = null)
    {
        // Must be added here. So that it works with profiles and the connect dialog.
        sessionInfo.ApplicationFilePath = SettingsManager.Current.PuTTY_ApplicationFilePath;

        var tabId = Guid.NewGuid();

        TabItems.Add(new DragablzTabItem(header ?? sessionInfo.HostOrSerialLine, new PuTTYControl(tabId, sessionInfo),
            tabId));

        // Select the added tab
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

        SettingsManager.Current.PuTTY_HostHistory = new ObservableCollection<string>(
            ListHelper.Modify(SettingsManager.Current.PuTTY_HostHistory.ToList(), host,
                SettingsManager.Current.General_HistoryListEntries));
    }

    private static void AddSerialLineToHistory(string serialLine)
    {
        if (string.IsNullOrEmpty(serialLine))
            return;

        SettingsManager.Current.PuTTY_SerialLineHistory = new ObservableCollection<string>(
            ListHelper.Modify(SettingsManager.Current.PuTTY_SerialLineHistory.ToList(), serialLine,
                SettingsManager.Current.General_HistoryListEntries));
    }

    private static void AddPortToHistory(int port)
    {
        if (port == 0)
            return;

        SettingsManager.Current.PuTTY_PortHistory = new ObservableCollection<string>(
            ListHelper.Modify(SettingsManager.Current.PuTTY_PortHistory.ToList(), port.ToString(),
                SettingsManager.Current.General_HistoryListEntries));
    }

    private static void AddBaudToHistory(int baud)
    {
        if (baud == 0)
            return;

        SettingsManager.Current.PuTTY_BaudHistory = new ObservableCollection<string>(
            ListHelper.Modify(SettingsManager.Current.PuTTY_BaudHistory.ToList(), baud.ToString(),
                SettingsManager.Current.General_HistoryListEntries));
    }

    private static void AddUsernameToHistory(string username)
    {
        if (string.IsNullOrEmpty(username))
            return;

        SettingsManager.Current.PuTTY_UsernameHistory = new ObservableCollection<string>(
            ListHelper.Modify(SettingsManager.Current.PuTTY_UsernameHistory.ToList(), username,
                SettingsManager.Current.General_HistoryListEntries));
    }

    private static void AddPrivateKeyToHistory(string privateKey)
    {
        if (string.IsNullOrEmpty(privateKey))
            return;

        SettingsManager.Current.PuTTY_PrivateKeyFileHistory = new ObservableCollection<string>(
            ListHelper.Modify(SettingsManager.Current.PuTTY_PrivateKeyFileHistory.ToList(), privateKey,
                SettingsManager.Current.General_HistoryListEntries));
    }

    private static void AddProfileToHistory(string profile)
    {
        if (string.IsNullOrEmpty(profile))
            return;

        SettingsManager.Current.PuTTY_ProfileHistory = new ObservableCollection<string>(
            ListHelper.Modify(SettingsManager.Current.PuTTY_ProfileHistory.ToList(), profile,
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

    public void FocusEmbeddedWindow()
    {
        /* Don't continue if
           - Search TextBox is focused
           - Header ContextMenu is opened
           - Profile ContextMenu is opened
        */
        if (_textBoxSearchIsFocused || HeaderContextMenuIsOpen || ProfileContextMenuIsOpen)
            return;

        var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

        if (window == null)
            return;

        // Find all TabablzControl in the active window
        foreach (var tabablzControl in VisualTreeHelper.FindVisualChildren<TabablzControl>(window))
        {
            // Skip if no items
            if (tabablzControl.Items.Count == 0)
                continue;

            // Focus embedded window in the selected tab
            (((DragablzTabItem)tabablzControl.SelectedItem)?.View as IEmbeddedWindow)?.FocusEmbeddedWindow();

            break;
        }
    }

    public void OnViewVisible()
    {
        _isViewActive = true;

        RefreshProfiles();

        FocusEmbeddedWindow();
    }

    public void OnViewHide()
    {
        _isViewActive = false;
    }

    private void CreateTags()
    {
        var tags = ProfileManager.Groups.SelectMany(x => x.Profiles).Where(x => x.PuTTY_Enabled)
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
            Source = ProfileManager.Groups.SelectMany(x => x.Profiles).Where(x => x.PuTTY_Enabled && (
                    string.IsNullOrEmpty(filter.Search) ||
                    x.Name.IndexOf(filter.Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                    x.PuTTY_HostOrSerialLine.IndexOf(filter.Search, StringComparison.OrdinalIgnoreCase) > -1) && (
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

    public void OnProfileFilterClosed()
    {
        ConfigurationManager.Current.IsProfileFilterPopupOpen = false;
    }

    public void OnProfileManagerDialogOpen()
    {
        ConfigurationManager.OnDialogOpen();
    }

    public void OnProfileManagerDialogClose()
    {
        ConfigurationManager.OnDialogClose();
    }

    private void WriteDefaultProfileToRegistry()
    {
        if (!IsExecutableConfigured)
            return;

        Log.Debug("Write PuTTY profile to registry...");

        PuTTY.WriteDefaultProfileToRegistry(SettingsManager.Current.Appearance_Theme);
    }

    #endregion

    #region Event

    private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SettingsInfo.PuTTY_ApplicationFilePath):
                CheckExecutable();
                break;
            case nameof(SettingsInfo.Appearance_Theme):
                WriteDefaultProfileToRegistry();
                break;
        }
    }

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