using NETworkManager.Utilities;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using static NETworkManager.Models.Network.SNMP;
using NETworkManager.Controls;
using Dragablz;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models.Export;
using NETworkManager.Views;
using System.Security;
using Amazon.EC2.Model;

namespace NETworkManager.ViewModels
{
    public class SNMPViewModel : ViewModelBase
    {
        #region Variables
        private readonly IDialogCoordinator _dialogCoordinator;

        public readonly int TabId;

        private readonly bool _isLoading;

        private string _host;
        public string Host
        {
            get => _host;
            set
            {
                if (value == _host)
                    return;

                _host = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView HostHistoryView { get; }

        public List<SNMPVersion> Versions { get; set; }

        private SNMPVersion _version;
        public SNMPVersion Version
        {
            get => _version;
            set
            {
                if (value == _version)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.SNMP_Version = value;

                _version = value;
                OnPropertyChanged();
            }
        }

        public List<SNMPMode> Modes { get; set; }

        private SNMPMode _mode;
        public SNMPMode Mode
        {
            get => _mode;
            set
            {
                if (value == _mode)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.SNMP_Mode = value;

                _mode = value;
                OnPropertyChanged();
            }
        }

        private string _oid;
        public string OID
        {
            get => _oid;
            set
            {
                if (value == _oid)
                    return;

                _oid = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView OIDHistoryView { get; }

        public List<SNMPV3Security> Securitys { get; set; }

        private SNMPV3Security _security;
        public SNMPV3Security Security
        {
            get => _security;
            set
            {
                if (value == _security)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.SNMP_Security = value;

                _security = value;
                OnPropertyChanged();
            }
        }

        private bool _isCommunityEmpty = true; // Initial it's empty
        public bool IsCommunityEmpty
        {
            get => _isCommunityEmpty;
            set
            {
                if (value == _isCommunityEmpty)
                    return;

                _isCommunityEmpty = value;
                OnPropertyChanged();
            }
        }

        private SecureString _community;
        public SecureString Community
        {
            get => _community;
            set
            {
                if (value == _community)
                    return;

                // Validate the community string
                if (value == null)
                    IsCommunityEmpty = true;
                else
                    IsCommunityEmpty = string.IsNullOrEmpty(SecureStringHelper.ConvertToString(value));

                _community = value;
                OnPropertyChanged();
            }
        }

        private string _username;
        public string Username
        {
            get => _username;
            set
            {
                if (value == _username)
                    return;

                _username = value;
                OnPropertyChanged();
            }
        }

        public List<SNMPV3AuthenticationProvider> AuthenticationProviders { get; set; }

        private SNMPV3AuthenticationProvider _authenticationProvider;
        public SNMPV3AuthenticationProvider AuthenticationProvider
        {
            get => _authenticationProvider;
            set
            {
                if (value == _authenticationProvider)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.SNMP_AuthenticationProvider = value;

                _authenticationProvider = value;
                OnPropertyChanged();
            }
        }

        private bool _isAuthEmpty = true; // Initial it's empty
        public bool IsAuthEmpty
        {
            get => _isAuthEmpty;
            set
            {
                if (value == _isAuthEmpty)
                    return;

                _isAuthEmpty = value;
                OnPropertyChanged();
            }
        }

        private SecureString _auth;
        public SecureString Auth
        {
            get => _auth;
            set
            {
                if (value == _auth)
                    return;

                // Validate the auth string
                if (value == null)
                    IsAuthEmpty = true;
                else
                    IsAuthEmpty = string.IsNullOrEmpty(SecureStringHelper.ConvertToString(value));

                _auth = value;
                OnPropertyChanged();
            }
        }

        public List<SNMPV3PrivacyProvider> PrivacyProviders { get; set; }

        private SNMPV3PrivacyProvider _privacyProvider;
        public SNMPV3PrivacyProvider PrivacyProvider
        {
            get => _privacyProvider;
            set
            {
                if (value == _privacyProvider)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.SNMP_PrivacyProvider = value;

                _privacyProvider = value;
                OnPropertyChanged();
            }
        }

        private bool _isPrivEmpty = true; // Initial it's empty
        public bool IsPrivEmpty
        {
            get => _isPrivEmpty;
            set
            {
                if (value == _isPrivEmpty)
                    return;

                _isPrivEmpty = value;
                OnPropertyChanged();
            }
        }

        private SecureString _priv;
        public SecureString Priv
        {
            get => _priv;
            set
            {
                if (value == _priv)
                    return;

                // Validate the auth string
                if (value == null)
                    IsPrivEmpty = true;
                else
                    IsPrivEmpty = string.IsNullOrEmpty(SecureStringHelper.ConvertToString(value));

                _priv = value;
                OnPropertyChanged();
            }
        }

        private string _data = string.Empty;
        public string Data
        {
            get => _data;
            set
            {
                if (value == _data)
                    return;

                _data = value;
                OnPropertyChanged();
            }
        }

        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                if (value == _isRunning)
                    return;

                _isRunning = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<SNMPReceivedInfo> _queryResults = new ObservableCollection<SNMPReceivedInfo>();
        public ObservableCollection<SNMPReceivedInfo> QueryResults
        {
            get => _queryResults;
            set
            {
                if (Equals(value, _queryResults))
                    return;

                _queryResults = value;
            }
        }

        public ICollectionView QueryResultsView { get; }

        private SNMPReceivedInfo _selectedQueryResult;
        public SNMPReceivedInfo SelectedQueryResult
        {
            get => _selectedQueryResult;
            set
            {
                if (value == _selectedQueryResult)
                    return;

                _selectedQueryResult = value;
                OnPropertyChanged();
            }
        }

        private IList _selectedQueryResults = new ArrayList();
        public IList SelectedQueryResults
        {
            get => _selectedQueryResults;
            set
            {
                if (Equals(value, _selectedQueryResults))
                    return;

                _selectedQueryResults = value;
                OnPropertyChanged();
            }
        }


        private bool _isStatusMessageDisplayed;
        public bool IsStatusMessageDisplayed
        {
            get => _isStatusMessageDisplayed;
            set
            {
                if (value == _isStatusMessageDisplayed)
                    return;

                _isStatusMessageDisplayed = value;
                OnPropertyChanged();
            }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (value == _statusMessage)
                    return;

                _statusMessage = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Contructor, load settings
        public SNMPViewModel(IDialogCoordinator instance, int tabId, string host)
        {
            _isLoading = true;

            _dialogCoordinator = instance;

            TabId = tabId;
            Host = host;

            // Set collection view
            HostHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.SNMP_HostHistory);
            OIDHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.SNMP_OIDHistory);

            // Result view
            QueryResultsView = CollectionViewSource.GetDefaultView(QueryResults);
            QueryResultsView.SortDescriptions.Add(new SortDescription(nameof(SNMPReceivedInfo.OID), ListSortDirection.Ascending));

            // Versions (v1, v2c, v3)
            Versions = System.Enum.GetValues(typeof(SNMPVersion)).Cast<SNMPVersion>().ToList();

            // Modes
            Modes = new List<SNMPMode> { SNMPMode.Get, SNMPMode.Walk, SNMPMode.Set };

            // Security
            Securitys = new List<SNMPV3Security> { SNMPV3Security.NoAuthNoPriv, SNMPV3Security.AuthNoPriv, SNMPV3Security.AuthPriv };

            // Auth / Priv
            AuthenticationProviders = new List<SNMPV3AuthenticationProvider> { SNMPV3AuthenticationProvider.MD5, SNMPV3AuthenticationProvider.SHA1 };
            PrivacyProviders = new List<SNMPV3PrivacyProvider> { SNMPV3PrivacyProvider.DES, SNMPV3PrivacyProvider.AES };

            LoadSettings();

            // Detect if settings have changed...
            SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;

            _isLoading = false;
        }

        private void LoadSettings()
        {
            Version = Versions.FirstOrDefault(x => x == SettingsManager.Current.SNMP_Version);
            Mode = Modes.FirstOrDefault(x => x == SettingsManager.Current.SNMP_Mode);
            Security = Securitys.FirstOrDefault(x => x == SettingsManager.Current.SNMP_Security);
            AuthenticationProvider = AuthenticationProviders.FirstOrDefault(x => x == SettingsManager.Current.SNMP_AuthenticationProvider);
            PrivacyProvider = PrivacyProviders.FirstOrDefault(x => x == SettingsManager.Current.SNMP_PrivacyProvider);
        }
        #endregion

        #region ICommands & Actions
        public ICommand WorkCommand => new RelayCommand(p => WorkAction(), Work_CanExecute);

        private bool Work_CanExecute(object paramter) => Application.Current.MainWindow != null && !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen;

        private void WorkAction()
        {
            Work();
        }

        public ICommand CopySelectedOIDCommand => new RelayCommand(p => CopySelectedOIDAction());

        private void CopySelectedOIDAction()
        {
            ClipboardHelper.SetClipboard(SelectedQueryResult.OID);
        }

        public ICommand CopySelectedDataCommand => new RelayCommand(p => CopySelectedDataAction());

        private void CopySelectedDataAction()
        {
            ClipboardHelper.SetClipboard(SelectedQueryResult.Data);
        }

        public ICommand ExportCommand => new RelayCommand(p => ExportAction());

        private async void ExportAction()
        {
            var customDialog = new CustomDialog
            {
                Title = Localization.Resources.Strings.Export
            };

            var exportViewModel = new ExportViewModel(async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                try
                {
                    ExportManager.Export(instance.FilePath, instance.FileType, instance.ExportAll ? QueryResults : new ObservableCollection<SNMPReceivedInfo>(SelectedQueryResults.Cast<SNMPReceivedInfo>().ToArray()));
                }
                catch (Exception ex)
                {
                    var settings = AppearanceManager.MetroDialog;
                    settings.AffirmativeButtonText = Localization.Resources.Strings.OK;

                    await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.Error, Localization.Resources.Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine + Environment.NewLine + ex.Message, MessageDialogStyle.Affirmative, settings);
                }

                SettingsManager.Current.SNMP_ExportFileType = instance.FileType;
                SettingsManager.Current.SNMP_ExportFilePath = instance.FilePath;
            }, instance => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); }, new ExportManager.ExportFileType[] { ExportManager.ExportFileType.CSV, ExportManager.ExportFileType.XML, ExportManager.ExportFileType.JSON }, true, SettingsManager.Current.SNMP_ExportFileType, SettingsManager.Current.SNMP_ExportFilePath);

