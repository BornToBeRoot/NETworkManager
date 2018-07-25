using System.Collections.ObjectModel;
using NETworkManager.Controls;
using Dragablz;
using MahApps.Metro.Controls.Dialogs;
using System.Windows.Input;
using NETworkManager.Views;
using NETworkManager.Models.Settings;
using System.ComponentModel;
using System.Windows.Data;
using System;
using System.Linq;
using System.Diagnostics;
using NETworkManager.Utilities;
using System.Collections.Generic;
using System.Windows;

namespace NETworkManager.ViewModels
{
    public class RemoteDesktopHostViewModel : ViewModelBase
    {
        #region Variables
        private readonly IDialogCoordinator _dialogCoordinator;

        public IInterTabClient InterTabClient { get; }
        public ObservableCollection<DragablzTabItem> TabItems { get; }

        private const string TagIdentifier = "tag=";

        private readonly bool _isLoading;

        private bool _isRDP8Dot1Available;
        public bool IsRDP8Dot1Available
        {
            get => _isRDP8Dot1Available;
            set
            {
                if (value == _isRDP8Dot1Available)
                    return;

                _isRDP8Dot1Available = value;
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

                if (!_isLoading && value.Value != 40) // Do not save the size when collapsed
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

            // Check if RDP 8.1 is available
            IsRDP8Dot1Available = Models.RemoteDesktop.RemoteDesktop.IsRDP8Dot1Available;

            if (IsRDP8Dot1Available)
            {
                InterTabClient = new DragablzInterTabClient(ApplicationViewManager.Name.RemoteDesktop);

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
                    if (search.StartsWith(TagIdentifier, StringComparison.OrdinalIgnoreCase))
                        return !string.IsNullOrEmpty(info.Tags) && info.RemoteDesktop_Enabled && info.Tags.Replace(" ", "").Split(';').Any(str => search.Substring(TagIdentifier.Length, search.Length - TagIdentifier.Length).Equals(str, StringComparison.OrdinalIgnoreCase));

                    // Search by: Name, RemoteDesktop_Host
                    return info.RemoteDesktop_Enabled && (info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1 || info.RemoteDesktop_Host.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1);
                };

                // This will select the first entry as selected item...
                SelectedProfile = Profiles.SourceCollection.Cast<ProfileInfo>().Where(x => x.RemoteDesktop_Enabled).OrderBy(x => x.Group).ThenBy(x => x.Name).FirstOrDefault();


                LoadSettings();
            }

            _isLoading = false;
        }

        private void LoadSettings()
        {
            ExpandProfileView = SettingsManager.Current.RemoteDesktop_ExpandProfileView;

            ProfileWidth = ExpandProfileView ? new GridLength(SettingsManager.Current.RemoteDesktop_ProfileWidth) : new GridLength(40);

            _tempProfileWidth = SettingsManager.Current.RemoteDesktop_ProfileWidth;
        }
        #endregion

        #region ICommand & Actions        
        public ICommand ConnectCommand
        {
            get { return new RelayCommand(p => ConnectAction(), Connect_CanExecute); }
        }

        private bool Connect_CanExecute(object parameter)
        {
            return IsRDP8Dot1Available && !ConfigurationManager.Current.IsTransparencyEnabled;
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

        public ICommand ConnectProfileAsCommand
        {
            get { return new RelayCommand(p => ConnectProfileAsAction()); }
        }

        private void ConnectProfileAsAction()
        {
            ConnectProfileAs();
        }

        public ICommand ConnectProfileExternalCommand
        {
            get { return new RelayCommand(p => ConnectProfileExternalAction()); }
        }

        private void ConnectProfileExternalAction()
        {
            Process.Start("mstsc.exe", $"/V:{SelectedProfile.RemoteDesktop_Host}");
        }

        public ICommand AddProfileCommand
        {
            get { return new RelayCommand(p => AddProfileAction()); }
        }

        private async void AddProfileAction()
        {
            var customDialog = new CustomDialog
            {
                Title = LocalizationManager.GetStringByKey("String_Header_AddProfile")
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
            var customDialog = new CustomDialog()
            {
                Title = LocalizationManager.GetStringByKey("String_Header_EditProfile")
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
            }, ProfileManager.GetGroups(), SelectedProfile);

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
            var customDialog = new CustomDialog()
            {
                Title = LocalizationManager.GetStringByKey("String_Header_CopyProfile")
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
            }, ProfileManager.GetGroups(), SelectedProfile);

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
            var customDialog = new CustomDialog()
            {
                Title = LocalizationManager.GetStringByKey("String_Header_DeleteProfile")
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
            }, LocalizationManager.GetStringByKey("String_DeleteProfileMessage"));

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
            var customDialog = new CustomDialog()
            {
                Title = LocalizationManager.GetStringByKey("String_Header_EditGroup")
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
            }, group.ToString());

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

