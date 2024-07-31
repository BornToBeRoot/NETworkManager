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
using Dragablz;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models;
using NETworkManager.Models.EventSystem;
using NETworkManager.Models.PowerShell;
using NETworkManager.Profiles;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using PowerShell = NETworkManager.Profiles.Application.PowerShell;

namespace NETworkManager.ViewModels;

public class PowerShellHostViewModel : ViewModelBase, IProfileManager
{
    #region Variables

    private readonly IDialogCoordinator _dialogCoordinator;
    private readonly DispatcherTimer _searchDispatcherTimer = new();

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
    private bool _disableFocusEmbeddedWindow;

    private bool _isConfigured;

    public bool IsConfigured
    {
        get => _isConfigured;
        set
        {
            if (value == _isConfigured)
                return;

            _isConfigured = value;
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

    /*
    private DragablzTabItem _selectedTabItem;

    public DragablzTabItem SelectedTabItem
    {
        get => _selectedTabItem;
        set
        {
            if (value == _selectedTabItem)
                return;

            _selectedTabItem = value;

            // Focus embedded window on switching tab
            if (!_disableFocusEmbeddedWindow)
                FocusEmbeddedWindow();

            OnPropertyChanged();
        }
    }
    */

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
            IsSearching = true;
            _searchDispatcherTimer.Start();

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
                SettingsManager.Current.PowerShell_ExpandProfileView = value;

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
                SettingsManager.Current.PowerShell_ProfileWidth = value.Value;

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

    public PowerShellHostViewModel(IDialogCoordinator instance)
    {
        _isLoading = true;

        _dialogCoordinator = instance;

        CheckSettings();

        InterTabClient = new DragablzInterTabClient(ApplicationName.PowerShell);
        InterTabPartition = ApplicationName.PowerShell.ToString();

        TabItems = [];

        // Profiles
        SetProfilesView();

        ProfileManager.OnProfilesUpdated += ProfileManager_OnProfilesUpdated;

        _searchDispatcherTimer.Interval = GlobalStaticConfiguration.SearchDispatcherTimerTimeSpan;
        _searchDispatcherTimer.Tick += SearchDispatcherTimer_Tick;

        LoadSettings();

        SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;

        _isLoading = false;
    }

    private void LoadSettings()
    {
        ExpandProfileView = SettingsManager.Current.PowerShell_ExpandProfileView;

        ProfileWidth = ExpandProfileView
            ? new GridLength(SettingsManager.Current.PowerShell_ProfileWidth)
            : new GridLength(GlobalStaticConfiguration.Profile_WidthCollapsed);

        _tempProfileWidth = SettingsManager.Current.PowerShell_ProfileWidth;
    }

    #endregion

    #region ICommand & Actions

    public ItemActionCallback CloseItemCommand => CloseItemAction;

    private void CloseItemAction(ItemActionCallbackArgs<TabablzControl> args)
    {
        ((args.DragablzItem.Content as DragablzTabItem)?.View as PowerShellControl)?.CloseTab();
    }

    private bool Connect_CanExecute(object obj)
    {
        return IsConfigured;
    }

    public ICommand ConnectCommand => new RelayCommand(_ => ConnectAction(), Connect_CanExecute);

    private void ConnectAction()
    {
        Connect().ConfigureAwait(false);
    }

    private bool IsConnected_CanExecute(object view)
    {
        if (view is PowerShellControl control)
            return control.IsConnected;

        return false;
    }

    public ICommand ReconnectCommand => new RelayCommand(ReconnectAction);

    private void ReconnectAction(object view)
    {
        if (view is not PowerShellControl control)
            return;

        if (control.ReconnectCommand.CanExecute(null))
            control.ReconnectCommand.Execute(null);
    }

    public ICommand ResizeWindowCommand => new RelayCommand(ResizeWindowAction, IsConnected_CanExecute);

    private void ResizeWindowAction(object view)
    {
        if (view is PowerShellControl control)
            control.ResizeEmbeddedWindow();
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
            .ShowAddProfileDialog(this, this, _dialogCoordinator, null, null, ApplicationName.PowerShell)
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

    public ICommand TextBoxSearchGotFocusCommand
    {
        get { return new RelayCommand(_ => _textBoxSearchIsFocused = true); }
    }

    public ICommand TextBoxSearchLostFocusCommand
    {
        get { return new RelayCommand(_ => _textBoxSearchIsFocused = false); }
    }

    public ICommand ClearSearchCommand => new RelayCommand(_ => ClearSearchAction());

    private void ClearSearchAction()
    {
        Search = string.Empty;
    }

    public ICommand OpenSettingsCommand => new RelayCommand(_ => OpenSettingsAction());

    private static void OpenSettingsAction()
    {
        EventSystem.RedirectToSettings();
    }

    #endregion

    #region Methods

    private void CheckSettings()
    {
        IsConfigured = !string.IsNullOrEmpty(SettingsManager.Current.PowerShell_ApplicationFilePath) &&
                       File.Exists(SettingsManager.Current.PowerShell_ApplicationFilePath);
    }

    private async Task Connect(string host = null)
    {
        var customDialog = new CustomDialog
        {
            Title = Strings.Connect
        };

        var connectViewModel = new PowerShellConnectViewModel(async instance =>
        {
            await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            ConfigurationManager.OnDialogClose();

            // Create profile info
            var sessionInfo = new PowerShellSessionInfo
            {
                EnableRemoteConsole = instance.EnableRemoteConsole,
                Host = instance.Host,
                Command = instance.Command,
                AdditionalCommandLine = instance.AdditionalCommandLine,
                ExecutionPolicy = instance.ExecutionPolicy
            };

            // Add to history
            // Note: The history can only be updated after the values have been read.
            //       Otherwise, in some cases, incorrect values are taken over.
            AddHostToHistory(instance.Host);

            // Connect
            Connect(sessionInfo);
        }, async _ =>
        {
            await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            ConfigurationManager.OnDialogClose();
        }, host);

        customDialog.Content = new PowerShellConnectDialog
        {
            DataContext = connectViewModel
        };

        ConfigurationManager.OnDialogOpen();
        await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
    }

    private void ConnectProfile()
    {
        Connect(PowerShell.CreateSessionInfo(SelectedProfile),
            SelectedProfile.Name);
    }

    private void ConnectProfileExternal()
    {
        var sessionInfo =
            PowerShell.CreateSessionInfo(SelectedProfile);

        Process.Start(new ProcessStartInfo
        {
            FileName = SettingsManager.Current.PowerShell_ApplicationFilePath,
            Arguments = Models.PowerShell.PowerShell.BuildCommandLine(sessionInfo)
        });
    }

    private void Connect(PowerShellSessionInfo sessionInfo, string header = null)
    {
        sessionInfo.ApplicationFilePath = SettingsManager.Current.PowerShell_ApplicationFilePath;

        var tabId = Guid.NewGuid();

        TabItems.Add(new DragablzTabItem(
            header ?? (sessionInfo.EnableRemoteConsole ? sessionInfo.Host : Strings.PowerShell),
            new PowerShellControl(tabId, sessionInfo), tabId));

        // Select the added tab
        _disableFocusEmbeddedWindow = true;
        SelectedTabIndex = TabItems.Count - 1;
        _disableFocusEmbeddedWindow = false;
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

        SettingsManager.Current.PowerShell_HostHistory = new ObservableCollection<string>(
            ListHelper.Modify(SettingsManager.Current.PowerShell_HostHistory.ToList(), host,
                SettingsManager.Current.General_HistoryListEntries));
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

    private void SetProfilesView(ProfileInfo profile = null)
    {
        Profiles = new CollectionViewSource
        {
            Source = ProfileManager.Groups.SelectMany(x => x.Profiles).Where(x => x.PowerShell_Enabled)
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

            // Search by: Name, PowerShell_Host
            return info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   info.PowerShell_Host.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1;
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
        RefreshProfiles();
    }

    private void SearchDispatcherTimer_Tick(object sender, EventArgs e)
    {
        _searchDispatcherTimer.Stop();

        RefreshProfiles();

        IsSearching = false;
    }

    private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SettingsInfo.PowerShell_ApplicationFilePath))
            CheckSettings();
    }

    #endregion
}