            customDialog.Content = new ExportDialog
            {
                DataContext = exportViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }
        #endregion

        #region Methods
        private async void Work()
        {
            IsStatusMessageDisplayed = false;
            IsRunning = true;

            QueryResults.Clear();

            // Change the tab title (not nice, but it works)
            var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

            if (window != null)
            {
                foreach (var tabablzControl in VisualTreeHelper.FindVisualChildren<TabablzControl>(window))
                {
                    tabablzControl.Items.OfType<DragablzTabItem>().First(x => x.Id == TabId).Header = Host;
                }
            }

            // Try to parse the string into an IP-Address
            if (!IPAddress.TryParse(Host, out var ipAddress))
            {
                var dnsResult = await DNSHelper.ResolveAorAaaaAsync(Host, SettingsManager.Current.SNMP_ResolveHostnamePreferIPv4);

                if (!dnsResult.HasError)
                {
                    ipAddress = dnsResult.Value;
                }
                else
                {
                    StatusMessage = string.Format(Localization.Resources.Strings.CouldNotResolveIPAddressFor, Host) + Environment.NewLine + dnsResult.ErrorMessage;
                    IsStatusMessageDisplayed = true;
                    IsRunning = false;
                    return;
                }
            }

            // SNMP...
            var snmp = new SNMP
            {
                Port = SettingsManager.Current.SNMP_Port,
                Timeout = SettingsManager.Current.SNMP_Timeout
            };

            snmp.Received += Snmp_Received;
            snmp.TimeoutReached += Snmp_TimeoutReached;
            snmp.Error += Snmp_Error;
            snmp.UserHasCanceled += Snmp_UserHasCanceled;
            snmp.Complete += Snmp_Complete;

            switch (Mode)
            {
                case SNMPMode.Get:
                    if (Version != SNMPVersion.V3)
                        snmp.GetV1V2CAsync(Version, ipAddress, Community, OID);
                    else
                        snmp.Getv3Async(ipAddress, OID, Security, Username, AuthenticationProvider, Auth, PrivacyProvider, Priv);
                    break;
                case SNMPMode.Walk:
                    if (Version != SNMPVersion.V3)
                        snmp.WalkV1V2CAsync(Version, ipAddress, Community, OID, SettingsManager.Current.SNMP_WalkMode);
                    else
                        snmp.WalkV3Async(ipAddress, OID, Security, Username, AuthenticationProvider, Auth, PrivacyProvider, Priv, SettingsManager.Current.SNMP_WalkMode);
                    break;
                case SNMPMode.Set:
                    if (Version != SNMPVersion.V3)
                        snmp.SetV1V2CAsync(Version, ipAddress, Community, OID, Data);
                    else
                        snmp.SetV3Async(ipAddress, OID, Security, Username, AuthenticationProvider, Auth, PrivacyProvider, Priv, Data);
                    break;
            }

            // Add to history...
            AddHostToHistory(Host);
            AddOIDToHistory(OID);
        }

