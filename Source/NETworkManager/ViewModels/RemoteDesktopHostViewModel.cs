using System.Collections.ObjectModel;
using NETworkManager.Controls;
using Dragablz;
using MahApps.Metro.Controls.Dialogs;
using System.Windows.Input;
using NETworkManager.Views;
using NETworkManager.Settings;
using System.ComponentModel;
using System.Windows.Data;
using System;
using System.Linq;
using System.Diagnostics;
using NETworkManager.Utilities;
using System.Windows;
using NETworkManager.Models.RemoteDesktop;
using NETworkManager.Profiles;
using System.Windows.Threading;
using NETworkManager.Settings;
using NETworkManager.Models;

namespace NETworkManager.ViewModels
{
    public class RemoteDesktopHostViewModel : ViewModelBase, IProfileManager
    {
        #region Variables
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly DispatcherTimer _searchDispatcherTimer = new DispatcherTimer();

        public IInterTabClient InterTabClient { get; }
        public ObservableCollection<DragablzTabItem> TabItems { get; }

        private readonly bool _isLoading;

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

                if (!_isLoading && Math.Abs(value.Value - GlobalStaticConfiguration.Profile_WidthCollapsed) > GlobalStaticConfiguration.FloatPointFix) // Do not save the size when collapsed
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

            TabItems = new ObservableCollection<DragablzTabItem>();

            Profiles = new CollectionViewSource { Source = ProfileManager.Profiles }.View;
            Profiles.GroupDescriptions.Add(new PropertyGroupDescription(nameof(ProfileInfo.Group)));
            Profiles.SortDescriptions.Add(new SortDescription(nameof(ProfileInfo.Group), ListSortDirection.Ascending));
            Profiles.SortDescriptions.Add(new SortDescription(nameof(ProfileInfo.Name), ListSortDirection.Ascending));
            Profiles.Filter = o =>
            {
                if (!(o is ProfileInfo info))
                    return false;

                if (string.IsNullOrEmpty(Search))
                    return info.RemoteDesktop_Enabled;

                var search = Search.Trim();

                // Search by: Tag=xxx (exact match, ignore case)
                if (search.StartsWith(ProfileManager.TagIdentifier, StringComparison.OrdinalIgnoreCase))
                    return !string.IsNullOrEmpty(info.Tags) && info.RemoteDesktop_Enabled && info.Tags.Replace(" ", "").Split(';').Any(str => search.Substring(ProfileManager.TagIdentifier.Length, search.Length - ProfileManager.TagIdentifier.Length).Equals(str, StringComparison.OrdinalIgnoreCase));

                // Search by: Name, RemoteDesktop_Host
                return info.RemoteDesktop_Enabled && (info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1 || info.RemoteDesktop_Host.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1);
            };

            // This will select the first entry as selected item...
            SelectedProfile = Profiles.SourceCollection.Cast<ProfileInfo>().Where(x => x.RemoteDesktop_Enabled).OrderBy(x => x.Group).ThenBy(x => x.Name).FirstOrDefault();

            _searchDispatcherTimer.Interval = GlobalStaticConfiguration.SearchDispatcherTimerTimeSpan;
            _searchDispatcherTimer.Tick += SearchDispatcherTimer_Tick;

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            ExpandProfileView = SettingsManager.Current.RemoteDesktop_ExpandProfileView;

            ProfileWidth = ExpandProfileView ? new GridLength(SettingsManager.Current.RemoteDesktop_ProfileWidth) : new GridLength(GlobalStaticConfiguration.Profile_WidthCollapsed);

            _tempProfileWidth = SettingsManager.Current.RemoteDesktop_ProfileWidth;
        }
        #endregion

        #region ICommand & Actions        
        public ICommand ConnectCommand => new RelayCommand(p => ConnectAction());

        private void ConnectAction()
        {
            Connect();
        }

        private bool RemoteDesktop_Disconnected_CanExecute(object view)
        {
            if (view is RemoteDesktopControl control)
                return !control.IsConnected;

            return false;
        }

        private bool RemoteDesktop_Connected_CanExecute(object view)
        {
            if (view is RemoteDesktopControl control)
                return control.IsConnected;

            return false;
        }

        public ICommand RemoteDesktop_ReconnectCommand => new RelayCommand(RemoteDesktop_ReconnectAction, RemoteDesktop_Disconnected_CanExecute);

        private void RemoteDesktop_ReconnectAction(object view)
        {
            if (view is RemoteDesktopControl control)
            {
                if (control.ReconnectCommand.CanExecute(null))
                    control.ReconnectCommand.Execute(null);
            }
        }

        public ICommand RemoteDesktop_DisconnectCommand => new RelayCommand(RemoteDesktop_DisconnectAction, RemoteDesktop_Connected_CanExecute);

        private void RemoteDesktop_DisconnectAction(object view)
        {
            if (view is RemoteDesktopControl control)
            {
                if (control.DisconnectCommand.CanExecute(null))
                    control.DisconnectCommand.Execute(null);
            }
        }

        public ICommand RemoteDesktop_FullscreenCommand => new RelayCommand(RemoteDesktop_FullscreenAction, RemoteDesktop_Connected_CanExecute);

        private void RemoteDesktop_FullscreenAction(object view)
        {
            if (view is RemoteDesktopControl control)
                control.FullScreen();
        }

