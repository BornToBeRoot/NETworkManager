using System.Collections.ObjectModel;
using NETworkManager.Controls;
using Dragablz;
using MahApps.Metro.Controls.Dialogs;
using System.Windows.Input;
using NETworkManager.Views.Dialogs;
using System.Windows;
using NETworkManager.ViewModels.Dialogs;
using NETworkManager.Models.Settings;
using System.ComponentModel;
using System.Windows.Data;
using System;
using System.Linq;
using System.Collections.Specialized;

namespace NETworkManager.ViewModels.Applications
{
    public class RemoteDesktopViewModel : ViewModelBase
    {
        #region Variables
        private IDialogCoordinator dialogCoordinator;

        public IInterTabClient InterTabClient { get; private set; }
        public ObservableCollection<DragablzRemoteDesktopTabItem> TabItems { get; private set; }

        private const string tagIdentifier = "tag=";

        private bool _isLoading = true;

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
        public RemoteDesktopViewModel(IDialogCoordinator instance)
        {
            dialogCoordinator = instance;

            InterTabClient = new DragablzMainInterTabClient();
            TabItems = new ObservableCollection<DragablzRemoteDesktopTabItem>();

            TabItems.CollectionChanged += TabItems_CollectionChanged;

            // Load sessions
            if (RemoteDesktopSessionManager.Sessions == null)
                RemoteDesktopSessionManager.Load();

            _remoteDesktopSessions = CollectionViewSource.GetDefaultView(RemoteDesktopSessionManager.Sessions);
            _remoteDesktopSessions.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
            _remoteDesktopSessions.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            _remoteDesktopSessions.Filter = o =>
            {
                if (string.IsNullOrEmpty(Search))
                    return true;

                RemoteDesktopSessionInfo info = o as RemoteDesktopSessionInfo;

                string search = Search.Trim();

                if (search.StartsWith(tagIdentifier, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(info.Tags))
                        return false;
                    else
                        return info.Tags.Replace(" ", "").Split(';').Any(str => search.Substring(tagIdentifier.Length, search.Length - tagIdentifier.Length).IndexOf(str, StringComparison.OrdinalIgnoreCase) > -1);
                }
                else
                {
                    return info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1;
                }
            };

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            ExpandSessionView = SettingsManager.Current.RemoteDesktop_ExpandSessionView;
        }
        #endregion

        #region Methods
        private void ConnectSession(Models.RemoteDesktop.RemoteDesktopSessionInfo sessionInfo, string Header = null)
        {
            // Add global settings...
            sessionInfo.AdjustScreenAutomatically = SettingsManager.Current.RemoteDesktop_AdjustScreenAutomatically;
            sessionInfo.DesktopWidth = SettingsManager.Current.RemoteDesktop_DesktopWidth;
            sessionInfo.DesktopHeight = SettingsManager.Current.RemoteDesktop_DesktopHeight;
            sessionInfo.ColorDepth = SettingsManager.Current.RemoteDesktop_ColorDepth;
            sessionInfo.RedirectClipboard = SettingsManager.Current.RemoteDesktop_RedirectClipboard;
            sessionInfo.RedirectDevices = SettingsManager.Current.RemoteDesktop_RedirectDevices;
            sessionInfo.RedirectDrives = SettingsManager.Current.RemoteDesktop_RedirectDrives;
            sessionInfo.RedirectPorts = SettingsManager.Current.RemoteDesktop_RedirectPorts;
            sessionInfo.RedirectSmartCards = SettingsManager.Current.RemoteDesktop_RedirectSmartCards;

            TabItems.Add(new DragablzRemoteDesktopTabItem(Header ?? sessionInfo.Hostname, new RemoteDesktopControl(sessionInfo)));
            SelectedTabIndex = TabItems.Count - 1;
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

            if (info.Hostname.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                e.Accepted = true;
            else
                e.Accepted = false;
        }
        #endregion

        #region ICommand & Actions
        public ICommand ConnectRemoteDesktopSessionCommand
        {
            get { return new RelayCommand(p => ConnectRemoteDesktopSessionAction()); }
        }

        private async void ConnectRemoteDesktopSessionAction()
        {
            CustomDialog customDialog = new CustomDialog()
            {
                Title = Application.Current.Resources["String_Header_Connect"] as string
            };

            RemoteDesktopSessionConnectViewModel connectRemoteDesktopSessionViewModel = new RemoteDesktopSessionConnectViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;

                Models.RemoteDesktop.RemoteDesktopSessionInfo remoteDesktopSessionInfo = new Models.RemoteDesktop.RemoteDesktopSessionInfo
                {
                    Hostname = instance.Hostname,
                };

                ConnectSession(remoteDesktopSessionInfo);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;
            });

            customDialog.Content = new RemoteDesktopSessionConnectDialog
            {
                DataContext = connectRemoteDesktopSessionViewModel
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

            RemoteDesktopSessionViewModel remoteDesktopSessionViewModel = new RemoteDesktopSessionViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;

                RemoteDesktopSessionInfo remoteDesktopSessionInfo = new RemoteDesktopSessionInfo
                {
                    Name = instance.Name,
                    Hostname = instance.Hostname,
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
            Models.RemoteDesktop.RemoteDesktopSessionInfo remoteDesktopSessionInfo = new Models.RemoteDesktop.RemoteDesktopSessionInfo
            {
                Hostname = SelectedSession.Hostname
            };

            ConnectSession(remoteDesktopSessionInfo, SelectedSession.Name);
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

            RemoteDesktopSessionViewModel remoteDesktopSessionViewModel = new RemoteDesktopSessionViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;

                RemoteDesktopSessionManager.RemoveSession(SelectedSession);

                RemoteDesktopSessionInfo remoteDesktopSessionInfo = new RemoteDesktopSessionInfo
                {
                    Name = instance.Name,
                    Hostname = instance.Hostname,
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
                Title = Application.Current.Resources["String_Header_CopySession"] as string
            };

            RemoteDesktopSessionViewModel remoteDesktopSessionViewModel = new RemoteDesktopSessionViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;

                RemoteDesktopSessionInfo remoteDesktopSessionInfo = new RemoteDesktopSessionInfo
                {
                    Name = instance.Name,
                    Hostname = instance.Hostname,
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
                Title = Application.Current.Resources["String_Header_DeleteSession"] as string
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
            }, Application.Current.Resources["String_DeleteSessionMessage"] as string);

            customDialog.Content = new ConfirmRemoveDialog
            {
                DataContext = confirmRemoveViewModel
            };

            ConfigurationManager.Current.FixAirspace = true;
            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }
        #endregion

        #region Events
        private void TabItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                DragablzRemoteDesktopTabItem item = (DragablzRemoteDesktopTabItem)e.OldItems[0];
                item.Dispose();
            }
        }
        #endregion
    }
}