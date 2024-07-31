using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Amazon;
using Amazon.EC2;
using Amazon.EC2.Model;
using Amazon.Runtime.CredentialManagement;
using Dragablz;
using log4net;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using NETworkManager.Controls;
using NETworkManager.Documentation;
using NETworkManager.Localization.Resources;
using NETworkManager.Models;
using NETworkManager.Models.AWS;
using NETworkManager.Models.EventSystem;
using NETworkManager.Profiles;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using AWSSessionManager = NETworkManager.Profiles.Application.AWSSessionManager;

namespace NETworkManager.ViewModels;

public class AWSSessionManagerHostViewModel : ViewModelBase, IProfileManager
{
    #region Variables

    private static readonly ILog Log = LogManager.GetLogger(typeof(AWSSessionManagerHostViewModel));
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

    private bool _isAWSCLIInstalled;

    public bool IsAWSCLIInstalled
    {
        get => _isAWSCLIInstalled;
        set
        {
            if (value == _isAWSCLIInstalled)
                return;

            _isAWSCLIInstalled = value;
            OnPropertyChanged();
        }
    }

    private bool _isAWSSessionManagerPluginInstalled;

    public bool IsAWSSessionManagerPluginInstalled
    {
        get => _isAWSSessionManagerPluginInstalled;
        set
        {
            if (value == _isAWSSessionManagerPluginInstalled)
                return;

            _isAWSSessionManagerPluginInstalled = value;
            OnPropertyChanged();
        }
    }

    private bool _isPowerShellConfigured;

    public bool IsPowerShellConfigured
    {
        get => _isPowerShellConfigured;
        set
        {
            if (value == _isPowerShellConfigured)
                return;

            _isPowerShellConfigured = value;
            OnPropertyChanged();
        }
    }

    private bool _isSyncEnabled;

    public bool IsSyncEnabled
    {
        get => _isSyncEnabled;
        set
        {
            if (value == _isSyncEnabled)
                return;

            _isSyncEnabled = value;
            OnPropertyChanged();
        }
    }

    private bool _isSyncing;

