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
using NETworkManager.Models.PowerShell;
using NETworkManager.Profiles;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
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
using AWSSessionManagerProfile = NETworkManager.Profiles.Application.AWSSessionManager;

namespace NETworkManager.ViewModels;

public class AWSSessionManagerHostViewModel : ViewModelBase, IProfileManager
{
    #region Variables

    private static readonly ILog Log = LogManager.GetLogger(typeof(AWSSessionManagerHostViewModel));

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

        // Check if AWS tools are installed
        CheckRequirements();

        // Check if PowerShell executable is configured
        CheckExecutable();

        // Try to find PowerShell executable
        if (!IsExecutableConfigured)
            TryFindExecutable();

        WriteDefaultProfileToRegistry();

        InterTabClient = new DragablzInterTabClient(ApplicationName.AWSSessionManager);
        InterTabPartition = nameof(ApplicationName.AWSSessionManager);

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

    public ICommand CheckRequirementsCommand => new RelayCommand(_ => CheckRequirementsAction());

    private void CheckRequirementsAction()
    {
        CheckRequirements();
    }

    public ItemActionCallback CloseItemCommand => CloseItemAction;

    private void CloseItemAction(ItemActionCallbackArgs<TabablzControl> args)
    {
        ((args.DragablzItem.Content as DragablzTabItem)?.View as AWSSessionManagerControl)?.CloseTab();
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
            .ShowAddProfileDialog(Application.Current.MainWindow, this, null, null, ApplicationName.AWSSessionManager)
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

    private void CheckRequirements()
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

    /// <summary>
    /// Check if the executable is configured and exists.
    /// </summary>
    private void CheckExecutable()
    {
        IsExecutableConfigured = !string.IsNullOrEmpty(SettingsManager.Current.AWSSessionManager_ApplicationFilePath) &&
                                 File.Exists(SettingsManager.Current.AWSSessionManager_ApplicationFilePath);

        if (IsExecutableConfigured)
            Log.Info(
                $"PowerShell executable found: \"{SettingsManager.Current.AWSSessionManager_ApplicationFilePath}\"");
        else
            Log.Warn("PowerShell executable not found!");
    }

    /// <summary>
    /// Try to find executable.
    /// </summary>
    private void TryFindExecutable()
    {
        Log.Info("Try to find PowerShell executable...");

        var applicationFilePath = ApplicationHelper.Find(PowerShell.PwshFileName);

        if (string.IsNullOrEmpty(applicationFilePath))
            applicationFilePath = ApplicationHelper.Find(PowerShell.WindowsPowerShellFileName);

        SettingsManager.Current.AWSSessionManager_ApplicationFilePath = applicationFilePath;

        CheckExecutable();

        if (!IsExecutableConfigured)
            Log.Warn("Install PowerShell or configure the path in the settings.");
    }

    private bool IsConfigured => IsAWSCLIInstalled && IsAWSSessionManagerPluginInstalled && IsExecutableConfigured;

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
                $"Preconditions not met! AWS CLI installed {IsAWSCLIInstalled}. AWS Session Manager plugin installed {IsAWSSessionManagerPluginInstalled}. PowerShell configured {IsExecutableConfigured}.");
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
        await Task.Delay(GlobalStaticConfiguration.ApplicationUIRefreshInterval);

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
        await Task.Delay(GlobalStaticConfiguration.ApplicationUIRefreshInterval);

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
                ProfileManager.RemoveGroup(ProfileManager.GetGroupByName(groupName));

            Log.Info("No EC2 Instance(s) found!");
        }
        else
        {
            if (ProfileManager.GroupExists(groupName))
                ProfileManager.ReplaceGroup(ProfileManager.GetGroupByName(groupName), groupInfo);
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
            ProfileManager.RemoveGroup(ProfileManager.GetGroupByName(groupName));

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
        var sessionInfo = AWSSessionManagerProfile.CreateSessionInfo(SelectedProfile);

        Connect(sessionInfo, SelectedProfile.Name);
    }

    private void ConnectProfileExternal()
    {
        var sessionInfo = AWSSessionManagerProfile.CreateSessionInfo(SelectedProfile);

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
        SelectedTabIndex = TabItems.Count - 1;
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

    private void CreateTags()
    {
        var tags = ProfileManager.Groups.SelectMany(x => x.Profiles).Where(x => x.AWSSessionManager_Enabled)
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
            Source = ProfileManager.Groups.SelectMany(x => x.Profiles).Where(x => x.AWSSessionManager_Enabled && (
                    string.IsNullOrEmpty(filter.Search) ||
                    x.Name.IndexOf(filter.Search, StringComparison.Ordinal) > -1 ||
                    x.AWSSessionManager_InstanceID.IndexOf(filter.Search, StringComparison.Ordinal) > -1) && (
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

    private void WriteDefaultProfileToRegistry()
    {
        if (!SettingsManager.Current.Appearance_PowerShellModifyGlobalProfile)
            return;

        if (!IsExecutableConfigured)
            return;

        Log.Debug("Write PowerShell profile to registry...");

        PowerShell.WriteDefaultProfileToRegistry(
            SettingsManager.Current.Appearance_Theme,
            SettingsManager.Current.AWSSessionManager_ApplicationFilePath);
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
                CheckExecutable();
                WriteDefaultProfileToRegistry();
                break;
            case nameof(SettingsInfo.Appearance_PowerShellModifyGlobalProfile):
            case nameof(SettingsInfo.Appearance_Theme):
                WriteDefaultProfileToRegistry();
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