using System.Collections.ObjectModel;
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
using System.IO;
using NETworkManager.Utilities;
using System.Diagnostics;
using NETworkManager.Models.PuTTY;
using System.Windows;
using NETworkManager.Profiles;
using System.Windows.Threading;
using NETworkManager.Models;
using NETworkManager.Models.EventSystem;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace NETworkManager.ViewModels
{
    public class PuTTYHostViewModel : ViewModelBase, IProfileManager
    {
        #region Variables
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly DispatcherTimer _searchDispatcherTimer = new DispatcherTimer();

        public IInterTabClient InterTabClient { get; }
        public ObservableCollection<DragablzTabItem> TabItems { get; }

        private readonly bool _isLoading = true;
        private bool _isViewActive = true;

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

                if (!_isLoading && Math.Abs(value.Value - GlobalStaticConfiguration.Profile_WidthCollapsed) > GlobalStaticConfiguration.FloatPointFix) // Do not save the size when collapsed
                    SettingsManager.Current.PuTTY_ProfileWidth = value.Value;

                _profileWidth = value;

                if (_canProfileWidthChange)
                    ResizeProfile(true);

                OnPropertyChanged();
            }
        }
        #endregion
        #endregion

        #region Constructor, load settings
        public PuTTYHostViewModel(IDialogCoordinator instance)
        {
            _dialogCoordinator = instance;

            // Check if putty is available...
            CheckIfConfigured();

            InterTabClient = new DragablzInterTabClient(ApplicationName.PuTTY);

            TabItems = new ObservableCollection<DragablzTabItem>();

            Profiles = new CollectionViewSource { Source = ProfileManager.Groups.SelectMany(x => x.Profiles) }.View;
            Profiles.GroupDescriptions.Add(new PropertyGroupDescription(nameof(ProfileInfo.Group)));
            Profiles.SortDescriptions.Add(new SortDescription(nameof(ProfileInfo.Group), ListSortDirection.Ascending));
            Profiles.SortDescriptions.Add(new SortDescription(nameof(ProfileInfo.Name), ListSortDirection.Ascending));
            Profiles.Filter = o =>
            {
                if (!(o is ProfileInfo info))
                    return false;

                if (string.IsNullOrEmpty(Search))
                    return info.PuTTY_Enabled;

                var search = Search.Trim();

                // Search by: Tag=xxx (exact match, ignore case)
                /*
                if (search.StartsWith(ProfileManager.TagIdentifier, StringComparison.OrdinalIgnoreCase))
                    return !string.IsNullOrEmpty(info.Tags) && info.PuTTY_Enabled && info.Tags.Replace(" ", "").Split(';').Any(str => search.Substring(ProfileManager.TagIdentifier.Length, search.Length - ProfileManager.TagIdentifier.Length).Equals(str, StringComparison.OrdinalIgnoreCase));
                */

                // Search by: Name, PuTTY_HostOrSerialLine
                return info.PuTTY_Enabled && (info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1 || info.PuTTY_HostOrSerialLine.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1);
            };

            // This will select the first entry as selected item...
            SelectedProfile = Profiles.SourceCollection.Cast<ProfileInfo>().Where(x => x.PuTTY_Enabled).OrderBy(x => x.Group).ThenBy(x => x.Name).FirstOrDefault();

            ProfileManager.OnProfilesUpdated += ProfileManager_OnProfilesUpdated;

            _searchDispatcherTimer.Interval = GlobalStaticConfiguration.SearchDispatcherTimerTimeSpan;
            _searchDispatcherTimer.Tick += SearchDispatcherTimer_Tick;

            LoadSettings();

            SettingsManager.Current.PropertyChanged += Current_PropertyChanged;

            _isLoading = false;
        }

        private void Current_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SettingsInfo.PuTTY_ApplicationFilePath))
                CheckIfConfigured();
        }

        private void LoadSettings()
        {
            ExpandProfileView = SettingsManager.Current.PuTTY_ExpandProfileView;

            ProfileWidth = ExpandProfileView ? new GridLength(SettingsManager.Current.PuTTY_ProfileWidth) : new GridLength(GlobalStaticConfiguration.Profile_WidthCollapsed);

            _tempProfileWidth = SettingsManager.Current.PuTTY_ProfileWidth;
        }
        #endregion

        #region ICommand & Actions
        public ItemActionCallback CloseItemCommand => CloseItemAction;

        private void CloseItemAction(ItemActionCallbackArgs<TabablzControl> args)
        {
            ((args.DragablzItem.Content as DragablzTabItem)?.View as PuTTYControl)?.CloseTab();
        }

        private bool PuTTY_Disconnected_CanExecute(object view)
        {
            if (view is PuTTYControl control)
                return !control.IsConnected;

            return false;
        }

        private bool PuTTY_Connected_CanExecute(object view)
        {
            if (view is PuTTYControl control)
                return control.IsConnected;

            return false;
        }

        public ICommand PuTTY_ReconnectCommand => new RelayCommand(PuTTY_ReconnectAction);

        private void PuTTY_ReconnectAction(object view)
        {
            if (view is PuTTYControl control)
            {
                if (control.ReconnectCommand.CanExecute(null))
                    control.ReconnectCommand.Execute(null);
            }
        }

        public ICommand PuTTY_RestartSessionCommand => new RelayCommand(PuTTY_RestartSessionAction, PuTTY_Connected_CanExecute);

        private void PuTTY_RestartSessionAction(object view)
        {
            if (view is PuTTYControl puttyControl)
                puttyControl.RestartSession();
        }

        public ICommand ConnectCommand => new RelayCommand(p => ConnectAction(), Connect_CanExecute);

        private bool Connect_CanExecute(object obj)
        {
            return IsConfigured;
        }

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
            ProfileDialogManager.ShowAddProfileDialog(this, _dialogCoordinator);
        }

        public ICommand EditProfileCommand => new RelayCommand(p => EditProfileAction());

        private void EditProfileAction()
        {
            ProfileDialogManager.ShowEditProfileDialog(this, _dialogCoordinator, SelectedProfile);
        }

        public ICommand CopyAsProfileCommand => new RelayCommand(p => CopyAsProfileAction());

        private void CopyAsProfileAction()
        {
            ProfileDialogManager.ShowCopyAsProfileDialog(this, _dialogCoordinator, SelectedProfile);
        }

        public ICommand DeleteProfileCommand => new RelayCommand(p => DeleteProfileAction());

        private void DeleteProfileAction()
        {
            ProfileDialogManager.ShowDeleteProfileDialog(this, _dialogCoordinator, SelectedProfile);
        }

        public ICommand EditGroupCommand => new RelayCommand(EditGroupAction);

        private void EditGroupAction(object group)
        {
            ProfileDialogManager.ShowEditGroupDialog(this, _dialogCoordinator, group.ToString());
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
        private void CheckIfConfigured()
        {
            IsConfigured = !string.IsNullOrEmpty(SettingsManager.Current.PuTTY_ApplicationFilePath) && File.Exists(SettingsManager.Current.PuTTY_ApplicationFilePath);
        }

        private async Task Connect(string host = null)
        {
            var customDialog = new CustomDialog
            {
                Title = Localization.Resources.Strings.Connect
            };

            var connectViewModel = new PuTTYConnectViewModel(async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;

                // Add host to history
                AddHostToHistory(instance.Host);
                AddSerialLineToHistory(instance.SerialLine);
                AddPortToHistory(instance.Port.ToString());
                AddBaudToHistory(instance.Baud.ToString());
                AddUsernameToHistory(instance.Username);
                AddPrivateKeyToHistory(instance.PrivateKeyFile);
                AddProfileToHistory(instance.Profile);

                // Create Profile info
                PuTTYSessionInfo info = new PuTTYSessionInfo
                {
                    HostOrSerialLine = instance.ConnectionMode == ConnectionMode.Serial ? instance.SerialLine : instance.Host,
                    Mode = instance.ConnectionMode,
                    PortOrBaud = instance.ConnectionMode == ConnectionMode.Serial ? instance.Baud : instance.Port,
                    Username = instance.Username,
                    PrivateKey = instance.PrivateKeyFile,
                    Profile = instance.Profile,
                    EnableLog = SettingsManager.Current.PuTTY_EnableSessionLog,
                    LogMode = SettingsManager.Current.PuTTY_LogMode,
                    LogFileName = SettingsManager.Current.PuTTY_LogFileName,
                    LogPath = Settings.Application.PuTTY.LogPath,
                    AdditionalCommandLine = instance.AdditionalCommandLine
                };

                Connect(info);
            }, async instance =>
                {
                    await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                    ConfigurationManager.Current.FixAirspace = false;
                }, host);

            customDialog.Content = new PuTTYConnectDialog
            {
                DataContext = connectViewModel
            };

            ConfigurationManager.Current.FixAirspace = true;
            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        private void ConnectProfile()
        {
            Connect(NETworkManager.Profiles.Application.PuTTY.CreateSessionInfo(SelectedProfile), SelectedProfile.Name);
        }

        private void ConnectProfileExternal()
        {
            // Create log path
            DirectoryCreator.CreateWithEnvironmentVariables(Settings.Application.PuTTY.LogPath);

            ProcessStartInfo info = new ProcessStartInfo
            {
                FileName = SettingsManager.Current.PuTTY_ApplicationFilePath,
                Arguments = PuTTY.BuildCommandLine(NETworkManager.Profiles.Application.PuTTY.CreateSessionInfo(SelectedProfile))
            };

            Process.Start(info);
        }

        private void Connect(PuTTYSessionInfo profileInfo, string header = null)
        {
            // Must be added here. So that it works with profiles and the connect dialog.
            profileInfo.ApplicationFilePath = SettingsManager.Current.PuTTY_ApplicationFilePath;

            TabItems.Add(new DragablzTabItem(header ?? profileInfo.HostOrSerialLine, new PuTTYControl(profileInfo)));

            SelectedTabIndex = TabItems.Count - 1;
        }

        public void AddTab(string host)
        {
            Connect(host);
        }

        // Modify history list
        private static void AddHostToHistory(string host)
        {
            // Create the new list
            var list = ListHelper.Modify(SettingsManager.Current.PuTTY_HostHistory.ToList(), host, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.PuTTY_HostHistory.Clear();

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.PuTTY_HostHistory.Add(x));
        }

        private static void AddSerialLineToHistory(string serialLine)
        {
            // Create the new list
            List<string> list = ListHelper.Modify(SettingsManager.Current.PuTTY_SerialLineHistory.ToList(), serialLine, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.PuTTY_SerialLineHistory.Clear();

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.PuTTY_SerialLineHistory.Add(x));
        }

        private static void AddPortToHistory(string port)
        {
            // Create the new list
            List<string> list = ListHelper.Modify(SettingsManager.Current.PuTTY_PortHistory.ToList(), port, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.PuTTY_PortHistory.Clear();

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.PuTTY_PortHistory.Add(x));
        }

        private static void AddBaudToHistory(string baud)
        {
            // Create the new list
            List<string> list = ListHelper.Modify(SettingsManager.Current.PuTTY_BaudHistory.ToList(), baud, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.PuTTY_BaudHistory.Clear();

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.PuTTY_BaudHistory.Add(x));
        }

        private static void AddUsernameToHistory(string username)
        {
            // Create the new list
            List<string> list = ListHelper.Modify(SettingsManager.Current.PuTTY_UsernameHistory.ToList(), username, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.PuTTY_UsernameHistory.Clear();

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.PuTTY_UsernameHistory.Add(x));
        }

        private static void AddPrivateKeyToHistory(string host)
        {
            // Create the new list
            List<string> list = ListHelper.Modify(SettingsManager.Current.PuTTY_PrivateKeyFileHistory.ToList(), host, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.PuTTY_PrivateKeyFileHistory.Clear();

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.PuTTY_PrivateKeyFileHistory.Add(x));
        }

        private static void AddProfileToHistory(string host)
        {
            // Create the new list
            List<string> list = ListHelper.Modify(SettingsManager.Current.PuTTY_ProfileHistory.ToList(), host, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.PuTTY_ProfileHistory.Clear();

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.PuTTY_ProfileHistory.Add(x));
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

            Profiles.Refresh();
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