    public bool IsSyncing
    {
        get => _isSyncing;
        set
        {
            if (value == _isSyncing)
                return;

            _isSyncing = value;
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
                SettingsManager.Current.AWSSessionManager_ExpandProfileView = value;

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
                SettingsManager.Current.AWSSessionManager_ProfileWidth = value.Value;

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

    public AWSSessionManagerHostViewModel(IDialogCoordinator instance)
    {
        _isLoading = true;

        _dialogCoordinator = instance;

        CheckInstallationStatus();
        CheckSettings();

        InterTabClient = new DragablzInterTabClient(ApplicationName.AWSSessionManager);
        InterTabPartition = ApplicationName.AWSSessionManager.ToString();

        TabItems = [];

        // Profiles
        SetProfilesView();

        ProfileManager.OnProfilesUpdated += ProfileManager_OnProfilesUpdated;

        _searchDispatcherTimer.Interval = GlobalStaticConfiguration.SearchDispatcherTimerTimeSpan;
        _searchDispatcherTimer.Tick += SearchDispatcherTimer_Tick;

        LoadSettings();

        SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;
        SettingsManager.Current.AWSSessionManager_AWSProfiles.CollectionChanged +=
            AWSSessionManager_AWSProfiles_CollectionChanged;

        SyncAllInstanceIDsFromAWS().ConfigureAwait(false);

        _isLoading = false;
    }

    private void LoadSettings()
    {
        IsSyncEnabled = SettingsManager.Current.AWSSessionManager_EnableSyncInstanceIDsFromAWS;

        ExpandProfileView = SettingsManager.Current.AWSSessionManager_ExpandProfileView;

        ProfileWidth = ExpandProfileView
            ? new GridLength(SettingsManager.Current.AWSSessionManager_ProfileWidth)
            : new GridLength(GlobalStaticConfiguration.Profile_WidthCollapsed);

        _tempProfileWidth = SettingsManager.Current.AWSSessionManager_ProfileWidth;
    }

    #endregion

    #region ICommand & Actions

    public ICommand CheckInstallationStatusCommand => new RelayCommand(_ => CheckInstallationStatusAction());

    private void CheckInstallationStatusAction()
    {
        CheckInstallationStatus();
    }

    public ItemActionCallback CloseItemCommand => CloseItemAction;

    private void CloseItemAction(ItemActionCallbackArgs<TabablzControl> args)
    {
        ((args.DragablzItem.Content as DragablzTabItem)?.View as AWSSessionManagerControl)?.CloseTab();
    }

    private bool Connect_CanExecute(object obj)
    {
        return IsPowerShellConfigured;
    }

    public ICommand ConnectCommand => new RelayCommand(_ => ConnectAction(), Connect_CanExecute);

    private void ConnectAction()
    {
        Connect().ConfigureAwait(false);
    }

    private bool IsConnected_CanExecute(object view)
    {
        if (view is AWSSessionManagerControl control)
            return control.IsConnected;

        return false;
    }

    public ICommand ReconnectCommand => new RelayCommand(ReconnectAction);

    private void ReconnectAction(object view)
    {
        if (view is AWSSessionManagerControl control)
            if (control.ReconnectCommand.CanExecute(null))
                control.ReconnectCommand.Execute(null);
    }

    public ICommand ResizeWindowCommand => new RelayCommand(ResizeWindowAction, IsConnected_CanExecute);

    private void ResizeWindowAction(object view)
    {
        if (view is AWSSessionManagerControl control)
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
            .ShowAddProfileDialog(this, this, _dialogCoordinator, null, null, ApplicationName.AWSSessionManager)
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

    private bool SyncInstanceIDsFromAWS_CanExecute(object obj)
    {
        return !IsSyncing && IsSyncEnabled;
    }

    public ICommand SyncAllInstanceIDsFromAWSCommand =>
        new RelayCommand(_ => SyncAllInstanceIDsFromAWSAction(), SyncInstanceIDsFromAWS_CanExecute);

    private void SyncAllInstanceIDsFromAWSAction()
    {
        SyncAllInstanceIDsFromAWS().ConfigureAwait(false);
    }

    public ICommand SyncGroupInstanceIDsFromAWSCommand =>
        new RelayCommand(SyncGroupInstanceIDsFromAWSAction, SyncInstanceIDsFromAWS_CanExecute);

    private void SyncGroupInstanceIDsFromAWSAction(object group)
    {
        SyncGroupInstanceIDsFromAWS((string)group).ConfigureAwait(false);
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

    public ICommand OpenDocumentationCommand
    {
        get { return new RelayCommand(_ => OpenDocumentationAction()); }
    }

    private void OpenDocumentationAction()
    {
        DocumentationManager.OpenDocumentation(DocumentationIdentifier.ApplicationAWSSessionManager);
    }

    public ICommand OpenSettingsCommand => new RelayCommand(_ => OpenSettingsAction());

    private static void OpenSettingsAction()
    {
        EventSystem.RedirectToSettings();
    }

    #endregion

    #region Methods

    private void CheckInstallationStatus()
    {
        using var key =
            Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");

        if (key == null)
            return;

        foreach (var subKeyName in key.GetSubKeyNames())
        {
            using var subKey = key.OpenSubKey(subKeyName);

            var displayName = subKey?.GetValue("DisplayName");

            switch (displayName)
            {
                case null:
                    continue;
                case "AWS Command Line Interface v2":
                    IsAWSCLIInstalled = true;
                    break;
                case "Session Manager Plugin":
                    IsAWSSessionManagerPluginInstalled = true;
                    break;
            }
        }
    }

    private void CheckSettings()
    {
        IsPowerShellConfigured = !string.IsNullOrEmpty(SettingsManager.Current.AWSSessionManager_ApplicationFilePath) &&
                                 File.Exists(SettingsManager.Current.AWSSessionManager_ApplicationFilePath);
    }

    private bool IsConfigured => IsAWSCLIInstalled && IsAWSSessionManagerPluginInstalled && IsPowerShellConfigured;

    private async Task SyncAllInstanceIDsFromAWS()
    {
        if (!IsSyncEnabled)
        {
            Log.Info("Sync all EC2 instances from AWS is disabled in the settings.");
            return;
        }

        Log.Info("Sync all EC2 instance(s) from AWS...");

        if (!IsConfigured)
        {
            Log.Warn(
                $"Preconditions not met! AWS CLI installed {IsAWSCLIInstalled}. AWS Session Manager plugin installed {IsAWSSessionManagerPluginInstalled}. PowerShell configured {IsPowerShellConfigured}.");
            return;
        }

        if (IsSyncing)
        {
            Log.Info("Skip... Sync is already running!");
            return;
        }

        // Check if profiles are available
        if (ProfileManager.LoadedProfileFile == null)
        {
            Log.Warn("Profile file is not loaded (or decrypted)! Please select (or unlock) a profile file first.");
            return;
        }

        IsSyncing = true;

        foreach (var profile in SettingsManager.Current.AWSSessionManager_AWSProfiles)
        {
            if (!profile.IsEnabled)
            {
                Log.Info($"Skip AWS profile \"[{profile.Profile}\\{profile.Region}]\" because it is disabled!");
                continue;
            }

            await SyncInstanceIDsFromAWS(profile.Profile, profile.Region);
        }

        // Make the user happy, let him see a reload animation (and he cannot spam the reload command)        
        await Task.Delay(2000);

        Log.Info("All Instance IDs synced from AWS!");

        IsSyncing = false;
    }

    private async Task SyncGroupInstanceIDsFromAWS(string group)
    {
        Log.Info($"Sync group \"{group}\"...");

        IsSyncing = true;

        // Extract "profile\region" from "~ [profile\region]"
        Regex regex = new(@"\[(.*?)\]");
        var result = regex.Match(group);

        if (result.Success)
        {
            // Split "profile\region" into profile and region
            var groupData = result.Groups[1].Value.Split(@"\");
            await SyncInstanceIDsFromAWS(groupData[0], groupData[1]);
        }
        else
        {
            Log.Error($"Could not extract AWS profile and AWS region from \"{group}\"!");
        }

        // Make the user happy, let him see a reload animation (and he cannot spam the reload command)        
        await Task.Delay(2000);

        Log.Info("Group synced!");

        IsSyncing = false;
    }

    private async Task SyncInstanceIDsFromAWS(string profile, string region)
    {
        Log.Info($"Sync EC2 Instance(s) for AWS profile \"[{profile}\\{region}]\"...");

        CredentialProfileStoreChain credentialProfileStoreChain = new();
        credentialProfileStoreChain.TryGetAWSCredentials(profile, out var credentials);

        if (credentials == null)
        {
            Log.Error(
                $"Could not detect AWS credentials for AWS profile \"{profile}\"! You can configure them in the file \"%USERPROFILE%\\.aws\\config\" or via aws cli with the command \"aws configure --profile <NAME>\" ");
            return;
        }

        using AmazonEC2Client client = new(credentials, RegionEndpoint.GetBySystemName(region));

        DescribeInstancesResponse response;

        try
        {
            response = await client.DescribeInstancesAsync();
        }
        catch (AmazonEC2Exception ex)
        {
            Log.Error($"Could not get EC2 Instance(s) from AWS! Error message: \"{ex.Message}\"");
            return;
        }

        var groupName = $"~ [{profile}\\{region}]";

        // Create a new group info for profiles
        var groupInfo = new GroupInfo
        {
            Name = groupName,
            IsDynamic = true
        };

        foreach (var reservation in response.Reservations)
        foreach (var instance in reservation.Instances)
        {
            if (SettingsManager.Current.AWSSessionManager_SyncOnlyRunningInstancesFromAWS &&
                instance.State.Name.Value != "running")
                continue;

            var tagName = instance.Tags.FirstOrDefault(x => x.Key == "Name");

            var name = tagName == null || tagName.Value == null
                ? instance.InstanceId
                : $"{tagName.Value} ({instance.InstanceId})";

            groupInfo.Profiles.Add(new ProfileInfo
            {
                Name = name,
                Host = instance.InstanceId,
                Group = $"~ [{profile}\\{region}]",
                IsDynamic = true,

                AWSSessionManager_Enabled = true,
                AWSSessionManager_InstanceID = instance.InstanceId,
                AWSSessionManager_OverrideProfile = true,
                AWSSessionManager_Profile = profile,
                AWSSessionManager_OverrideRegion = true,
                AWSSessionManager_Region = region
            });
        }

        // Remove, replace or add group
        var profilesChangedCurrentState = ProfileManager.ProfilesChanged;
        ProfileManager.ProfilesChanged = false;

        if (groupInfo.Profiles.Count == 0)
        {
            if (ProfileManager.GroupExists(groupName))
                ProfileManager.RemoveGroup(ProfileManager.GetGroup(groupName));

            Log.Info("No EC2 Instance(s) found!");
        }
        else
        {
            if (ProfileManager.GroupExists(groupName))
                ProfileManager.ReplaceGroup(ProfileManager.GetGroup(groupName), groupInfo);
            else
                ProfileManager.AddGroup(groupInfo);

            Log.Info($"Found {groupInfo.Profiles.Count} EC2 Instance(s) and added them to the group \"{groupName}\"!");
        }

        ProfileManager.ProfilesChanged = profilesChangedCurrentState;
    }

    private void RemoveDynamicGroups()
    {
        foreach (var profile in SettingsManager.Current.AWSSessionManager_AWSProfiles)
        {
            if (!profile.IsEnabled)
                continue;

            RemoveDynamicGroup(profile.Profile, profile.Region);
        }
    }

    private void RemoveDynamicGroup(string profile, string region)
    {
        var groupName = $"~ [{profile}\\{region}]";

        var profilesChangedCurrentState = ProfileManager.ProfilesChanged;
        ProfileManager.ProfilesChanged = false;

        if (ProfileManager.GroupExists(groupName))
            ProfileManager.RemoveGroup(ProfileManager.GetGroup(groupName));

        ProfileManager.ProfilesChanged = profilesChangedCurrentState;
    }

    private async Task Connect()
    {
        var customDialog = new CustomDialog
        {
            Title = Strings.Connect
        };

        var connectViewModel = new AWSSessionManagerConnectViewModel(async instance =>
        {
            await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            ConfigurationManager.OnDialogClose();

            // Create profile info
            var sessionInfo = new AWSSessionManagerSessionInfo
            {
                InstanceID = instance.InstanceID,
                Profile = instance.Profile,
                Region = instance.Region
            };

            // Add to history
            // Note: The history can only be updated after the values have been read.
            //       Otherwise, in some cases, incorrect values are taken over.
            AddInstanceIDToHistory(instance.InstanceID);
            AddProfileToHistory(instance.Profile);
            AddRegionToHistory(instance.Region);

            // Connect
            Connect(sessionInfo);
        }, async _ =>
        {
            await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            ConfigurationManager.OnDialogClose();
        });

        customDialog.Content = new AWSSessionManagerConnectDialog
        {
            DataContext = connectViewModel
        };

        ConfigurationManager.OnDialogOpen();
        await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
    }

    private void ConnectProfile()
    {
        var sessionInfo = AWSSessionManager.CreateSessionInfo(SelectedProfile);

        Connect(sessionInfo, SelectedProfile.Name);
    }

    private void ConnectProfileExternal()
    {
        var sessionInfo = AWSSessionManager.CreateSessionInfo(SelectedProfile);

        Process.Start(new ProcessStartInfo
        {
            FileName = SettingsManager.Current.AWSSessionManager_ApplicationFilePath,
            Arguments = Models.AWS.AWSSessionManager.BuildCommandLine(sessionInfo)
        });
    }

    private void Connect(AWSSessionManagerSessionInfo sessionInfo, string header = null)
    {
        sessionInfo.ApplicationFilePath = SettingsManager.Current.AWSSessionManager_ApplicationFilePath;

        var tabId = Guid.NewGuid();

        TabItems.Add(new DragablzTabItem(header ?? sessionInfo.InstanceID,
            new AWSSessionManagerControl(tabId, sessionInfo), tabId));

        // Select the added tab
        _disableFocusEmbeddedWindow = true;
        SelectedTabIndex = TabItems.Count - 1;
        _disableFocusEmbeddedWindow = false;
    }

    // Modify history list
    private static void AddInstanceIDToHistory(string instanceID)
    {
        if (string.IsNullOrEmpty(instanceID))
            return;

        SettingsManager.Current.AWSSessionManager_InstanceIDHistory = new ObservableCollection<string>(
            ListHelper.Modify(SettingsManager.Current.AWSSessionManager_InstanceIDHistory.ToList(), instanceID,
                SettingsManager.Current.General_HistoryListEntries));
    }

    private static void AddProfileToHistory(string profile)
    {
        if (string.IsNullOrEmpty(profile))
            return;

        SettingsManager.Current.AWSSessionManager_ProfileHistory = new ObservableCollection<string>(
            ListHelper.Modify(SettingsManager.Current.AWSSessionManager_ProfileHistory.ToList(), profile,
                SettingsManager.Current.General_HistoryListEntries));
    }

    private static void AddRegionToHistory(string region)
    {
        if (string.IsNullOrEmpty(region))
            return;

        SettingsManager.Current.AWSSessionManager_RegionHistory = new ObservableCollection<string>(
            ListHelper.Modify(SettingsManager.Current.AWSSessionManager_RegionHistory.ToList(), region,
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

    public void OnViewVisible(bool fromSettings)
    {
        _isViewActive = true;

        RefreshProfiles();

        // Do not synchronize If the view becomes visible again
        // after the settings have been opened
        if (!fromSettings)
            SyncAllInstanceIDsFromAWS().ConfigureAwait(false);
    }

    public void OnViewHide()
    {
        _isViewActive = false;
    }

    public void OnProfileLoaded()
    {
        SyncAllInstanceIDsFromAWS().ConfigureAwait(false);
    }

    private void SetProfilesView(ProfileInfo profile = null)
    {
        Profiles = new CollectionViewSource
        {
            Source = ProfileManager.Groups.SelectMany(x => x.Profiles).Where(x => x.AWSSessionManager_Enabled)
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

            // Search by: Name, AWSSessionManager_InstanceID
            return info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   info.AWSSessionManager_InstanceID.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1;
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

    private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SettingsInfo.AWSSessionManager_EnableSyncInstanceIDsFromAWS):
            {
                IsSyncEnabled = SettingsManager.Current.AWSSessionManager_EnableSyncInstanceIDsFromAWS;

                if (IsSyncEnabled)
                    SyncAllInstanceIDsFromAWS().ConfigureAwait(false);
                else
                    RemoveDynamicGroups();
                break;
            }
            case nameof(SettingsInfo.AWSSessionManager_SyncOnlyRunningInstancesFromAWS):
                SyncAllInstanceIDsFromAWS().ConfigureAwait(false);
                break;
            case nameof(SettingsInfo.AWSSessionManager_ApplicationFilePath):
                CheckSettings();
                break;
        }
    }

    private void AWSSessionManager_AWSProfiles_CollectionChanged(object sender,
        NotifyCollectionChangedEventArgs e)
    {
        // Remove groups
        if (e.OldItems != null)
            foreach (AWSProfileInfo profile in e.OldItems)
                RemoveDynamicGroup(profile.Profile, profile.Region);

        // Sync new groups
        if (e.NewItems == null)
            return;

        foreach (AWSProfileInfo profile in e.NewItems)
            if (profile.IsEnabled)
                SyncInstanceIDsFromAWS(profile.Profile, profile.Region).ConfigureAwait(false);
    }

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

    #endregion
}