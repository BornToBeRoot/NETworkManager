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
using System.IO;
using NETworkManager.Utilities;
using System.Diagnostics;
using NETworkManager.Models.PuTTY;
using System.Windows;

namespace NETworkManager.ViewModels
{
    public class PuTTYHostViewModel : ViewModelBase
    {
        #region Variables
        private readonly IDialogCoordinator _dialogCoordinator;

        public IInterTabClient InterTabClient { get; }
        public ObservableCollection<DragablzTabItem> TabItems { get; }

        private const string TagIdentifier = "tag=";

        private readonly bool _isLoading;

        private bool _isPuTTYConfigured;
        public bool IsPuTTYConfigured
        {
            get => _isPuTTYConfigured;
            set
            {
                if (value == _isPuTTYConfigured)
                    return;

                _isPuTTYConfigured = value;
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

                if (!_isLoading && value.Value != 40) // Do not save the size when collapsed
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
            _isLoading = true;

            _dialogCoordinator = instance;

            // Check if putty is available...
            CheckIfPuTTYConfigured();

            InterTabClient = new DragablzInterTabClient(ApplicationViewManager.Name.PuTTY);

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
                    return info.PuTTY_Enabled;

                var search = Search.Trim();

                // Search by: Tag=xxx (exact match, ignore case)
                if (search.StartsWith(TagIdentifier, StringComparison.OrdinalIgnoreCase))
                    return !string.IsNullOrEmpty(info.Tags) && info.PuTTY_Enabled && info.Tags.Replace(" ", "").Split(';').Any(str => search.Substring(TagIdentifier.Length, search.Length - TagIdentifier.Length).Equals(str, StringComparison.OrdinalIgnoreCase));

                // Search by: Name, PuTTY_HostOrSerialLine
                return info.PuTTY_Enabled && (info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1 || info.PuTTY_HostOrSerialLine.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1);
            };

            // This will select the first entry as selected item...
            SelectedProfile = Profiles.SourceCollection.Cast<ProfileInfo>().Where(x => x.PuTTY_Enabled).OrderBy(x => x.Group).ThenBy(x => x.Name).FirstOrDefault();

            LoadSettings();

            SettingsManager.Current.PropertyChanged += Current_PropertyChanged;

            _isLoading = false;
        }

