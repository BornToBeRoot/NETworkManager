using System.Collections.ObjectModel;
using NETworkManager.Controls;
using Dragablz;
using System.Windows.Input;
using System.Windows;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels.Dialogs;
using NETworkManager.Models.Settings;
using System.Collections.Generic;
using NETworkManager.Helpers;
using System.Linq;
using NETworkManager.Views.Dialogs;
using System.ComponentModel;
using System.Windows.Data;
using System;
using System.Diagnostics;

namespace NETworkManager.ViewModels.Applications
{
    public class PuTTYViewModel : ViewModelBase
    {
        #region Variables
        private IDialogCoordinator dialogCoordinator;

        public IInterTabClient InterTabClient { get; private set; }
        public ObservableCollection<DragablzPuTTYTabItem> TabItems { get; private set; }

        private const string tagIdentifier = "tag=";

        private bool _isLoading = true;

        private int _tabId = 0;

        private bool _isPuTTYConfigured;
        public bool IsPuTTYConfigured
        {
            get { return _isPuTTYConfigured; }
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
        ICollectionView _puTTYSessions;
        public ICollectionView PuTTYSessions
        {
            get { return _puTTYSessions; }
        }

        private PuTTYSessionInfo _selectedSession = new PuTTYSessionInfo();
        public PuTTYSessionInfo SelectedSession
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
                    SettingsManager.Current.PuTTY_ExpandSessionView = value;

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

                PuTTYSessions.Refresh();

                OnPropertyChanged();
            }
        }
        #endregion
        #endregion

        #region Constructor
        public PuTTYViewModel(IDialogCoordinator instance)
        {
            dialogCoordinator = instance;

            // Check if putty is available...
            IsPuTTYConfigured = true;

            InterTabClient = new DragablzPuTTYInterTabClient();
            TabItems = new ObservableCollection<DragablzPuTTYTabItem>();


            // Load sessions
            if (PuTTYSessionManager.Sessions == null)
                PuTTYSessionManager.Load();

            _puTTYSessions = CollectionViewSource.GetDefaultView(PuTTYSessionManager.Sessions);
            _puTTYSessions.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
            _puTTYSessions.SortDescriptions.Add(new SortDescription("Group", ListSortDirection.Ascending));
            _puTTYSessions.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            _puTTYSessions.Filter = o =>
            {
                if (string.IsNullOrEmpty(Search))
                    return true;

                PuTTYSessionInfo info = o as PuTTYSessionInfo;

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

        private void LoadSettings()
        {
            ExpandSessionView = SettingsManager.Current.PuTTY_ExpandSessionView;
        }
        #endregion

        #region ICommand & Actions
        public ICommand ConnectNewSessionCommand
        {
            get { return new RelayCommand(p => ConnectNewSessionAction()); }
        }

        private async void ConnectNewSessionAction()
        {
            CustomDialog customDialog = new CustomDialog()
            {
                Title = Application.Current.Resources["String_Header_Connect"] as string
            };

            PuTTYSessionConnectViewModel puTTYSessionConnectViewModel = new PuTTYSessionConnectViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;

                // Add host to history
                AddHostToHistory(instance.Host);

                // Create new remote desktop session info
                Models.PuTTY.PuTTYSessionInfo puTTYSessionInfo = new Models.PuTTY.PuTTYSessionInfo
                {
                    Hostname = instance.Host
                };

                /*
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
                }*/

                ConnectSession(puTTYSessionInfo);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;
            });

            customDialog.Content = new PuTTYSessionConnectDialog
            {
                DataContext = puTTYSessionConnectViewModel
            };

            ConfigurationManager.Current.FixAirspace = true;
            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand AddSessionCommand
        {
            get { return new RelayCommand(p => AddSessionAction()); }
        }

        private async void AddSessionAction()
        {
            CustomDialog customDialog = new CustomDialog()
            {
                Title = Application.Current.Resources["String_Header_AddSession"] as string
            };

            PuTTYSessionViewModel puTTYSessionViewModel = new PuTTYSessionViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;

                PuTTYSessionInfo puTTYSessionInfo = new PuTTYSessionInfo
                {
                    Name = instance.Name,
                    Host = instance.Host,
                    CredentialID = instance.CredentialID,
                    Group = instance.Group,
                    Tags = instance.Tags
                };

                PuTTYSessionManager.AddSession(puTTYSessionInfo);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;
            }, PuTTYSessionManager.GetSessionGroups());