        public ICommand RemoteDesktop_AdjustScreenCommand => new RelayCommand(RemoteDesktop_AdjustScreenAction, RemoteDesktop_Connected_CanExecute);

        private void RemoteDesktop_AdjustScreenAction(object view)
        {
            if (view is RemoteDesktopControl control)
                control.AdjustScreen();
        }

        public ICommand RemoteDesktop_SendCtrlAltDelCommand => new RelayCommand(RemoteDesktop_SendCtrlAltDelAction, RemoteDesktop_Connected_CanExecute);

        private async void RemoteDesktop_SendCtrlAltDelAction(object view)
        {
            if (view is RemoteDesktopControl control)
            {
                try
                {
                    control.SendKey(Keystroke.CtrlAltDel);
                }
                catch (Exception ex)
                {
                    ConfigurationManager.Current.FixAirspace = true;

                    await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.Error, string.Format("{0}\n\nMessage:\n{1}", Localization.Resources.Strings.CouldNotSendKeystroke, ex.Message, MessageDialogStyle.Affirmative, AppearanceManager.MetroDialog));

                    ConfigurationManager.Current.FixAirspace = false;
                }
            }
        }

        public ICommand ConnectProfileCommand => new RelayCommand(p => ConnectProfileAction(), ConnectProfile_CanExecute);

        private bool ConnectProfile_CanExecute(object obj)
        {
            return SelectedProfile != null;
        }

        private void ConnectProfileAction()
        {
            ConnectProfile();
        }

        public ICommand ConnectProfileAsCommand => new RelayCommand(p => ConnectProfileAsAction());

        private void ConnectProfileAsAction()
        {
            ConnectProfileAs();
        }

        public ICommand ConnectProfileExternalCommand => new RelayCommand(p => ConnectProfileExternalAction());

        private void ConnectProfileExternalAction()
        {
            Process.Start("mstsc.exe", $"/V:{SelectedProfile.RemoteDesktop_Host}");
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

        public ItemActionCallback CloseItemCommand => CloseItemAction;

        private static void CloseItemAction(ItemActionCallbackArgs<TabablzControl> args)
        {
            ((args.DragablzItem.Content as DragablzTabItem)?.View as RemoteDesktopControl)?.CloseTab();
        }
        #endregion

        #region Methods
        // Connect via Dialog
        private async void Connect(string host = null)
        {
            var customDialog = new CustomDialog
            {
                Title = Localization.Resources.Strings.Connect
            };

            var remoteDesktopConnectViewModel = new RemoteDesktopConnectViewModel(async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;

                // Add host to history
                AddHostToHistory(instance.Host);

                // Create new session info with default settings
                var sessionInfo = Models.RemoteDesktopTMP.RemoteDesktop.CreateSessionInfo();

                sessionInfo.Hostname = instance.Host;

                if (instance.UseCredentials)
                {
                    sessionInfo.CustomCredentials = true;

                    sessionInfo.Username = instance.Username;
                    sessionInfo.Password = instance.Password;
                }

                Connect(sessionInfo);
            }, async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;
            })
            {
                Host = host
            };

            customDialog.Content = new RemoteDesktopConnectDialog
            {
                DataContext = remoteDesktopConnectViewModel
            };

            ConfigurationManager.Current.FixAirspace = true;
            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        // Connect via Profile
        private void ConnectProfile()
        {
            var profileInfo = SelectedProfile;

            var sessionInfo = Models.RemoteDesktopTMP.RemoteDesktop.CreateSessionInfo(profileInfo);

            Connect(sessionInfo, profileInfo.Name);
        }

        // Connect via Profile with Credentials
        private async void ConnectProfileAs()
        {
            var profileInfo = SelectedProfile;

            var sessionInfo = Models.RemoteDesktopTMP.RemoteDesktop.CreateSessionInfo(profileInfo);

            var customDialog = new CustomDialog
            {
                Title = Localization.Resources.Strings.ConnectAs
            };

            var remoteDesktopConnectViewModel = new RemoteDesktopConnectViewModel(async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;

                if (instance.UseCredentials)
                {
                    sessionInfo.CustomCredentials = true;
                    sessionInfo.Username = instance.Username;
                    sessionInfo.Password = instance.Password;
                }

                Connect(sessionInfo, instance.Name);
            }, async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;
            }, true)
            {
                // Set name, hostname
                Name = profileInfo.Name,
                Host = profileInfo.RemoteDesktop_Host,

                // Request credentials
                UseCredentials = true
            };

            customDialog.Content = new RemoteDesktopConnectDialog
            {
                DataContext = remoteDesktopConnectViewModel
            };

            ConfigurationManager.Current.FixAirspace = true;
            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        private void Connect(RemoteDesktopSessionInfo sessionInfo, string header = null)
        {
            TabItems.Add(new DragablzTabItem(header ?? sessionInfo.Hostname, new RemoteDesktopControl(sessionInfo)));
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
            var list = ListHelper.Modify(SettingsManager.Current.RemoteDesktop_HostHistory.ToList(), host, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.RemoteDesktop_HostHistory.Clear();

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.RemoteDesktop_HostHistory.Add(x));
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
            RefreshProfiles();
        }

        public void OnViewHide()
        {

        }

        public void RefreshProfiles()
        {
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
        private void SearchDispatcherTimer_Tick(object sender, EventArgs e)
        {
            StopDelayedSearch();
        }
        #endregion
    }
}