using System.Collections.ObjectModel;
using NETworkManager.Controls;
using Dragablz;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models.Settings;
using System.Linq;
using NETworkManager.Views;
using System.ComponentModel;
using System.Windows.Data;
using System;
using System.Diagnostics;
using System.IO;
using NETworkManager.Utilities;
using System.Windows;
using NETworkManager.Models.TigerVNC;
using NETworkManager.Enum;

namespace NETworkManager.ViewModels
{
    public class TigerVNCHostViewModel : ViewModelBase
    {
        #region Variables
        private readonly IDialogCoordinator _dialogCoordinator;

        public IInterTabClient InterTabClient { get; }
        public ObservableCollection<DragablzTabItem> TabItems { get; }

        private readonly bool _isLoading;

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

                Profiles.Refresh();

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
                    SettingsManager.Current.TigerVNC_ExpandProfileView = value;

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
                    SettingsManager.Current.TigerVNC_ProfileWidth = value.Value;

                _profileWidth = value;

                if (_canProfileWidthChange)
                    ResizeProfile(true);

                OnPropertyChanged();
            }
        }
        #endregion
        #endregion

        #region Constructor, load settings
        public TigerVNCHostViewModel(IDialogCoordinator instance)
        {
            _isLoading = true;

            _dialogCoordinator = instance;

            // Check if putty is available...
            CheckIfConfigured();

            InterTabClient = new DragablzInterTabClient(ApplicationViewManager.Name.TigerVNC);

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
                    return info.TigerVNC_Enabled;

                var search = Search.Trim();

                // Search by: Tag=xxx (exact match, ignore case)
                if (search.StartsWith(ProfileManager.TagIdentifier, StringComparison.OrdinalIgnoreCase))
                    return !string.IsNullOrEmpty(info.Tags) && info.TigerVNC_Enabled && info.Tags.Replace(" ", "").Split(';').Any(str => search.Substring(ProfileManager.TagIdentifier.Length, search.Length - ProfileManager.TagIdentifier.Length).Equals(str, StringComparison.OrdinalIgnoreCase));

                // Search by: Name, TigerVNC_Host
                return info.TigerVNC_Enabled && (info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1 || info.TigerVNC_Host.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1);
            };

            // This will select the first entry as selected item...
            SelectedProfile = Profiles.SourceCollection.Cast<ProfileInfo>().Where(x => x.TigerVNC_Enabled).OrderBy(x => x.Group).ThenBy(x => x.Name).FirstOrDefault();

            LoadSettings();

            SettingsManager.Current.PropertyChanged += Current_PropertyChanged;

            _isLoading = false;
        }

        private void Current_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SettingsInfo.TigerVNC_ApplicationFilePath))
                CheckIfConfigured();
        }

        private void LoadSettings()
        {
            ExpandProfileView = SettingsManager.Current.TigerVNC_ExpandProfileView;

            ProfileWidth = ExpandProfileView ? new GridLength(SettingsManager.Current.TigerVNC_ProfileWidth) : new GridLength(GlobalStaticConfiguration.Profile_WidthCollapsed);

            _tempProfileWidth = SettingsManager.Current.TigerVNC_ProfileWidth;
        }
        #endregion

        #region ICommand & Actions
        public ItemActionCallback CloseItemCommand => CloseItemAction;

        private void CloseItemAction(ItemActionCallbackArgs<TabablzControl> args)
        {
            ((args.DragablzItem.Content as DragablzTabItem)?.View as TigerVNCControl)?.CloseTab();
        }

        public ICommand ConnectCommand => new RelayCommand(p => ConnectAction(), Connect_CanExecute);

        private bool Connect_CanExecute(object obj) => IsConfigured && !ConfigurationManager.Current.IsTransparencyEnabled;

        private void ConnectAction()
        {
            Connect();
        }

        public ICommand ConnectProfileCommand => new RelayCommand(p => ConnectProfileAction());

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

        private async void AddProfileAction()
        {
            var customDialog = new CustomDialog
            {
                Title = Resources.Localization.Strings.AddProfile
            };

            var profileViewModel = new ProfileViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;

                ProfileManager.AddProfile(instance);
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;
            }, ProfileManager.GetGroups());

            customDialog.Content = new ProfileDialog
            {
                DataContext = profileViewModel
            };

            ConfigurationManager.Current.FixAirspace = true;
            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand EditProfileCommand => new RelayCommand(p => EditProfileAction());

        private async void EditProfileAction()
        {
            var customDialog = new CustomDialog
            {
                Title = Resources.Localization.Strings.EditProfile
            };

            var profileViewModel = new ProfileViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;

                ProfileManager.RemoveProfile(SelectedProfile);

                ProfileManager.AddProfile(instance);
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;
            }, ProfileManager.GetGroups(), ProfileEditMode.Edit, SelectedProfile);

            customDialog.Content = new ProfileDialog
            {
                DataContext = profileViewModel
            };

            ConfigurationManager.Current.FixAirspace = true;
            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand CopyAsProfileCommand => new RelayCommand(p => CopyAsProfileAction());

        private async void CopyAsProfileAction()
        {
            var customDialog = new CustomDialog
            {
                Title = Resources.Localization.Strings.CopyProfile
            };

            var profileViewModel = new ProfileViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;

                ProfileManager.AddProfile(instance);
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;
            }, ProfileManager.GetGroups(), ProfileEditMode.Copy, SelectedProfile);

            customDialog.Content = new ProfileDialog
            {
                DataContext = profileViewModel
            };

            ConfigurationManager.Current.FixAirspace = true;
            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand DeleteProfileCommand => new RelayCommand(p => DeleteProfileAction());

        private async void DeleteProfileAction()
        {
            var customDialog = new CustomDialog
            {
                Title = Resources.Localization.Strings.DeleteProfile
            };

            var confirmRemoveViewModel = new ConfirmRemoveViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;

                ProfileManager.RemoveProfile(SelectedProfile);
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;
            }, Resources.Localization.Strings.DeleteProfileMessage);

            customDialog.Content = new ConfirmRemoveDialog
            {
                DataContext = confirmRemoveViewModel
            };

            ConfigurationManager.Current.FixAirspace = true;
            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand EditGroupCommand => new RelayCommand(EditGroupAction);

        private async void EditGroupAction(object group)
        {
            var customDialog = new CustomDialog
            {
                Title = Resources.Localization.Strings.EditGroup
            };

            var editGroupViewModel = new GroupViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;

                ProfileManager.RenameGroup(instance.OldGroup, instance.Group);

                Profiles.Refresh();
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;
            }, group.ToString(), ProfileManager.GetGroups());

            customDialog.Content = new GroupDialog
            {
                DataContext = editGroupViewModel
            };

            ConfigurationManager.Current.FixAirspace = true;
            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
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
            IsConfigured = !string.IsNullOrEmpty(SettingsManager.Current.TigerVNC_ApplicationFilePath) && File.Exists(SettingsManager.Current.TigerVNC_ApplicationFilePath);
        }

        private async void Connect(string host = null)
        {
            var customDialog = new CustomDialog
            {
                Title = Resources.Localization.Strings.Connect
            };

            var connectViewModel = new TigerVNCConnectViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;

                // Add host to history
                AddHostToHistory(instance.Host);
                AddPortToHistory(instance.Port);

                // Create Profile info
                var info = new TigerVNCSessionInfo
                {
                    Host = instance.Host,
                    Port = instance.Port
                };

                // Connect
                Connect(info);
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;
            }, host);

            customDialog.Content = new TigerVNCConnectDialog
            {
                DataContext = connectViewModel
            };

            ConfigurationManager.Current.FixAirspace = true;
            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        private void ConnectProfile()
        {
            Connect(TigerVNC.CreateSessionInfo(SelectedProfile), SelectedProfile.Name);
        }

        private void ConnectProfileExternal()
        {
            var info = new ProcessStartInfo
            {
                FileName = SettingsManager.Current.TigerVNC_ApplicationFilePath,
                Arguments = TigerVNC.BuildCommandLine(TigerVNC.CreateSessionInfo(SelectedProfile))
            };

            Process.Start(info);
        }

        private void Connect(TigerVNCSessionInfo sessionInfo, string header = null)
        {
            sessionInfo.ApplicationFilePath = SettingsManager.Current.TigerVNC_ApplicationFilePath;

            TabItems.Add(new DragablzTabItem(header ?? sessionInfo.Host, new TigerVNCControl(sessionInfo)));

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
            var list = ListHelper.Modify(SettingsManager.Current.TigerVNC_HostHistory.ToList(), host, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.TigerVNC_HostHistory.Clear();

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.TigerVNC_HostHistory.Add(x));
        }

        private static void AddPortToHistory(int port)
        {
            // Create the new list
            var list = ListHelper.Modify(SettingsManager.Current.TigerVNC_PortHistory.ToList(), port, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.TigerVNC_PortHistory.Clear();

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.TigerVNC_PortHistory.Add(x));
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
            // Refresh profiles
            Profiles.Refresh();
        }

        public void OnViewHide()
        {

        }
        #endregion
    }
}