            customDialog.Content = new PuTTYSessionDialog
            {
                DataContext = puTTYSessionViewModel
            };

            ConfigurationManager.Current.FixAirspace = true;
            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand ConnectSessionCommand
        {
            get { return new RelayCommand(p => ConnectSessionAction()); }
        }

        private async void ConnectSessionAction()
        {
            Models.PuTTY.PuTTYSessionInfo sessionInfo = new Models.PuTTY.PuTTYSessionInfo
            {
                Hostname = SelectedSession.Host
            };
            
            if (SelectedSession.CredentialID != null) // Credentials need to be unlocked first
            {
                if (!CredentialManager.Loaded)
                {
                    CustomDialog customDialog = new CustomDialog()
                    {
                        Title = Application.Current.Resources["String_Header_MasterPassword"] as string
                    };

                    CredentialsMasterPasswordViewModel credentialsMasterPasswordViewModel = new CredentialsMasterPasswordViewModel(async instance =>
                    {
                        await dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                        if (CredentialManager.Load(instance.Password))
                        {
                            CredentialInfo credentialInfo = CredentialManager.GetCredentialByID((int)SelectedSession.CredentialID);

                            if (credentialInfo == null)
                            {
                                await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_Header_CredentialNotFound"] as string, Application.Current.Resources["String_CredentialNotFoundMessage"] as string, MessageDialogStyle.Affirmative, AppearanceManager.MetroDialog);

                                return;
                            }

                            /*
                            sessionInfo.CustomCredentials = true;
                            sessionInfo.Username = credentialInfo.Username;
                            sessionInfo.Password = credentialInfo.Password;
                            */

                            ConnectSession(sessionInfo, SelectedSession.Name);
                        }
                        else
                        {
                            await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_Header_WrongPassword"] as string, Application.Current.Resources["String_WrongPasswordDecryptionFailed"] as string, MessageDialogStyle.Affirmative, AppearanceManager.MetroDialog);
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
                        await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_Header_CredentialNotFound"] as string, Application.Current.Resources["String_CredentialNotFoundMessage"] as string, MessageDialogStyle.Affirmative, AppearanceManager.MetroDialog);

                        return;
                    }

                    /*
                    sessionInfo.CustomCredentials = true;
                    sessionInfo.Username = credentialInfo.Username;
                    sessionInfo.Password = credentialInfo.Password;
                    */

                    ConnectSession(sessionInfo, SelectedSession.Name);
                }
            }
            else // Connect without credentials
            {
                ConnectSession(sessionInfo, SelectedSession.Name);
            }
        }

        public ICommand ConnectSessionAsCommand
        {
            get { return new RelayCommand(p => ConnectSessionAsAction()); }
        }

        private async void ConnectSessionAsAction()
        {
            CustomDialog customDialog = new CustomDialog()
            {
                Title = Application.Current.Resources["String_Header_ConnectAs"] as string
            };

            PuTTYSessionConnectViewModel puTTYSessionConnectViewModel = new PuTTYSessionConnectViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;

                Models.PuTTY.PuTTYSessionInfo puTTYSessionInfo = new Models.PuTTY.PuTTYSessionInfo
                {
                    Hostname = instance.Host
                };

                /*
                if (instance.UseCredentials)
                {
                    puTTYSessionInfo.CustomCredentials = true;

                    if (instance.CustomCredentials)
                    {
                        puTTYSessionInfo.Username = instance.Username;
                        puTTYSessionInfo.Password = instance.Password;
                    }
                    else
                    {
                        CredentialInfo credentialInfo = CredentialManager.GetCredentialByID((int)instance.CredentialID);

                        puTTYSessionInfo.Username = credentialInfo.Username;
                        puTTYSessionInfo.Password = credentialInfo.Password;
                    }
                }
                */

                ConnectSession(puTTYSessionInfo, instance.Name);
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

            customDialog.Content = new PuTTYSessionConnectDialog
            {
                DataContext = puTTYSessionConnectViewModel
            };

            ConfigurationManager.Current.FixAirspace = true;
            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand ConnectSessionExternalCommand
        {
            get { return new RelayCommand(p => ConnectSessionExternalAction()); }
        }

        private void ConnectSessionExternalAction()
        {
            //Process.Start("mstsc.exe", string.Format("/V:{0}", SelectedSession.Host));
        }

        public ICommand EditSessionCommand
        {
            get { return new RelayCommand(p => EditSessionAction()); }
        }

        private async void EditSessionAction()
        {
            CustomDialog customDialog = new CustomDialog()
            {
                Title = Application.Current.Resources["String_Header_EditSession"] as string
            };

            PuTTYSessionViewModel puTTYSessionViewModel = new PuTTYSessionViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;

                PuTTYSessionManager.RemoveSession(SelectedSession);

                PuTTYSessionInfo puTTYSessionInfo = new PuTTYSessionInfo
                {
                    Name = instance.Name,
                    Host = instance.Host,
                    CredentialID = instance.CredentialID,
                    Group = instance.Group,
                    Tags = instance.Tags
                };

                PuTTYSessionManager.AddSession(puTTYSessionInfo);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;
            }, PuTTYSessionManager.GetSessionGroups(), SelectedSession);

            customDialog.Content = new PuTTYSessionDialog
            {
                DataContext = puTTYSessionViewModel
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
                Title = Application.Current.Resources["String_Header_CopySession"] as string
            };

            PuTTYSessionViewModel puTTYSessionViewModel = new PuTTYSessionViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;

                PuTTYSessionInfo puTTYSessionInfo = new PuTTYSessionInfo
                {
                    Name = instance.Name,
                    Host = instance.Host,
                    Group = instance.Group,
                    Tags = instance.Tags
                };

                PuTTYSessionManager.AddSession(puTTYSessionInfo);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;
            }, PuTTYSessionManager.GetSessionGroups(), SelectedSession);

