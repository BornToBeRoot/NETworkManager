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
using System.Threading.Tasks;

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

        #region Sessions
        ICollectionView _remoteDesktopSessions;
        public ICollectionView RemoteDesktopSessions
        {
            get { return _remoteDesktopSessions; }
        }

        private RemoteDesktopSessionInfo _selectedSession = new RemoteDesktopSessionInfo();
        public RemoteDesktopSessionInfo SelectedSession
        {
            get { return _selectedSession; }
            set
            {
                if (value == _selectedSession)
                    return;

                _selectedSession = value;
                OnPropertyChanged();
            }
        }

        private bool _expandSessionView;
        public bool ExpandSessionView
        {
            get { return _expandSessionView; }
            set
            {
                if (value == _expandSessionView)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.RemoteDesktop_ExpandSessionView = value;

                _expandSessionView = value;
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

                RemoteDesktopSessions.Refresh();

                OnPropertyChanged();
            }
        }
        #endregion
        #endregion

        #region Constructor
        public RemoteDesktopHostViewModel(IDialogCoordinator instance)
        {
            dialogCoordinator = instance;

            // Check if RDP 8.1 is available
            IsRDP8dot1Available = Models.RemoteDesktop.RemoteDesktop.IsRDP8dot1Available();

            if (IsRDP8dot1Available)
            {
                InterTabClient = new DragablzRemoteDesktopInterTabClient();
                TabItems = new ObservableCollection<DragablzTabItem>();

                // Load sessions
                if (RemoteDesktopSessionManager.Sessions == null)
                    RemoteDesktopSessionManager.Load();

                _remoteDesktopSessions = CollectionViewSource.GetDefaultView(RemoteDesktopSessionManager.Sessions);
                _remoteDesktopSessions.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
                _remoteDesktopSessions.SortDescriptions.Add(new SortDescription("Group", ListSortDirection.Ascending));
                _remoteDesktopSessions.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                _remoteDesktopSessions.Filter = o =>
                {
                    if (string.IsNullOrEmpty(Search))
                        return true;

                    RemoteDesktopSessionInfo info = o as RemoteDesktopSessionInfo;

                    string search = Search.Trim();

                    // Search by: Tag
                    if (search.StartsWith(tagIdentifier, StringComparison.OrdinalIgnoreCase))
                    {
                        if (string.IsNullOrEmpty(info.Tags))
                            return false;
                        else
                            return info.Tags.Replace(" ", "").Split(';').Any(str => search.Substring(tagIdentifier.Length, search.Length - tagIdentifier.Length).IndexOf(str, StringComparison.OrdinalIgnoreCase) > -1);
                    }
                    else // Search by: Name, Hostname
                    {
                        return info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1 || info.Host.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1;
                    }
                };

                LoadSettings();
            }

            _isLoading = false;
        }

        private void LoadSettings()
        {
            ExpandSessionView = SettingsManager.Current.RemoteDesktop_ExpandSessionView;
        }
        #endregion

        #region ICommand & Actions        
        public ICommand ConnectNewSessionCommand
        {
            get { return new RelayCommand(p => ConnectNewSessionAction(), ConnectNewSession_CanExecute); }
        }

        private bool ConnectNewSession_CanExecute(object parameter)
        {
            return IsRDP8dot1Available;
        }

        private void ConnectNewSessionAction()
        {
            ConnectNewSession();
        }

        public ICommand AddSessionCommand
        {
            get { return new RelayCommand(p => AddSessionAction()); }
        }

        private async void AddSessionAction()
        {
            CustomDialog customDialog = new CustomDialog()
            {
                Title = LocalizationManager.GetStringByKey("String_Header_AddSession")
            };

            RemoteDesktopSessionViewModel remoteDesktopSessionViewModel = new RemoteDesktopSessionViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;

                RemoteDesktopSessionInfo remoteDesktopSessionInfo = new RemoteDesktopSessionInfo
                {
                    Name = instance.Name,
                    Host = instance.Host,
                    CredentialID = instance.CredentialID,
                    Group = instance.Group,
                    Tags = instance.Tags
                };

                RemoteDesktopSessionManager.AddSession(remoteDesktopSessionInfo);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;
            }, RemoteDesktopSessionManager.GetSessionGroups());

            customDialog.Content = new RemoteDesktopSessionDialog
            {
                DataContext = remoteDesktopSessionViewModel
            };

            ConfigurationManager.Current.FixAirspace = true;
            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand ConnectSessionCommand
        {
            get { return new RelayCommand(p => ConnectSessionAction()); }
        }

        private void ConnectSessionAction()
        {
            ConnectSession();
        }

        public ICommand ConnectSessionAsCommand
        {
            get { return new RelayCommand(p => ConnectSessionAsAction()); }
        }

        private void ConnectSessionAsAction()
        {
            ConnectSessionAs();
        }

        public ICommand ConnectSessionExternalCommand
        {
            get { return new RelayCommand(p => ConnectSessionExternalAction()); }
        }

        private void ConnectSessionExternalAction()
        {
            Process.Start("mstsc.exe", string.Format("/V:{0}", SelectedSession.Host));
        }

        public ICommand EditSessionCommand
        {
            get { return new RelayCommand(p => EditSessionAction()); }
        }

        private async void EditSessionAction()
        {
            CustomDialog customDialog = new CustomDialog()
            {
                Title = LocalizationManager.GetStringByKey("String_Header_EditSession")
            };

            RemoteDesktopSessionViewModel remoteDesktopSessionViewModel = new RemoteDesktopSessionViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;

                RemoteDesktopSessionManager.RemoveSession(SelectedSession);

                RemoteDesktopSessionInfo remoteDesktopSessionInfo = new RemoteDesktopSessionInfo
                {
                    Name = instance.Name,
                    Host = instance.Host,
                    CredentialID = instance.CredentialID,
                    Group = instance.Group,
                    Tags = instance.Tags
                };

                RemoteDesktopSessionManager.AddSession(remoteDesktopSessionInfo);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;
            }, RemoteDesktopSessionManager.GetSessionGroups(), SelectedSession);

            customDialog.Content = new RemoteDesktopSessionDialog
            {
                DataContext = remoteDesktopSessionViewModel
            };

            ConfigurationManager.Current.FixAirspace = true;
            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand CopyAsSessionCommand
        {
            get { return new RelayCommand(p => CopyAsSessionAction()); }
        }

        private async void CopyAsSessionAction()
        {
            CustomDialog customDialog = new CustomDialog()
            {
                Title = LocalizationManager.GetStringByKey("String_Header_CopySession")
            };

            RemoteDesktopSessionViewModel remoteDesktopSessionViewModel = new RemoteDesktopSessionViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;

                RemoteDesktopSessionInfo remoteDesktopSessionInfo = new RemoteDesktopSessionInfo
                {
                    Name = instance.Name,
                    Host = instance.Host,
                    Group = instance.Group,
                    Tags = instance.Tags
                };

                RemoteDesktopSessionManager.AddSession(remoteDesktopSessionInfo);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;
            }, RemoteDesktopSessionManager.GetSessionGroups(), SelectedSession);

            customDialog.Content = new RemoteDesktopSessionDialog
            {
                DataContext = remoteDesktopSessionViewModel
            };

            ConfigurationManager.Current.FixAirspace = true;
            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand DeleteSessionCommand
        {
            get { return new RelayCommand(p => DeleteSessionAction()); }
        }

        private async void DeleteSessionAction()
        {
            CustomDialog customDialog = new CustomDialog()
            {
                Title = LocalizationManager.GetStringByKey("String_Header_DeleteSession")
            };

            ConfirmRemoveViewModel confirmRemoveViewModel = new ConfirmRemoveViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;

                RemoteDesktopSessionManager.RemoveSession(SelectedSession);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;
            }, LocalizationManager.GetStringByKey("String_DeleteSessionMessage"));

            customDialog.Content = new ConfirmRemoveDialog
            {
                DataContext = confirmRemoveViewModel
            };

            ConfigurationManager.Current.FixAirspace = true;
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
                ConfigurationManager.Current.FixAirspace = false;

                RemoteDesktopSessionManager.RenameGroup(instance.OldGroup, instance.Group);

                _remoteDesktopSessions.Refresh();
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;
            }, group.ToString());

            customDialog.Content = new GroupDialog
            {
                DataContext = editGroupViewModel
            };

            ConfigurationManager.Current.FixAirspace = true;
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
            ((args.DragablzItem.Content as DragablzTabItem).View as RemoteDesktopControl).OnClose();
        }
        #endregion

        #region Methods
        private async void ConnectNewSession(string host = null)
        {
            CustomDialog customDialog = new CustomDialog()
            {
                Title = LocalizationManager.GetStringByKey("String_Header_Connect")
            };

            RemoteDesktopSessionConnectViewModel remoteDesktopSessionConnectViewModel = new RemoteDesktopSessionConnectViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;

                // Add host to history
                AddHostToHistory(instance.Host);

                // Create new remote desktop session info
                Models.RemoteDesktop.RemoteDesktopSessionInfo remoteDesktopSessionInfo = new Models.RemoteDesktop.RemoteDesktopSessionInfo
                {
                    Hostname = instance.Host
                };

                if (instance.UseCredentials)
                {
                    remoteDesktopSessionInfo.CustomCredentials = true;

                    if (instance.CustomCredentials)
                    {
                        remoteDesktopSessionInfo.Username = instance.Username;
                        remoteDesktopSessionInfo.Password = instance.Password;
                    }
                    else
                    {
                        CredentialInfo credentialInfo = CredentialManager.GetCredentialByID((int)instance.CredentialID);

                        remoteDesktopSessionInfo.Username = credentialInfo.Username;
                        remoteDesktopSessionInfo.Password = credentialInfo.Password;
                    }
                }

                Connect(remoteDesktopSessionInfo);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;
            })
            {
                Host = host
            };

            customDialog.Content = new RemoteDesktopSessionConnectDialog
            {
                DataContext = remoteDesktopSessionConnectViewModel
            };

            ConfigurationManager.Current.FixAirspace = true;
            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        private async void ConnectSession()
        {
            Models.RemoteDesktop.RemoteDesktopSessionInfo sessionInfo = new Models.RemoteDesktop.RemoteDesktopSessionInfo
            {
                Hostname = SelectedSession.Host
            };

            if (SelectedSession.CredentialID != null) // Credentials need to be unlocked first
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

                        if (CredentialManager.Load(instance.Password))
                        {
                            CredentialInfo credentialInfo = CredentialManager.GetCredentialByID((int)SelectedSession.CredentialID);

                            if (credentialInfo == null)
                            {
                                await dialogCoordinator.ShowMessageAsync(this, LocalizationManager.GetStringByKey("String_Header_CredentialNotFound"), LocalizationManager.GetStringByKey("String_CredentialNotFoundMessage"), MessageDialogStyle.Affirmative, AppearanceManager.MetroDialog);

                                return;
                            }

                            sessionInfo.CustomCredentials = true;
                            sessionInfo.Username = credentialInfo.Username;
                            sessionInfo.Password = credentialInfo.Password;

                            Connect(sessionInfo, SelectedSession.Name);
                        }
                        else
                        {
                            await dialogCoordinator.ShowMessageAsync(this, LocalizationManager.GetStringByKey("String_Header_WrongPassword"), LocalizationManager.GetStringByKey("String_WrongPasswordDecryptionFailed"), MessageDialogStyle.Affirmative, AppearanceManager.MetroDialog);
                        }
                    }, instance =>
                    {
                        dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                    });

                    customDialog.Content = new CredentialsMasterPasswordDialog
                    {
                        DataContext = credentialsMasterPasswordViewModel
                    };

                    await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
                }
                else // Connect already unlocked
                {
                    CredentialInfo credentialInfo = CredentialManager.GetCredentialByID((int)SelectedSession.CredentialID);

                    if (credentialInfo == null)
                    {
                        await dialogCoordinator.ShowMessageAsync(this, LocalizationManager.GetStringByKey("String_Header_CredentialNotFound"), LocalizationManager.GetStringByKey("String_CredentialNotFoundMessage"), MessageDialogStyle.Affirmative, AppearanceManager.MetroDialog);

                        return;
                    }

                    sessionInfo.CustomCredentials = true;
                    sessionInfo.Username = credentialInfo.Username;
                    sessionInfo.Password = credentialInfo.Password;

                    Connect(sessionInfo, SelectedSession.Name);
                }
            }
            else // Connect without credentials
            {
                Connect(sessionInfo, SelectedSession.Name);
            }
        }

        private async void ConnectSessionAs()
        {
            CustomDialog customDialog = new CustomDialog()
            {
                Title = LocalizationManager.GetStringByKey("String_Header_ConnectAs")
            };

            RemoteDesktopSessionConnectViewModel connectRemoteDesktopSessionViewModel = new RemoteDesktopSessionConnectViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;

                Models.RemoteDesktop.RemoteDesktopSessionInfo remoteDesktopSessionInfo = new Models.RemoteDesktop.RemoteDesktopSessionInfo
                {
                    Hostname = instance.Host
                };

                if (instance.UseCredentials)
                {
                    remoteDesktopSessionInfo.CustomCredentials = true;

                    if (instance.CustomCredentials)
                    {
                        remoteDesktopSessionInfo.Username = instance.Username;
                        remoteDesktopSessionInfo.Password = instance.Password;
                    }
                    else
                    {
                        CredentialInfo credentialInfo = CredentialManager.GetCredentialByID((int)instance.CredentialID);

                        remoteDesktopSessionInfo.Username = credentialInfo.Username;
                        remoteDesktopSessionInfo.Password = credentialInfo.Password;
                    }
                }

                Connect(remoteDesktopSessionInfo, instance.Name);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;
            }, true)
            {
                // Set name, hostname
                Name = SelectedSession.Name,
                Host = SelectedSession.Host,

                // Request credentials
                UseCredentials = true
            };

            customDialog.Content = new RemoteDesktopSessionConnectDialog
            {
                DataContext = connectRemoteDesktopSessionViewModel
            };

            ConfigurationManager.Current.FixAirspace = true;
            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        private void Connect(Models.RemoteDesktop.RemoteDesktopSessionInfo sessionInfo, string Header = null)
        {
            // Add global settings...
            sessionInfo.AdjustScreenAutomatically = SettingsManager.Current.RemoteDesktop_AdjustScreenAutomatically;
            sessionInfo.UseCurrentViewSize = SettingsManager.Current.RemoteDesktop_UseCurrentViewSize;

            if (SettingsManager.Current.RemoteDesktop_UseCustomScreenSize)
            {
                sessionInfo.DesktopWidth = SettingsManager.Current.RemoteDesktop_CustomScreenWidth;
                sessionInfo.DesktopHeight = SettingsManager.Current.RemoteDesktop_CustomScreenHeight;
            }
            else
            {
                sessionInfo.DesktopWidth = SettingsManager.Current.RemoteDesktop_ScreenWidth;
                sessionInfo.DesktopHeight = SettingsManager.Current.RemoteDesktop_ScreenHeight;
            }

            sessionInfo.ColorDepth = SettingsManager.Current.RemoteDesktop_ColorDepth;
            sessionInfo.Port = SettingsManager.Current.RemoteDesktop_Port;
            sessionInfo.EnableCredSspSupport = SettingsManager.Current.RemoteDesktop_EnableCredSspSupport;
            sessionInfo.AuthenticationLevel = SettingsManager.Current.RemoteDesktop_AuthenticationLevel;
            sessionInfo.RedirectClipboard = SettingsManager.Current.RemoteDesktop_RedirectClipboard;
            sessionInfo.RedirectDevices = SettingsManager.Current.RemoteDesktop_RedirectDevices;
            sessionInfo.RedirectDrives = SettingsManager.Current.RemoteDesktop_RedirectDrives;
            sessionInfo.RedirectPorts = SettingsManager.Current.RemoteDesktop_RedirectPorts;
            sessionInfo.RedirectSmartCards = SettingsManager.Current.RemoteDesktop_RedirectSmartCards;

            TabItems.Add(new DragablzTabItem(Header ?? sessionInfo.Hostname, new RemoteDesktopControl(sessionInfo)));
            SelectedTabIndex = TabItems.Count - 1;
        }

        public void AddTab(string host)
        {
            ConnectNewSession(host);
        }

        private void RemoteDesktopSession_Search(object sender, FilterEventArgs e)
        {
            if (string.IsNullOrEmpty(Search))
            {
                e.Accepted = true;
                return;
            }

            RemoteDesktopSessionInfo info = e.Item as RemoteDesktopSessionInfo;

            string search = Search.Trim();

            if (info.Host.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                e.Accepted = true;
            else
                e.Accepted = false;
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
        #endregion
    }
}