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
        private IDialogCoordinator dialogCoordinator;

        public IInterTabClient InterTabClient { get; private set; }
        public ObservableCollection<DragablzTabItem> TabItems { get; private set; }

        private const string tagIdentifier = "tag=";

        private bool _isLoading = true;

        private bool _isRDP8dot1Available;
        public bool IsRDP8dot1Available
        {
            get { return _isRDP8dot1Available; }
            set
            {
                if (value == _isRDP8dot1Available)
                    return;

                _isRDP8dot1Available = value;
                OnPropertyChanged();
            }
        }

        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set
            {
                if (value == _selectedTabIndex)
                    return;

                _selectedTabIndex = value;
                OnPropertyChanged();
            }
        }

        #region Profiles
        ICollectionView _profiles;
        public ICollectionView Profiles
        {
            get { return _profiles; }
        }
                
        private ProfileInfo _selectedProfile = new ProfileInfo();
        public ProfileInfo SelectedProfile
        {
            get { return _selectedProfile; }
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
            get { return _search; }
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
            get { return _expandProfileView; }
            set
            {
                if (value == _expandProfileView)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.RemoteDesktop_ExpandProfileView = value;

                _expandProfileView = value;

                if (_canProfileWidthChange)
                    ResizeProfile(dueToChangedSize: false);

                OnPropertyChanged();
            }
        }

        private GridLength _profileWidth;
        public GridLength ProfileWidth
        {
            get { return _profileWidth; }
            set
            {
                if (value == _profileWidth)
                    return;

                if (!_isLoading && value.Value != 40) // Do not save the size when collapsed
                    SettingsManager.Current.RemoteDesktop_ProfileWidth = value.Value;

                _profileWidth = value;

                if (_canProfileWidthChange)
                    ResizeProfile(dueToChangedSize: true);

                OnPropertyChanged();
            }
        }
        #endregion
        #endregion

        #region Constructor, load settings
        public RemoteDesktopHostViewModel(IDialogCoordinator instance)
        {
            dialogCoordinator = instance;

            // Check if RDP 8.1 is available
            IsRDP8dot1Available = Models.RemoteDesktop.RemoteDesktop.IsRDP8dot1Available();

            if (IsRDP8dot1Available)
            {
                InterTabClient = new DragablzInterTabClient(ApplicationViewManager.Name.RemoteDesktop);

                TabItems = new ObservableCollection<DragablzTabItem>();

                _profiles = new CollectionViewSource { Source = ProfileManager.Profiles }.View;
                _profiles.GroupDescriptions.Add(new PropertyGroupDescription(nameof(ProfileInfo.Group)));
                _profiles.SortDescriptions.Add(new SortDescription(nameof(ProfileInfo.Group), ListSortDirection.Ascending));
                _profiles.SortDescriptions.Add(new SortDescription(nameof(ProfileInfo.Name), ListSortDirection.Ascending));
                _profiles.Filter = o =>
                {
                    ProfileInfo info = o as ProfileInfo;

                    if (string.IsNullOrEmpty(Search))
                        return info.RemoteDesktop_Enabled;

                    string search = Search.Trim();

                    // Search by: Tag=xxx (exact match, ignore case)
                    if (search.StartsWith(tagIdentifier, StringComparison.OrdinalIgnoreCase))
                    {
                        if (string.IsNullOrEmpty(info.Tags))
                            return false;
                        else
                            return (info.RemoteDesktop_Enabled && info.Tags.Replace(" ", "").Split(';').Any(str => search.Substring(tagIdentifier.Length, search.Length - tagIdentifier.Length).Equals(str, StringComparison.OrdinalIgnoreCase)));
                    }
                    else // Search by: Name, RemoteDesktop_Host
                    {
                        return (info.RemoteDesktop_Enabled && (info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1 || info.RemoteDesktop_Host.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1));
                    }
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

            if (ExpandProfileView)
                ProfileWidth = new GridLength(SettingsManager.Current.RemoteDesktop_ProfileWidth);
            else
                ProfileWidth = new GridLength(40);

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
            return IsRDP8dot1Available && !ConfigurationManager.Current.IsTransparencyEnabled;
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
            Process.Start("mstsc.exe", string.Format("/V:{0}", SelectedProfile.RemoteDesktop_Host));
        }

        public ICommand AddProfileCommand
        {
            get { return new RelayCommand(p => AddProfileAction()); }
        }

        private async void AddProfileAction()
        {
            CustomDialog customDialog = new CustomDialog()
            {
                Title = LocalizationManager.GetStringByKey("String_Header_AddProfile")
            };

            ProfileViewModel profileViewModel = new ProfileViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.IsDialogOpen = false;

                ProfileManager.AddProfile(instance);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.IsDialogOpen = false;
            }, ProfileManager.GetGroups());

            customDialog.Content = new ProfileDialog
            {
                DataContext = profileViewModel
            };

            ConfigurationManager.Current.IsDialogOpen = true;
            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand EditProfileCommand
        {
            get { return new RelayCommand(p => EditProfileAction()); }
        }

        private async void EditProfileAction()
        {
            CustomDialog customDialog = new CustomDialog()
            {
                Title = LocalizationManager.GetStringByKey("String_Header_EditProfile")
            };

            ProfileViewModel profileViewModel = new ProfileViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.IsDialogOpen = false;

                ProfileManager.RemoveProfile(SelectedProfile);

                ProfileManager.AddProfile(instance);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.IsDialogOpen = false;
            }, ProfileManager.GetGroups(), SelectedProfile);

            customDialog.Content = new ProfileDialog
            {
                DataContext = profileViewModel
            };

            ConfigurationManager.Current.IsDialogOpen = true;
            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand CopyAsProfileCommand
        {
            get { return new RelayCommand(p => CopyAsProfileAction()); }
        }

        private async void CopyAsProfileAction()
        {
            CustomDialog customDialog = new CustomDialog()
            {
                Title = LocalizationManager.GetStringByKey("String_Header_CopyProfile")
            };

            ProfileViewModel profileViewModel = new ProfileViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.IsDialogOpen = false;

                ProfileManager.AddProfile(instance);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.IsDialogOpen = false;
            }, ProfileManager.GetGroups(), SelectedProfile);

            customDialog.Content = new ProfileDialog
            {
                DataContext = profileViewModel
            };

            ConfigurationManager.Current.IsDialogOpen = true;
            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand DeleteProfileCommand
        {
            get { return new RelayCommand(p => DeleteProfileAction()); }
        }

        private async void DeleteProfileAction()
        {
            CustomDialog customDialog = new CustomDialog()
            {
                Title = LocalizationManager.GetStringByKey("String_Header_DeleteProfile")
            };

            ConfirmRemoveViewModel confirmRemoveViewModel = new ConfirmRemoveViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.IsDialogOpen = false;

                ProfileManager.RemoveProfile(SelectedProfile);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.IsDialogOpen = false;
            }, LocalizationManager.GetStringByKey("String_DeleteProfileMessage"));

            customDialog.Content = new ConfirmRemoveDialog
            {
                DataContext = confirmRemoveViewModel
            };

            ConfigurationManager.Current.IsDialogOpen = true;
            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand EditGroupCommand
        {
            get { return new RelayCommand(p => EditGroupAction(p)); }
        }

        private async void EditGroupAction(object group)
        {
            CustomDialog customDialog = new CustomDialog()
            {
                Title = LocalizationManager.GetStringByKey("String_Header_EditGroup")
            };

            GroupViewModel editGroupViewModel = new GroupViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.IsDialogOpen = false;

                ProfileManager.RenameGroup(instance.OldGroup, instance.Group);

                Refresh();
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.IsDialogOpen = false;
            }, group.ToString());

            customDialog.Content = new GroupDialog
            {
                DataContext = editGroupViewModel
            };

            ConfigurationManager.Current.IsDialogOpen = true;
            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand ClearSearchCommand
        {
            get { return new RelayCommand(p => ClearSearchAction()); }
        }

        private void ClearSearchAction()
        {
            Search = string.Empty;
        }

        public ItemActionCallback CloseItemCommand
        {
            get { return CloseItemAction; }
        }

        private void CloseItemAction(ItemActionCallbackArgs<TabablzControl> args)
        {
            ((args.DragablzItem.Content as DragablzTabItem).View as RemoteDesktopControl).CloseTab();
        }

        public ICommand OpenSettingsCommand
        {
            get { return new RelayCommand(p => OpenSettingsAction()); }
        }

        private void OpenSettingsAction()
        {
            EventSystem.RedirectToSettings();
        }
        #endregion

        #region Methods
        private async void Connect(string host = null)
        {
            CustomDialog customDialog = new CustomDialog()
            {
                Title = LocalizationManager.GetStringByKey("String_Header_Connect")
            };

            RemoteDesktopConnectViewModel remoteDesktopConnectViewModel = new RemoteDesktopConnectViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.IsDialogOpen = false;

                // Add host to history
                AddHostToHistory(instance.Host);

                // Create new remote desktop Profile info
                Models.RemoteDesktop.RemoteDesktopSessionInfo session = new Models.RemoteDesktop.RemoteDesktopSessionInfo
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
                        CredentialInfo credentialInfo = CredentialManager.GetCredentialByID((int)instance.CredentialID);

                        session.Username = credentialInfo.Username;
                        session.Password = credentialInfo.Password;
                    }
                }

                Connect(session);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
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
            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        private async void ConnectProfile()
        {
            Models.RemoteDesktop.RemoteDesktopSessionInfo session = new Models.RemoteDesktop.RemoteDesktopSessionInfo
            {
                Hostname = SelectedProfile.RemoteDesktop_Host
            };

            if (SelectedProfile.CredentialID > -1) // Credentials need to be unlocked first
            {
                if (!CredentialManager.Loaded)
                {
                    CustomDialog customDialog = new CustomDialog()
                    {
                        Title = LocalizationManager.GetStringByKey("String_Header_MasterPassword")
                    };

                    CredentialsMasterPasswordViewModel credentialsMasterPasswordViewModel = new CredentialsMasterPasswordViewModel(async instance =>
                    {
                        await dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                        ConfigurationManager.Current.IsDialogOpen = false;

                        if (CredentialManager.Load(instance.Password))
                        {
                            CredentialInfo credentialInfo = CredentialManager.GetCredentialByID(SelectedProfile.CredentialID);

                            if (credentialInfo == null)
                            {
                                ConfigurationManager.Current.IsDialogOpen = true;
                                await dialogCoordinator.ShowMessageAsync(this, LocalizationManager.GetStringByKey("String_Header_CredentialNotFound"), LocalizationManager.GetStringByKey("String_CredentialNotFoundMessage"), MessageDialogStyle.Affirmative, AppearanceManager.MetroDialog);
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
                            await dialogCoordinator.ShowMessageAsync(this, LocalizationManager.GetStringByKey("String_Header_WrongPassword"), LocalizationManager.GetStringByKey("String_WrongPasswordDecryptionFailed"), MessageDialogStyle.Affirmative, AppearanceManager.MetroDialog);
                            ConfigurationManager.Current.IsDialogOpen = false;
                        }
                    }, instance =>
                    {                        
                        dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                        ConfigurationManager.Current.IsDialogOpen = false;
                    });

                    customDialog.Content = new CredentialsMasterPasswordDialog
                    {
                        DataContext = credentialsMasterPasswordViewModel
                    };


                    await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
                }
                else // Connect already unlocked
                {
                    CredentialInfo credentialInfo = CredentialManager.GetCredentialByID(SelectedProfile.CredentialID);

                    if (credentialInfo == null)
                    {
                        ConfigurationManager.Current.IsDialogOpen = true;
                        await dialogCoordinator.ShowMessageAsync(this, LocalizationManager.GetStringByKey("String_Header_CredentialNotFound"), LocalizationManager.GetStringByKey("String_CredentialNotFoundMessage"), MessageDialogStyle.Affirmative, AppearanceManager.MetroDialog);
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
            CustomDialog customDialog = new CustomDialog()
            {
                Title = LocalizationManager.GetStringByKey("String_Header_ConnectAs")
            };

            RemoteDesktopConnectViewModel remoteDesktopConnectViewModel = new RemoteDesktopConnectViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.IsDialogOpen = false;

                Models.RemoteDesktop.RemoteDesktopSessionInfo session = new Models.RemoteDesktop.RemoteDesktopSessionInfo
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
                        CredentialInfo credentialInfo = CredentialManager.GetCredentialByID((int)instance.CredentialID);

                        session.Username = credentialInfo.Username;
                        session.Password = credentialInfo.Password;
                    }
                }

                Connect(session, instance.Name);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
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
            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        private void Connect(Models.RemoteDesktop.RemoteDesktopSessionInfo ProfileInfo, string Header = null)
        {
            // Add global settings...
            ProfileInfo.AdjustScreenAutomatically = SettingsManager.Current.RemoteDesktop_AdjustScreenAutomatically;
            ProfileInfo.UseCurrentViewSize = SettingsManager.Current.RemoteDesktop_UseCurrentViewSize;

            if (SettingsManager.Current.RemoteDesktop_UseCustomScreenSize)
            {
                ProfileInfo.DesktopWidth = SettingsManager.Current.RemoteDesktop_CustomScreenWidth;
                ProfileInfo.DesktopHeight = SettingsManager.Current.RemoteDesktop_CustomScreenHeight;
            }
            else
            {
                ProfileInfo.DesktopWidth = SettingsManager.Current.RemoteDesktop_ScreenWidth;
                ProfileInfo.DesktopHeight = SettingsManager.Current.RemoteDesktop_ScreenHeight;
            }

            ProfileInfo.ColorDepth = SettingsManager.Current.RemoteDesktop_ColorDepth;
            ProfileInfo.Port = SettingsManager.Current.RemoteDesktop_Port;
            ProfileInfo.EnableCredSspSupport = SettingsManager.Current.RemoteDesktop_EnableCredSspSupport;
            ProfileInfo.AuthenticationLevel = SettingsManager.Current.RemoteDesktop_AuthenticationLevel;
            ProfileInfo.RedirectClipboard = SettingsManager.Current.RemoteDesktop_RedirectClipboard;
            ProfileInfo.RedirectDevices = SettingsManager.Current.RemoteDesktop_RedirectDevices;
            ProfileInfo.RedirectDrives = SettingsManager.Current.RemoteDesktop_RedirectDrives;
            ProfileInfo.RedirectPorts = SettingsManager.Current.RemoteDesktop_RedirectPorts;
            ProfileInfo.RedirectSmartCards = SettingsManager.Current.RemoteDesktop_RedirectSmartCards;

            TabItems.Add(new DragablzTabItem(Header ?? ProfileInfo.Hostname, new RemoteDesktopControl(ProfileInfo)));
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
            List<string> list = ListHelper.Modify(SettingsManager.Current.RemoteDesktop_HostHistory.ToList(), host, SettingsManager.Current.General_HistoryListEntries);

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
                if (ProfileWidth.Value == 40)
                    ExpandProfileView = false;
                else
                    ExpandProfileView = true;
            }
            else
            {
                if (ExpandProfileView)
                {
                    if (_tempProfileWidth == 40)
                        ProfileWidth = new GridLength(250);
                    else
                        ProfileWidth = new GridLength(_tempProfileWidth);
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