        public ItemActionCallback CloseItemCommand => CloseItemAction;

        private static void CloseItemAction(ItemActionCallbackArgs<TabablzControl> args)
        {
            ((args.DragablzItem.Content as DragablzTabItem)?.View as RemoteDesktopControl)?.CloseTab();
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
        private async void Connect(string host = null)
        {
            var customDialog = new CustomDialog()
            {
                Title = LocalizationManager.GetStringByKey("String_Header_Connect")
            };

            var remoteDesktopConnectViewModel = new RemoteDesktopConnectViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.IsDialogOpen = false;

                // Add host to history
                AddHostToHistory(instance.Host);

                // Create new remote desktop Profile info
                var session = new Models.RemoteDesktop.RemoteDesktopSessionInfo
                {
                    Hostname = instance.Host
                };

                if (instance.UseCredentials)
                {
                    session.CustomCredentials = true;

                    if (instance.CustomCredentials)
                    {
                        session.Username = instance.Username;
                        session.Password = instance.Password;
                    }
                    else
                    {
                        if (instance.CredentialID != null)
                        {
                            var credentialInfo = CredentialManager.GetCredentialByID((int)instance.CredentialID);

                            session.Username = credentialInfo.Username;
                            session.Password = credentialInfo.Password;
                        }
                    }
                }

                Connect(session);
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.IsDialogOpen = false;
            })
            {
                Host = host
            };

            customDialog.Content = new RemoteDesktopConnectDialog
            {
                DataContext = remoteDesktopConnectViewModel
            };

            ConfigurationManager.Current.IsDialogOpen = true;
            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        private async void ConnectProfile()
        {
            var session = new Models.RemoteDesktop.RemoteDesktopSessionInfo
            {
                Hostname = SelectedProfile.RemoteDesktop_Host
            };

            if (SelectedProfile.CredentialID > -1) // Credentials need to be unlocked first
            {
                if (!CredentialManager.IsLoaded)
                {
                    var customDialog = new CustomDialog()
                    {
                        Title = LocalizationManager.GetStringByKey("String_Header_MasterPassword")
                    };

                    var credentialsMasterPasswordViewModel = new CredentialsMasterPasswordViewModel(async instance =>
                    {
                        await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                        ConfigurationManager.Current.IsDialogOpen = false;

                        if (CredentialManager.Load(instance.Password))
                        {
                            var credentialInfo = CredentialManager.GetCredentialByID(SelectedProfile.CredentialID);

                            if (credentialInfo == null)
                            {
                                ConfigurationManager.Current.IsDialogOpen = true;
                                await _dialogCoordinator.ShowMessageAsync(this, LocalizationManager.GetStringByKey("String_Header_CredentialNotFound"), LocalizationManager.GetStringByKey("String_CredentialNotFoundMessage"), MessageDialogStyle.Affirmative, AppearanceManager.MetroDialog);
                                ConfigurationManager.Current.IsDialogOpen = false;

                                return;
                            }

                            session.CustomCredentials = true;
                            session.Username = credentialInfo.Username;
                            session.Password = credentialInfo.Password;

                            Connect(session, SelectedProfile.Name);
                        }
                        else
                        {
                            ConfigurationManager.Current.IsDialogOpen = true;
                            await _dialogCoordinator.ShowMessageAsync(this, LocalizationManager.GetStringByKey("String_Header_WrongPassword"), LocalizationManager.GetStringByKey("String_WrongPasswordDecryptionFailed"), MessageDialogStyle.Affirmative, AppearanceManager.MetroDialog);
                            ConfigurationManager.Current.IsDialogOpen = false;
                        }
                    }, instance =>
                    {                        
                        _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                        ConfigurationManager.Current.IsDialogOpen = false;
                    });

                    customDialog.Content = new CredentialsMasterPasswordDialog
                    {
                        DataContext = credentialsMasterPasswordViewModel
                    };

                    ConfigurationManager.Current.IsDialogOpen = true;
                    await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
                }
                else // Connect already unlocked
                {
                    var credentialInfo = CredentialManager.GetCredentialByID(SelectedProfile.CredentialID);

                    if (credentialInfo == null)
                    {
                        ConfigurationManager.Current.IsDialogOpen = true;
                        await _dialogCoordinator.ShowMessageAsync(this, LocalizationManager.GetStringByKey("String_Header_CredentialNotFound"), LocalizationManager.GetStringByKey("String_CredentialNotFoundMessage"), MessageDialogStyle.Affirmative, AppearanceManager.MetroDialog);
                        ConfigurationManager.Current.IsDialogOpen = false;

                        return;
                    }

                    session.CustomCredentials = true;
                    session.Username = credentialInfo.Username;
                    session.Password = credentialInfo.Password;

                    Connect(session, SelectedProfile.Name);
                }
            }
            else // Connect without credentials
            {
                Connect(session, SelectedProfile.Name);
            }
        }

