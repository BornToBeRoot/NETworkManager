using System.Collections.ObjectModel;
using NETworkManager.Controls;
using Dragablz;
using System.Windows.Input;
using System.Windows;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models.Settings;
using System.Collections.Generic;
using System.Linq;
using NETworkManager.Views;
using System.ComponentModel;
using System.Windows.Data;
using System;
using System.IO;
using NETworkManager.Utilities;
using System.Diagnostics;
using NETworkManager.Models.PuTTY;

namespace NETworkManager.ViewModels
{
    public class PuTTYViewModel : ViewModelBase
    {
        #region Variables
        private IDialogCoordinator dialogCoordinator;

        public IInterTabClient InterTabClient { get; private set; }
        public ObservableCollection<DragablzPuTTYTabItem> TabItems { get; private set; }

        private const string tagIdentifier = "tag=";

        private bool _isLoading = true;

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

        private Models.Settings.PuTTYSessionInfo _selectedSession = new Models.Settings.PuTTYSessionInfo();
        public Models.Settings.PuTTYSessionInfo SelectedSession
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
            CheckIfPuTTYConfigured();

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

                Models.Settings.PuTTYSessionInfo info = o as Models.Settings.PuTTYSessionInfo;

                string search = Search.Trim();

                // Search by: Tag
                if (search.StartsWith(tagIdentifier, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(info.Tags))
                        return false;
                    else
                        return info.Tags.Replace(" ", "").Split(';').Any(str => search.Substring(tagIdentifier.Length, search.Length - tagIdentifier.Length).IndexOf(str, StringComparison.OrdinalIgnoreCase) > -1);
                }
                else // Search by: Name, (Hostname || SerialLine)
                {
                    return info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1 || info.HostOrSerialLine.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1;
                }
            };

            LoadSettings();