        private void Current_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SettingsInfo.PuTTY_PuTTYLocation))
                CheckIfPuTTYConfigured();
        }

        private void LoadSettings()
        {
            ExpandProfileView = SettingsManager.Current.PuTTY_ExpandProfileView;

            ProfileWidth = ExpandProfileView ? new GridLength(SettingsManager.Current.PuTTY_ProfileWidth) : new GridLength(40);

            _tempProfileWidth = SettingsManager.Current.PuTTY_ProfileWidth;
        }
        #endregion

        #region ICommand & Actions
        public ItemActionCallback CloseItemCommand => CloseItemAction;

        private void CloseItemAction(ItemActionCallbackArgs<TabablzControl> args)
        {
            ((args.DragablzItem.Content as DragablzTabItem)?.View as PuttyControl)?.CloseTab();
        }

        public ICommand ConnectCommand
        {
            get { return new RelayCommand(p => ConnectAction(), Connect_CanExecute); }
        }

        private bool Connect_CanExecute(object obj)
        {
            return IsPuTTYConfigured && !ConfigurationManager.Current.IsTransparencyEnabled;
        }

        private void ConnectAction()
        {
            Connect();
        }

        public ICommand ConnectProfileCommand
        {
            get { return new RelayCommand(p => ConnectProfileAction()); }
        }

        private void ConnectProfileAction()
        {
            ConnectProfile();
        }

        public ICommand ConnectProfileExternalCommand
        {
            get { return new RelayCommand(p => ConnectProfileExternalAction()); }
        }

        private void ConnectProfileExternalAction()
        {
            ConnectProfileExternal();
        }

        public ICommand AddProfileCommand
        {
            get { return new RelayCommand(p => AddProfileAction()); }
        }

        private async void AddProfileAction()
        {
            var customDialog = new CustomDialog
            {
                Title = Resources.Localization.Strings.AddProfile
            };

            var profileViewModel = new ProfileViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.IsDialogOpen = false;

                ProfileManager.AddProfile(instance);
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.IsDialogOpen = false;
            }, ProfileManager.GetGroups());

            customDialog.Content = new ProfileDialog
            {
                DataContext = profileViewModel
            };

            ConfigurationManager.Current.IsDialogOpen = true;
            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand EditProfileCommand
        {
            get { return new RelayCommand(p => EditProfileAction()); }
        }

        private async void EditProfileAction()
        {
            var customDialog = new CustomDialog
            {
                Title = Resources.Localization.Strings.EditProfile
            };

            var profileViewModel = new ProfileViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.IsDialogOpen = false;

                ProfileManager.RemoveProfile(SelectedProfile);

                ProfileManager.AddProfile(instance);
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.IsDialogOpen = false;
            }, ProfileManager.GetGroups(), true,SelectedProfile);

            customDialog.Content = new ProfileDialog
            {
                DataContext = profileViewModel
            };

            ConfigurationManager.Current.IsDialogOpen = true;
            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand CopyAsProfileCommand
        {
            get { return new RelayCommand(p => CopyAsProfileAction()); }
        }

        private async void CopyAsProfileAction()
        {
            var customDialog = new CustomDialog
            {
                Title = Resources.Localization.Strings.CopyProfile
            };

            var profileViewModel = new ProfileViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.IsDialogOpen = false;

                ProfileManager.AddProfile(instance);
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.IsDialogOpen = false;
            }, ProfileManager.GetGroups(), false,SelectedProfile);

            customDialog.Content = new ProfileDialog
            {
                DataContext = profileViewModel
            };

            ConfigurationManager.Current.IsDialogOpen = true;
            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand DeleteProfileCommand
        {
            get { return new RelayCommand(p => DeleteProfileAction()); }
        }

        private async void DeleteProfileAction()
        {
            var customDialog = new CustomDialog
            {
                Title = Resources.Localization.Strings.DeleteProfile
            };

            var confirmRemoveViewModel = new ConfirmRemoveViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.IsDialogOpen = false;

                ProfileManager.RemoveProfile(SelectedProfile);
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.IsDialogOpen = false;
            }, Resources.Localization.Strings.DeleteProfileMessage);

            customDialog.Content = new ConfirmRemoveDialog
            {
                DataContext = confirmRemoveViewModel
            };

            ConfigurationManager.Current.IsDialogOpen = true;
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
                ConfigurationManager.Current.IsDialogOpen = false;

                ProfileManager.RenameGroup(instance.OldGroup, instance.Group);

                Refresh();
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.IsDialogOpen = false;
            }, group.ToString(), ProfileManager.GetGroups());

            customDialog.Content = new GroupDialog
            {
                DataContext = editGroupViewModel
            };

            ConfigurationManager.Current.IsDialogOpen = true;
            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand ClearSearchCommand
        {
            get { return new RelayCommand(p => ClearSearchAction()); }
        }

        private void ClearSearchAction()
        {
            Search = string.Empty;
        }

        public ICommand OpenSettingsCommand
        {
            get { return new RelayCommand(p => OpenSettingsAction()); }
        }

        private static void OpenSettingsAction()
        {
            EventSystem.RedirectToSettings();
        }
        #endregion

        #region Methods
        private void CheckIfPuTTYConfigured()
        {
            IsPuTTYConfigured = !string.IsNullOrEmpty(SettingsManager.Current.PuTTY_PuTTYLocation) && File.Exists(SettingsManager.Current.PuTTY_PuTTYLocation);
        }

        private async void Connect(string host = null)
        {
            var customDialog = new CustomDialog
            {
                Title = Resources.Localization.Strings.Connect
            };

            var puTTYConnectViewModel = new PuTTYConnectViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.IsDialogOpen = false;

                // Add host to history
                AddHostToHistory(instance.Host);
                AddSerialLineToHistory(instance.SerialLine);
                AddPortToHistory(instance.Port.ToString());
                AddBaudToHistory(instance.Baud.ToString());
                AddUsernameToHistory(instance.Username);
                AddProfileToHistory(instance.Profile);

                // Create Profile info
                var puTTYProfileInfo = new PuTTYProfileInfo
                {
                    HostOrSerialLine = instance.ConnectionMode == PuTTY.ConnectionMode.Serial ? instance.SerialLine : instance.Host,
                    Mode = instance.ConnectionMode,
                    PortOrBaud = instance.ConnectionMode == PuTTY.ConnectionMode.Serial ? instance.Baud : instance.Port,
                    Username = instance.Username,
                    Profile = instance.Profile,
                    AdditionalCommandLine = instance.AdditionalCommandLine
                };

                // Connect
                Connect(puTTYProfileInfo);
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.IsDialogOpen = false;
            })
            {
                Host = host
            };

            customDialog.Content = new PuTTYConnectDialog
            {
                DataContext = puTTYConnectViewModel
            };

            ConfigurationManager.Current.IsDialogOpen = true;
            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        private void ConnectProfile()
        {
            Connect(PuTTYProfileInfo.Parse(SelectedProfile), SelectedProfile.Name);
        }

        private void ConnectProfileExternal()
        {
            var info = new ProcessStartInfo
            {
                FileName = SettingsManager.Current.PuTTY_PuTTYLocation,
                Arguments = PuTTY.BuildCommandLine(PuTTYProfileInfo.Parse(SelectedProfile))
            };

            Process.Start(info);
        }

        private void Connect(PuTTYProfileInfo profileInfo, string header = null)
        {
            // Add PuTTY path here...
            profileInfo.PuTTYLocation = SettingsManager.Current.PuTTY_PuTTYLocation;

            TabItems.Add(new DragablzTabItem(header ?? profileInfo.HostOrSerialLine, new PuttyControl(profileInfo)));

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
            var list = ListHelper.Modify(SettingsManager.Current.PuTTY_SerialLineHistory.ToList(), serialLine, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.PuTTY_SerialLineHistory.Clear();

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.PuTTY_SerialLineHistory.Add(x));
        }

        private static void AddPortToHistory(string host)
        {
            // Create the new list
            var list = ListHelper.Modify(SettingsManager.Current.PuTTY_PortHistory.ToList(), host, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.PuTTY_PortHistory.Clear();

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.PuTTY_PortHistory.Add(x));
        }

        private static void AddBaudToHistory(string host)
        {
            // Create the new list
            var list = ListHelper.Modify(SettingsManager.Current.PuTTY_BaudHistory.ToList(), host, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.PuTTY_BaudHistory.Clear();

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.PuTTY_BaudHistory.Add(x));
        }

        private static void AddUsernameToHistory(string host)
        {
            // Create the new list
            var list = ListHelper.Modify(SettingsManager.Current.PuTTY_UsernameHistory.ToList(), host, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.PuTTY_UsernameHistory.Clear();

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.PuTTY_UsernameHistory.Add(x));
        }

        private static void AddProfileToHistory(string host)
        {
            // Create the new list
            var list = ListHelper.Modify(SettingsManager.Current.PuTTY_ProfileHistory.ToList(), host, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.PuTTY_ProfileHistory.Clear();

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.PuTTY_ProfileHistory.Add(x));
        }

        private void ResizeProfile(bool dueToChangedSize)
        {
            _canProfileWidthChange = false;

            if (dueToChangedSize)
            {
                ExpandProfileView = ProfileWidth.Value != 40;
            }
            else
            {
                if (ExpandProfileView)
                {
                    ProfileWidth = _tempProfileWidth == 40 ? new GridLength(250) : new GridLength(_tempProfileWidth);
                }
                else
                {
                    _tempProfileWidth = ProfileWidth.Value;
                    ProfileWidth = new GridLength(40);
                }
            }

            _canProfileWidthChange = true;
        }

        public void Refresh()
        {
            // Refresh profiles
            Profiles.Refresh();
        }
        #endregion
    }
}