        public void OnClose()
        {

        }

        private void AddHostToHistory(string host)
        {
            // Create the new list
            var list = ListHelper.Modify(SettingsManager.Current.SNMP_HostHistory.ToList(), host, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.SNMP_HostHistory.Clear();
            OnPropertyChanged(nameof(Host)); // Raise property changed again, after the collection has been cleared

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.SNMP_HostHistory.Add(x));
        }

        private void AddOIDToHistory(string oid)
        {
            // Create the new list
            var list = ListHelper.Modify(SettingsManager.Current.SNMP_OIDHistory.ToList(), oid, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.SNMP_OIDHistory.Clear();
            OnPropertyChanged(nameof(OID)); // Raise property changed again, after the collection has been cleared

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.SNMP_OIDHistory.Add(x));
        }
        #endregion

        #region Events
        private void Snmp_Received(object sender, SNMPReceivedArgs e)
        {
            var snmpReceivedInfo = SNMPReceivedInfo.Parse(e);

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
            {
                //lock (QueryResults)
                QueryResults.Add(snmpReceivedInfo);
            }));
        }

        private void Snmp_TimeoutReached(object sender, EventArgs e)
        {
            StatusMessage = Localization.Resources.Strings.TimeoutOnSNMPQuery;
            IsStatusMessageDisplayed = true;
            IsRunning = false;
        }

        private void Snmp_Error(object sender, EventArgs e)
        {
            StatusMessage = Mode == SNMPMode.Set ? Localization.Resources.Strings.ErrorInResponseCheckIfYouHaveWritePermissions : Localization.Resources.Strings.ErrorInResponse;
            IsStatusMessageDisplayed = true;
            IsRunning = false;
        }

        private void Snmp_UserHasCanceled(object sender, EventArgs e)
        {
            StatusMessage = Localization.Resources.Strings.CanceledByUserMessage;
            IsStatusMessageDisplayed = true;
            IsRunning = false;
        }

        private void Snmp_Complete(object sender, EventArgs e)
        {
            if (Mode == SNMPMode.Set)
            {
                StatusMessage = Localization.Resources.Strings.DataHasBeenUpdated;
                IsStatusMessageDisplayed = true;
            }

            IsRunning = false;
        }

        private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

        }
        #endregion
    }
}