            SettingsManager.Current.PropertyChanged += Current_PropertyChanged;
        }

        private void Current_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SettingsInfo.PuTTY_PuTTYLocation))
                CheckIfPuTTYConfigured();
        }

        private void LoadSettings()
        {
            ExpandSessionView = SettingsManager.Current.PuTTY_ExpandSessionView;
        }
        #endregion

        #region ICommand & Actions
        public ItemActionCallback CloseItemCommand
        {
            get { return CloseItemAction; }
        }

        private void CloseItemAction(ItemActionCallbackArgs<TabablzControl> args)
        {
            ((args.DragablzItem.Content as DragablzPuTTYTabItem).View as PuTTYControl).OnClose();
        }

        public ICommand ConnectNewSessionCommand
        {
            get { return new RelayCommand(p => ConnectNewSessionAction()); }
        }

        private async void ConnectNewSessionAction()
        {
            CustomDialog customDialog = new CustomDialog()
            {
                Title = LocalizationManager.GetStringByKey("String_Header_Connect")
            };

            PuTTYSessionConnectViewModel puTTYSessionConnectViewModel = new PuTTYSessionConnectViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;

                // Add host to history
                AddHostToHistory(instance.Host);
                AddSerialLineToHistory(instance.SerialLine);
                AddPortToHistory(instance.Port.ToString());
                AddBaudToHistory(instance.Baud.ToString());
                AddUsernameToHistory(instance.Username);
                AddProfileToHistory(instance.Profile);

                // Create session info
                Models.PuTTY.PuTTYSessionInfo puTTYSessionInfo = new Models.PuTTY.PuTTYSessionInfo
                {
                    HostOrSerialLine = instance.ConnectionMode == PuTTY.ConnectionMode.Serial ? instance.SerialLine : instance.Host,
                    Mode = instance.ConnectionMode,
                    PortOrBaud = instance.ConnectionMode == PuTTY.ConnectionMode.Serial ? instance.Baud : instance.Port,
                    Username = instance.Username,
                    Profile = instance.Profile,
                    AdditionalCommandLine = instance.AdditionalCommandLine
                };

                // Connect
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
                Title = LocalizationManager.GetStringByKey("String_Header_AddSession")
            };

            PuTTYSessionViewModel puTTYSessionViewModel = new PuTTYSessionViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;

                PuTTYSessionManager.AddSession(new Models.Settings.PuTTYSessionInfo(instance.Name, instance.ConnectionMode, instance.ConnectionMode == PuTTY.ConnectionMode.Serial ? instance.SerialLine : instance.Host, instance.ConnectionMode == PuTTY.ConnectionMode.Serial ? instance.Baud : instance.Port, instance.Username, instance.Profile, instance.AdditionalCommandLine, instance.Group, instance.Tags));
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

        private void ConnectSessionAction()
        {
            // Connect
            ConnectSession(Models.PuTTY.PuTTYSessionInfo.Parse(SelectedSession), SelectedSession.Name);
        }

        public ICommand ConnectSessionExternalCommand
        {
            get { return new RelayCommand(p => ConnectSessionExternalAction()); }
        }

        private void ConnectSessionExternalAction()
        {
            ProcessStartInfo info = new ProcessStartInfo
            {
                FileName = SettingsManager.Current.PuTTY_PuTTYLocation,
                Arguments = PuTTY.BuildCommandLine(Models.PuTTY.PuTTYSessionInfo.Parse(SelectedSession))
            };

            Process.Start(info);
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

            PuTTYSessionViewModel puTTYSessionViewModel = new PuTTYSessionViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;

                PuTTYSessionManager.RemoveSession(SelectedSession);

                PuTTYSessionManager.AddSession(new Models.Settings.PuTTYSessionInfo(instance.Name, instance.ConnectionMode, instance.ConnectionMode == PuTTY.ConnectionMode.Serial ? instance.SerialLine : instance.Host, instance.ConnectionMode == Models.PuTTY.PuTTY.ConnectionMode.Serial ? instance.Baud : instance.Port, instance.Username, instance.Profile, instance.AdditionalCommandLine, instance.Group, instance.Tags));
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
                Title = LocalizationManager.GetStringByKey("String_Header_CopySession")
            };

            PuTTYSessionViewModel puTTYSessionViewModel = new PuTTYSessionViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;

                PuTTYSessionManager.AddSession(new Models.Settings.PuTTYSessionInfo(instance.Name, instance.ConnectionMode, instance.ConnectionMode == Models.PuTTY.PuTTY.ConnectionMode.Serial ? instance.SerialLine : instance.Host, instance.ConnectionMode == Models.PuTTY.PuTTY.ConnectionMode.Serial ? instance.Baud : instance.Port, instance.Username, instance.Profile, instance.AdditionalCommandLine, instance.Group, instance.Tags));
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
                Title = LocalizationManager.GetStringByKey("String_Header_DeleteSession")
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

                PuTTYSessionManager.RenameGroup(instance.OldGroup, instance.Group);

                _puTTYSessions.Refresh();
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, group.ToString());

            customDialog.Content = new GroupDialog
            {
                DataContext = editGroupViewModel
            };

            ConfigurationManager.Current.FixAirspace = true;
            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }
        #endregion

        #region Methods
        private void CheckIfPuTTYConfigured()
        {
            if (!string.IsNullOrEmpty(SettingsManager.Current.PuTTY_PuTTYLocation))
                IsPuTTYConfigured = File.Exists(SettingsManager.Current.PuTTY_PuTTYLocation);
            else
                IsPuTTYConfigured = false;
        }

        private void ConnectSession(Models.PuTTY.PuTTYSessionInfo sessionInfo, string Header = null)
        {
            // Add PuTTY path here...
            sessionInfo.PuTTYLocation = SettingsManager.Current.PuTTY_PuTTYLocation;

            TabItems.Add(new DragablzPuTTYTabItem(Header ?? sessionInfo.HostOrSerialLine, new PuTTYControl(sessionInfo)));
            SelectedTabIndex = TabItems.Count - 1;
        }

        // Modify history list
        private void AddHostToHistory(string host)
        {
            // Create the new list
            List<string> list = ListHelper.Modify(SettingsManager.Current.PuTTY_HostHistory.ToList(), host, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.PuTTY_HostHistory.Clear();

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.PuTTY_HostHistory.Add(x));
        }

        private void AddSerialLineToHistory(string serialLine)
        {
            // Create the new list
            List<string> list = ListHelper.Modify(SettingsManager.Current.PuTTY_SerialLineHistory.ToList(), serialLine, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.PuTTY_SerialLineHistory.Clear();

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.PuTTY_SerialLineHistory.Add(x));
        }

        private void AddPortToHistory(string host)
        {
            // Create the new list
            List<string> list = ListHelper.Modify(SettingsManager.Current.PuTTY_PortHistory.ToList(), host, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.PuTTY_PortHistory.Clear();

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.PuTTY_PortHistory.Add(x));
        }

        private void AddBaudToHistory(string host)
        {
            // Create the new list
            List<string> list = ListHelper.Modify(SettingsManager.Current.PuTTY_BaudHistory.ToList(), host, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.PuTTY_BaudHistory.Clear();

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.PuTTY_BaudHistory.Add(x));
        }

        private void AddUsernameToHistory(string host)
        {
            // Create the new list
            List<string> list = ListHelper.Modify(SettingsManager.Current.PuTTY_UsernameHistory.ToList(), host, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.PuTTY_UsernameHistory.Clear();

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.PuTTY_UsernameHistory.Add(x));
        }

        private void AddProfileToHistory(string host)
        {
            // Create the new list
            List<string> list = ListHelper.Modify(SettingsManager.Current.PuTTY_ProfileHistory.ToList(), host, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.PuTTY_ProfileHistory.Clear();

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.PuTTY_ProfileHistory.Add(x));
        }
        #endregion
    }
}