        private async void ConnectProfileAs()
        {
            var customDialog = new CustomDialog()
            {
                Title = LocalizationManager.GetStringByKey("String_Header_ConnectAs")
            };

            var remoteDesktopConnectViewModel = new RemoteDesktopConnectViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.IsDialogOpen = false;

                var session = new Models.RemoteDesktop.RemoteDesktopSessionInfo
                {
                    Hostname = instance.Host
                };

                if (instance.UseCredentials)
                {
                    session.CustomCredentials = true;

                    if (instance.CustomCredentials)
                    {
                        session.Username = instance.Username;
                        session.Password = instance.Password;
                    }
                    else
                    {
                        if (instance.CredentialID != null)
                        {
                            var credentialInfo = CredentialManager.GetCredentialByID((int)instance.CredentialID);

                            session.Username = credentialInfo.Username;
                            session.Password = credentialInfo.Password;
                        }
                    }
                }

                Connect(session, instance.Name);
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.IsDialogOpen = false;
            }, true)
            {
                // Set name, hostname
                Name = SelectedProfile.Name,
                Host = SelectedProfile.RemoteDesktop_Host,

                // Request credentials
                UseCredentials = true
            };

            customDialog.Content = new RemoteDesktopConnectDialog
            {
                DataContext = remoteDesktopConnectViewModel
            };

            ConfigurationManager.Current.IsDialogOpen = true;
            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        private void Connect(Models.RemoteDesktop.RemoteDesktopSessionInfo profileInfo, string header = null)
        {
            // Add global settings...
            profileInfo.AdjustScreenAutomatically = SettingsManager.Current.RemoteDesktop_AdjustScreenAutomatically;
            profileInfo.UseCurrentViewSize = SettingsManager.Current.RemoteDesktop_UseCurrentViewSize;

            if (SettingsManager.Current.RemoteDesktop_UseCustomScreenSize)
            {
                profileInfo.DesktopWidth = SettingsManager.Current.RemoteDesktop_CustomScreenWidth;
                profileInfo.DesktopHeight = SettingsManager.Current.RemoteDesktop_CustomScreenHeight;
            }
            else
            {
                profileInfo.DesktopWidth = SettingsManager.Current.RemoteDesktop_ScreenWidth;
                profileInfo.DesktopHeight = SettingsManager.Current.RemoteDesktop_ScreenHeight;
            }

            profileInfo.ColorDepth = SettingsManager.Current.RemoteDesktop_ColorDepth;
            profileInfo.Port = SettingsManager.Current.RemoteDesktop_Port;
            profileInfo.EnableCredSspSupport = SettingsManager.Current.RemoteDesktop_EnableCredSspSupport;
            profileInfo.AuthenticationLevel = SettingsManager.Current.RemoteDesktop_AuthenticationLevel;
            profileInfo.KeyboardHookMode = SettingsManager.Current.RemoteDesktop_KeyboardHookMode;
            profileInfo.RedirectClipboard = SettingsManager.Current.RemoteDesktop_RedirectClipboard;
            profileInfo.RedirectDevices = SettingsManager.Current.RemoteDesktop_RedirectDevices;
            profileInfo.RedirectDrives = SettingsManager.Current.RemoteDesktop_RedirectDrives;
            profileInfo.RedirectPorts = SettingsManager.Current.RemoteDesktop_RedirectPorts;
            profileInfo.RedirectSmartCards = SettingsManager.Current.RemoteDesktop_RedirectSmartCards;

            TabItems.Add(new DragablzTabItem(header ?? profileInfo.Hostname, new RemoteDesktopControl(profileInfo)));
            SelectedTabIndex = TabItems.Count - 1;
        }

        public void AddTab(string host)
        {
            Connect(host);
        }

        // Modify history list
        private void AddHostToHistory(string host)
        {
            // Create the new list
            var list = ListHelper.Modify(SettingsManager.Current.RemoteDesktop_HostHistory.ToList(), host, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.RemoteDesktop_HostHistory.Clear();

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.RemoteDesktop_HostHistory.Add(x));
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