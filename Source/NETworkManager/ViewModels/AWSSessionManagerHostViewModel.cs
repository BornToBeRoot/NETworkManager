﻿using System.Collections.ObjectModel;
using NETworkManager.Controls;
using Dragablz;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Settings;
using System.Linq;
using NETworkManager.Views;
using System.ComponentModel;
using System.Windows.Data;
using System;
using System.Diagnostics;
using System.IO;
using NETworkManager.Utilities;
using System.Windows;
using NETworkManager.Profiles;
using System.Windows.Threading;
using NETworkManager.Models;
using NETworkManager.Models.EventSystem;
using System.Threading.Tasks;
using System.Collections.Generic;
using Amazon;
using Amazon.EC2;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using NETworkManager.Models.AWS;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace NETworkManager.ViewModels
{
    public class AWSSessionManagerHostViewModel : ViewModelBase, IProfileManager
    {
        #region Variables
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly DispatcherTimer _searchDispatcherTimer = new DispatcherTimer();

        public IInterTabClient InterTabClient { get; }
        public ObservableCollection<DragablzTabItem> TabItems { get; }

        private readonly bool _isLoading = true;
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
        #region Profiles

        public ICollectionView Profiles { get; }

        private ProfileInfo _selectedProfile = new ProfileInfo();
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

                StartDelayedSearch();

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

                if (!_isLoading && Math.Abs(value.Value - GlobalStaticConfiguration.Profile_WidthCollapsed) > GlobalStaticConfiguration.FloatPointFix) // Do not save the size when collapsed
                    SettingsManager.Current.AWSSessionManager_ProfileWidth = value.Value;

                _profileWidth = value;

                if (_canProfileWidthChange)
                    ResizeProfile(true);

                OnPropertyChanged();
            }
        }
        #endregion
        #endregion

        #region Constructor, load settings
        public AWSSessionManagerHostViewModel(IDialogCoordinator instance)
        {
            _dialogCoordinator = instance;
                        
            CheckInstallationStatus();
            CheckSettings();

            InterTabClient = new DragablzInterTabClient(ApplicationName.AWSSessionManager);

            TabItems = new ObservableCollection<DragablzTabItem>();

            Profiles = new CollectionViewSource { Source = ProfileManager.Groups.SelectMany(x => x.Profiles) }.View;
            Profiles.GroupDescriptions.Add(new PropertyGroupDescription(nameof(ProfileInfo.Group)));
            Profiles.SortDescriptions.Add(new SortDescription(nameof(ProfileInfo.Group), ListSortDirection.Ascending));
            Profiles.SortDescriptions.Add(new SortDescription(nameof(ProfileInfo.Name), ListSortDirection.Ascending));
            Profiles.Filter = o =>
            {
                if (o is not ProfileInfo info)
                    return false;

                if (string.IsNullOrEmpty(Search))
                    return info.AWSSessionManager_Enabled;

                var search = Search.Trim();

                // Search by: Tag=xxx (exact match, ignore case)

                //if (search.StartsWith(ProfileManager.TagIdentifier, StringComparison.OrdinalIgnoreCase))
                //    return !string.IsNullOrEmpty(info.Tags) && info.AWSSessionManager_Enabled && info.Tags.Replace(" ", "").Split(';').Any(str => search.Substring(ProfileManager.TagIdentifier.Length, search.Length - ProfileManager.TagIdentifier.Length).Equals(str, StringComparison.OrdinalIgnoreCase));
                //

                // Search by: Name, AWSSessionManager_Host
                return info.AWSSessionManager_Enabled && (info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1 || info.AWSSessionManager_InstanceID.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1);
            };

            //Test();

            // This will select the first entry as selected item...
            SelectedProfile = Profiles.SourceCollection.Cast<ProfileInfo>().Where(x => x.AWSSessionManager_Enabled).OrderBy(x => x.Group).ThenBy(x => x.Name).FirstOrDefault();

            ProfileManager.OnProfilesUpdated += ProfileManager_OnProfilesUpdated;

            _searchDispatcherTimer.Interval = GlobalStaticConfiguration.SearchDispatcherTimerTimeSpan;
            _searchDispatcherTimer.Tick += SearchDispatcherTimer_Tick;

            LoadSettings();

            SyncAllInstanceIDsFromAWS();

            SettingsManager.Current.PropertyChanged += Current_PropertyChanged;
            SettingsManager.Current.AWSSessionManager_AWSProfiles.CollectionChanged += AWSSessionManager_AWSProfiles_CollectionChanged;

            _isLoading = false;
        }

        private async Task SyncAllInstanceIDsFromAWS()
        {
            if (!SettingsManager.Current.AWSSessionManager_EnableSyncInstanceIDsFromAWS)
            {
                Debug.WriteLine("Sync with AWS is disabled!");
                return;
            }

            IsSyncing = true;

            foreach (var profile in SettingsManager.Current.AWSSessionManager_AWSProfiles)
            {
                if (!profile.IsEnabled)
                {
                    Debug.WriteLine($"Sync for profile {profile.Profile}\\{profile.Region} is disabled!");
                    continue;
                }

                await SyncInstanceIDsFromAWS(profile.Profile, profile.Region);
            }

            //await Task.Delay(2000);

            IsSyncing = false;
        }

        private async Task SyncGroupInstanceIDsFromAWS(string group)
        {
            IsSyncing = true;

            Debug.WriteLine("Sync group: " + group);

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
                Debug.WriteLine("Could not get profile and region from from group...");
            }

            //await Task.Delay(2000);

            IsSyncing = false;
        }

        private async Task SyncInstanceIDsFromAWS(string profile, string region)
        {
            CredentialProfileStoreChain credentialProfileStoreChain = new();

            credentialProfileStoreChain.TryGetAWSCredentials(profile, out AWSCredentials credentials);

            Debug.WriteLine($"Sync profile {profile}\\{region}...");
            Debug.WriteLine("Using credentials: " + credentials.GetCredentials().AccessKey);

            using AmazonEC2Client client = new(credentials, RegionEndpoint.GetBySystemName(region));

            var response = await client.DescribeInstancesAsync();

            var groupName = $"~ [{profile}\\{region}]";

            // Create a new group info for profiles
            var groupInfo = new GroupInfo()
            {
                Name = groupName,
                IsDynamic = true,
            };

            foreach (var reservation in response.Reservations)
            {
                foreach (var instance in reservation.Instances)
                {
                    if (SettingsManager.Current.AWSSessionManager_SyncOnlyRunningInstancesFromAWS && instance.State.Name.Value != "running")
                        continue;

                    Debug.WriteLine("Found Instance: " + instance.InstanceId);

                    var tagName = instance.Tags.FirstOrDefault(x => x.Key == "Name");
                    var name = tagName.Value == null ? instance.InstanceId : $"{tagName.Value} ({instance.InstanceId})";

                    groupInfo.Profiles.Add(new ProfileInfo()
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
            }

            // Replace or add group
            if (ProfileManager.GroupExists(groupName))
                ProfileManager.ReplaceGroup(ProfileManager.GetGroup(groupName), groupInfo);
            else
                ProfileManager.AddGroup(groupInfo);

            Debug.WriteLine("Sync done!");
        }

        private void Current_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // ToDo.... disable -> delete
            if (e.PropertyName == nameof(SettingsManager.Current.AWSSessionManager_EnableSyncInstanceIDsFromAWS))
            {
                if (SettingsManager.Current.AWSSessionManager_EnableSyncInstanceIDsFromAWS)
                    SyncAllInstanceIDsFromAWS();
            }

            //if (e.PropertyName == nameof(SettingsManager.Current.AWSSessionManager_AWSProfiles))
            //    SyncAllInstanceIDsFromAWS();
            // Does not fire event OnPropertyChange if collection changes...

            if (e.PropertyName == nameof(SettingsInfo.AWSSessionManager_ApplicationFilePath))
                CheckSettings();
        }

        // ToDo ->  Detect diff / only sync diff
        private void AWSSessionManager_AWSProfiles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

        }

        private void LoadSettings()
        {
            ExpandProfileView = SettingsManager.Current.AWSSessionManager_ExpandProfileView;

            ProfileWidth = ExpandProfileView ? new GridLength(SettingsManager.Current.AWSSessionManager_ProfileWidth) : new GridLength(GlobalStaticConfiguration.Profile_WidthCollapsed);

            _tempProfileWidth = SettingsManager.Current.AWSSessionManager_ProfileWidth;
        }
        #endregion

        #region ICommand & Actions
        public ICommand CheckInstallationStatusCommand => new RelayCommand(p => CheckInstallationStatusAction());

        private void CheckInstallationStatusAction()
        {
            CheckInstallationStatus();
        }

        public ItemActionCallback CloseItemCommand => CloseItemAction;

        private void CloseItemAction(ItemActionCallbackArgs<TabablzControl> args)
        {
            ((args.DragablzItem.Content as DragablzTabItem)?.View as AWSSessionManagerControl)?.CloseTab();
        }

        private bool AWSSessionManager_Connected_CanExecute(object view)
        {
            if (view is AWSSessionManagerControl control)
                return control.IsConnected;

            return false;
        }

        public ICommand AWSSessionManager_ReconnectCommand => new RelayCommand(AWSSessionManager_ReconnectAction);

        private void AWSSessionManager_ReconnectAction(object view)
        {
            if (view is AWSSessionManagerControl control)
            {
                if (control.ReconnectCommand.CanExecute(null))
                    control.ReconnectCommand.Execute(null);
            }
        }

        public ICommand AWSSessionManager_ResizeWindowCommand => new RelayCommand(AWSSessionManager_ResizeWindowAction, AWSSessionManager_Connected_CanExecute);

        private void AWSSessionManager_ResizeWindowAction(object view)
        {
            if (view is AWSSessionManagerControl control)
                control.ResizeEmbeddedWindow();
        }

        public ICommand ConnectCommand => new RelayCommand(p => ConnectAction(), Connect_CanExecute);

        private bool Connect_CanExecute(object obj) => IsPowerShellConfigured;

        private void ConnectAction()
        {
            Connect();
        }

        public ICommand ConnectProfileCommand => new RelayCommand(p => ConnectProfileAction(), ConnectProfile_CanExecute);

        private bool ConnectProfile_CanExecute(object obj)
        {
            return !IsSearching && SelectedProfile != null;
        }

        private void ConnectProfileAction()
        {
            ConnectProfile();
        }

        public ICommand ConnectProfileExternalCommand => new RelayCommand(p => ConnectProfileExternalAction());

        private void ConnectProfileExternalAction()
        {
            ConnectProfileExternal();
        }

        public ICommand AddProfileCommand => new RelayCommand(p => AddProfileAction());

        private void AddProfileAction()
        {
            ProfileDialogManager.ShowAddProfileDialog(this, _dialogCoordinator, null, ApplicationName.AWSSessionManager);
        }

        private bool ModifyProfile_CanExecute(object obj) => SelectedProfile != null && !SelectedProfile.IsDynamic;

        public ICommand EditProfileCommand => new RelayCommand(p => EditProfileAction(), ModifyProfile_CanExecute);

        private void EditProfileAction()
        {
            ProfileDialogManager.ShowEditProfileDialog(this, _dialogCoordinator, SelectedProfile);
        }

        public ICommand CopyAsProfileCommand => new RelayCommand(p => CopyAsProfileAction(), ModifyProfile_CanExecute);

        private void CopyAsProfileAction()
        {
            ProfileDialogManager.ShowCopyAsProfileDialog(this, _dialogCoordinator, SelectedProfile);
        }

        public ICommand DeleteProfileCommand => new RelayCommand(p => DeleteProfileAction(), ModifyProfile_CanExecute);

        private void DeleteProfileAction()
        {
            ProfileDialogManager.ShowDeleteProfileDialog(this, _dialogCoordinator, new List<ProfileInfo> { SelectedProfile });
        }

        public ICommand EditGroupCommand => new RelayCommand(EditGroupAction);

        private void EditGroupAction(object group)
        {
            ProfileDialogManager.ShowEditGroupDialog(this, _dialogCoordinator, ProfileManager.GetGroup(group.ToString()));
        }

        private bool SyncInstanceIDsFromAWS_CanExecute(object obj) => !IsSyncing;

        public ICommand SyncAllInstanceIDsFromAWSCommand => new RelayCommand(p => SyncAllInstanceIDsFromAWSAction(), SyncInstanceIDsFromAWS_CanExecute);

        private void SyncAllInstanceIDsFromAWSAction()
        {
            SyncAllInstanceIDsFromAWS();
        }

        public ICommand SyncGroupInstanceIDsFromAWSCommand => new RelayCommand(SyncGroupInstanceIDsFromAWSAction, SyncInstanceIDsFromAWS_CanExecute);

        private void SyncGroupInstanceIDsFromAWSAction(object group)
        {
            SyncGroupInstanceIDsFromAWS((string)group);
        }

        public ICommand ClearSearchCommand => new RelayCommand(p => ClearSearchAction());

        private void ClearSearchAction()
        {
            Search = string.Empty;
        }

        public ICommand OpenSettingsCommand => new RelayCommand(p => OpenSettingsAction());

        private static void OpenSettingsAction()
        {
            EventSystem.RedirectToSettings();
        }
        #endregion

        #region Methods        
        private void CheckInstallationStatus()
        {
            // Maybe also check DisplayVersion?
            RegistryKey awsCLIRegistryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{01CCB9EC-7FA4-494D-9EEC-395C080CAA7C}", false);
            IsAWSCLIInstalled = awsCLIRegistryKey != null && awsCLIRegistryKey.GetValue("DisplayName") != null;

            RegistryKey awsSessionManagerPluginRegistryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{13DD0F7B-C2A8-4F22-BF55-CD0D719DBA46}", false);
            IsAWSSessionManagerPluginInstalled = awsSessionManagerPluginRegistryKey != null && awsSessionManagerPluginRegistryKey.GetValue("DisplayName") != null;
        }
        
        private void CheckSettings()
        {
            IsPowerShellConfigured = !string.IsNullOrEmpty(SettingsManager.Current.AWSSessionManager_ApplicationFilePath) && File.Exists(SettingsManager.Current.AWSSessionManager_ApplicationFilePath);
        }

        private async Task Connect()
        {
            var customDialog = new CustomDialog
            {
                Title = Localization.Resources.Strings.Connect
            };

            var connectViewModel = new AWSSessionManagerConnectViewModel(async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;

                // Create profile info
                var info = new AWSSessionManagerSessionInfo
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
                Connect(info);
            }, async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;
            });

            customDialog.Content = new AWSSessionManagerConnectDialog
            {
                DataContext = connectViewModel
            };

            ConfigurationManager.Current.FixAirspace = true;
            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        private void ConnectProfile()
        {
            Connect(NETworkManager.Profiles.Application.AWSSessionManager.CreateSessionInfo(SelectedProfile), SelectedProfile.Name);
        }

        private void ConnectProfileExternal()
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = SettingsManager.Current.AWSSessionManager_ApplicationFilePath,
                Arguments = AWSSessionManager.BuildCommandLine(NETworkManager.Profiles.Application.AWSSessionManager.CreateSessionInfo(SelectedProfile))
            });
        }

        private void Connect(AWSSessionManagerSessionInfo sessionInfo, string header = null)
        {
            sessionInfo.ApplicationFilePath = SettingsManager.Current.AWSSessionManager_ApplicationFilePath;

            TabItems.Add(new DragablzTabItem(header ?? sessionInfo.InstanceID, new AWSSessionManagerControl(sessionInfo)));

            SelectedTabIndex = TabItems.Count - 1;
        }

        // Modify history list
        private static void AddInstanceIDToHistory(string instanceID)
        {
            if (string.IsNullOrEmpty(instanceID))
                return;

            SettingsManager.Current.AWSSessionManager_InstanceIDHistory = new ObservableCollection<string>(ListHelper.Modify(SettingsManager.Current.AWSSessionManager_InstanceIDHistory.ToList(), instanceID, SettingsManager.Current.General_HistoryListEntries));
        }

        private static void AddProfileToHistory(string profile)
        {
            if (string.IsNullOrEmpty(profile))
                return;

            SettingsManager.Current.AWSSessionManager_ProfileHistory = new ObservableCollection<string>(ListHelper.Modify(SettingsManager.Current.AWSSessionManager_ProfileHistory.ToList(), profile, SettingsManager.Current.General_HistoryListEntries));
        }

        private static void AddRegionToHistory(string region)
        {
            if (string.IsNullOrEmpty(region))
                return;

            SettingsManager.Current.AWSSessionManager_RegionHistory = new ObservableCollection<string>(ListHelper.Modify(SettingsManager.Current.AWSSessionManager_RegionHistory.ToList(), region, SettingsManager.Current.General_HistoryListEntries));
        }

        private void StartDelayedSearch()
        {
            if (!IsSearching)
            {
                IsSearching = true;

                _searchDispatcherTimer.Start();
            }
            else
            {
                _searchDispatcherTimer.Stop();
                _searchDispatcherTimer.Start();
            }
        }

        private void StopDelayedSearch()
        {
            _searchDispatcherTimer.Stop();

            RefreshProfiles();

            IsSearching = false;
        }

        private void ResizeProfile(bool dueToChangedSize)
        {
            _canProfileWidthChange = false;

            if (dueToChangedSize)
            {
                ExpandProfileView = Math.Abs(ProfileWidth.Value - GlobalStaticConfiguration.Profile_WidthCollapsed) > GlobalStaticConfiguration.FloatPointFix;
            }
            else
            {
                if (ExpandProfileView)
                {
                    ProfileWidth = Math.Abs(_tempProfileWidth - GlobalStaticConfiguration.Profile_WidthCollapsed) < GlobalStaticConfiguration.FloatPointFix ? new GridLength(GlobalStaticConfiguration.Profile_DefaultWidthExpanded) : new GridLength(_tempProfileWidth);
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

        public void RefreshProfiles()
        {
            if (!_isViewActive)
                return;

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
            {
                Profiles.Refresh();
            }));
        }

        public void OnProfileDialogOpen()
        {
            ConfigurationManager.Current.FixAirspace = true;
        }

        public void OnProfileDialogClose()
        {
            ConfigurationManager.Current.FixAirspace = false;
        }
        #endregion

        #region Event
        private void ProfileManager_OnProfilesUpdated(object sender, EventArgs e)
        {
            RefreshProfiles();
        }

        private void SearchDispatcherTimer_Tick(object sender, EventArgs e)
        {
            StopDelayedSearch();
        }
        #endregion
    }
}