            customDialog.Content = new PuTTYSessionDialog
            {
                DataContext = puTTYSessionViewModel
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
                Title = Application.Current.Resources["String_Header_DeleteSession"] as string
            };

            ConfirmRemoveViewModel confirmRemoveViewModel = new ConfirmRemoveViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;

                PuTTYSessionManager.RemoveSession(SelectedSession);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;
            }, Application.Current.Resources["String_DeleteSessionMessage"] as string);

            customDialog.Content = new ConfirmRemoveDialog
            {
                DataContext = confirmRemoveViewModel
            };

            ConfigurationManager.Current.FixAirspace = true;
            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }
        #endregion

        #region Methods
        private void ConnectSession(Models.PuTTY.PuTTYSessionInfo sessionInfo, string Header = null)
        {
            // Add PuTTY path here...

            TabItems.Add(new DragablzPuTTYTabItem(Header ?? sessionInfo.Hostname, new PuTTYControl(sessionInfo)));
            SelectedTabIndex = TabItems.Count - 1;
        }

        // Modify history list
        private void AddHostToHistory(string host)
        {
            // Create the new list
            List<string> list = ListHelper.Modify(SettingsManager.Current.PuTTY_HostHistory.ToList(), host, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.RemoteDesktop_HostHistory.Clear();

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.RemoteDesktop_HostHistory.Add(x));
        }
        #endregion
    }
}