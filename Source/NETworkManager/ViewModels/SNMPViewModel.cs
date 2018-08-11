using NETworkManager.Utilities;
using NETworkManager.Models.Network;
using NETworkManager.Models.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using static NETworkManager.Models.Network.SNMP;
using NETworkManager.Controls;
using Dragablz;

namespace NETworkManager.ViewModels
{
    public class SNMPViewModel : ViewModelBase
    {
        #region Variables
        private readonly DispatcherTimer _dispatcherTimer = new DispatcherTimer();
        private readonly Stopwatch _stopwatch = new Stopwatch();

        private readonly int _tabId;

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

        private string _community;
        public string Community
        {
            get => _community;
            set
            {
                if (value == _community)
                    return;

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

        private string _auth;
        public string Auth
        {
            get => _auth;
            set
            {
                if (value == _auth)
                    return;

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

        private string _priv;
        public string Priv
        {
            get => _priv;
            set
            {
                if (value == _priv)
                    return;

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

        private bool _isWorking;
        public bool IsWorking
        {
            get => _isWorking;
            set
            {
                if (value == _isWorking)
                    return;

                _isWorking = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<SNMPReceivedInfo> _queryResult = new ObservableCollection<SNMPReceivedInfo>();
        public ObservableCollection<SNMPReceivedInfo> QueryResult
        {
            get => _queryResult;
            set
            {
                if (Equals(value, _queryResult))
                    return;

                _queryResult = value;
            }
        }

        public ICollectionView QueryResultView { get; }

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

        private bool _displayStatusMessage;
        public bool DisplayStatusMessage
        {
            get => _displayStatusMessage;
            set
            {
                if (value == _displayStatusMessage)
                    return;

                _displayStatusMessage = value;
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

        private DateTime? _startTime;
        public DateTime? StartTime
        {
            get => _startTime;
            set
            {
                if (value == _startTime)
                    return;

                _startTime = value;
                OnPropertyChanged();
            }
        }

        private TimeSpan _duration;
        public TimeSpan Duration
        {
            get => _duration;
            set
            {
                if (value == _duration)
                    return;

                _duration = value;
                OnPropertyChanged();
            }
        }

        private DateTime? _endTime;
        public DateTime? EndTime
        {
            get => _endTime;
            set
            {
                if (value == _endTime)
                    return;

                _endTime = value;
                OnPropertyChanged();
            }
        }

        private int _responses;
        public int Responses
        {
            get => _responses;
            set
            {
                if (value == _responses)
                    return;

                _responses = value;
                OnPropertyChanged();
            }
        }

        private bool _expandStatistics;
        public bool ExpandStatistics
        {
            get => _expandStatistics;
            set
            {
                if (value == _expandStatistics)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.SNMP_ExpandStatistics = value;

                _expandStatistics = value;
                OnPropertyChanged();
            }
        }

        public bool ShowStatistics => SettingsManager.Current.SNMP_ShowStatistics;

        #endregion

        #region Contructor, load settings
        public SNMPViewModel(int tabId, string host)
        {
            _isLoading = true;

            _tabId = tabId;
            Host = host;

            // Set collection view
            HostHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.SNMP_HostHistory);
            OIDHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.SNMP_OIDHistory);

            // Result view
            QueryResultView = CollectionViewSource.GetDefaultView(QueryResult);
            QueryResultView.SortDescriptions.Add(new SortDescription(nameof(SNMPReceivedInfo.OID), ListSortDirection.Ascending));

            // Versions (v1, v2c, v3)
            Versions = Enum.GetValues(typeof(SNMPVersion)).Cast<SNMPVersion>().ToList();

            // Modes
            Modes = new List<SNMPMode>() { SNMPMode.Get, SNMPMode.Walk, SNMPMode.Set };

            // Security
            Securitys = new List<SNMPV3Security>() { SNMPV3Security.NoAuthNoPriv, SNMPV3Security.AuthNoPriv, SNMPV3Security.AuthPriv };

            // Auth / Priv
            AuthenticationProviders = new List<SNMPV3AuthenticationProvider>() { SNMPV3AuthenticationProvider.MD5, SNMPV3AuthenticationProvider.SHA1 };
            PrivacyProviders = new List<SNMPV3PrivacyProvider>() { SNMPV3PrivacyProvider.DES, SNMPV3PrivacyProvider.AES };

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
            ExpandStatistics = SettingsManager.Current.SNMP_ExpandStatistics;
        }
        #endregion

        #region ICommands & Actions
        public ICommand WorkCommand
        {
            get { return new RelayCommand(p => WorkAction()); }
        }

        private void WorkAction()
        {
            Work();
        }

        public ICommand CopySelectedOIDCommand
        {
            get { return new RelayCommand(p => CopySelectedOIDAction()); }
        }

        private void CopySelectedOIDAction()
        {
            Clipboard.SetText(SelectedQueryResult.OID);
        }

        public ICommand CopySelectedDataCommand
        {
            get { return new RelayCommand(p => CopySelectedDataAction()); }
        }

        private void CopySelectedDataAction()
        {
            Clipboard.SetText(SelectedQueryResult.Data);
        }

        #endregion

        #region Methods
        private async void Work()
        {
            DisplayStatusMessage = false;
            IsWorking = true;

            // Measure time
            StartTime = DateTime.Now;
            _stopwatch.Start();
            _dispatcherTimer.Tick += DispatcherTimer_Tick;
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            _dispatcherTimer.Start();
            EndTime = null;

            QueryResult.Clear();
            Responses = 0;

            // Change the tab title (not nice, but it works)
            var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

            if (window != null)
            {
                foreach (var tabablzControl in VisualTreeHelper.FindVisualChildren<TabablzControl>(window))
                {
                    tabablzControl.Items.OfType<DragablzTabItem>().First(x => x.Id == _tabId).Header = Host;
                }
            }

            // Try to parse the string into an IP-Address
            IPAddress.TryParse(Host, out var ipAddress);

            try
            {
                // Try to resolve the hostname
                if (ipAddress == null)
                {
                    var ipHostEntrys = await Dns.GetHostEntryAsync(Host);

                    foreach (var ipAddr in ipHostEntrys.AddressList)
                    {
                        switch (ipAddr.AddressFamily)
                        {
                            case AddressFamily.InterNetwork when SettingsManager.Current.SNMP_ResolveHostnamePreferIPv4:
                                ipAddress = ipAddr;
                                break;
                            case AddressFamily.InterNetworkV6 when !SettingsManager.Current.SNMP_ResolveHostnamePreferIPv4:
                                ipAddress = ipAddr;
                                break;
                        }
                    }

                    // Fallback --> If we could not resolve our prefered ip protocol for the hostname
                    if (ipAddress == null)
                    {
                        foreach (var ipAddr in ipHostEntrys.AddressList)
                        {
                            ipAddress = ipAddr;
                            break;
                        }
                    }
                }
            }
            catch (SocketException) // This will catch DNS resolve errors
            {
                Finished();

                StatusMessage = string.Format(Resources.Localization.Strings.CouldNotResolveHostnameFor, Host);
                DisplayStatusMessage = true;

                return;
            }

            // SNMP...
            var snmpOptions = new SNMPOptions()
            {
                Port = SettingsManager.Current.SNMP_Port,
                Timeout = SettingsManager.Current.SNMP_Timeout
            };

            var snmp = new SNMP();

            snmp.Received += Snmp_Received;
            snmp.Timeout += Snmp_Timeout;
            snmp.Error += Snmp_Error;
            snmp.UserHasCanceled += Snmp_UserHasCanceled;
            snmp.Complete += Snmp_Complete;

            switch (Mode)
            {
                case SNMPMode.Get:
                    if (Version != SNMPVersion.V3)
                        snmp.GetV1V2CAsync(Version, ipAddress, Community, OID, snmpOptions);
                    else
                        snmp.Getv3Async(ipAddress, OID, Security, Username, AuthenticationProvider, Auth, PrivacyProvider, Priv, snmpOptions);
                    break;
                case SNMPMode.Walk:
                    if (Version != SNMPVersion.V3)
                        snmp.WalkV1V2CAsync(Version, ipAddress, Community, OID, SettingsManager.Current.SNMP_WalkMode, snmpOptions);
                    else
                        snmp.WalkV3Async(ipAddress, OID, Security, Username, AuthenticationProvider, Auth, PrivacyProvider, Priv, SettingsManager.Current.SNMP_WalkMode, snmpOptions);
                    break;
                case SNMPMode.Set:
                    if (Version != SNMPVersion.V3)
                        snmp.SetV1V2CAsync(Version, ipAddress, Community, OID, Data, snmpOptions);
                    else
                        snmp.SetV3Async(ipAddress, OID, Security, Username, AuthenticationProvider, Auth, PrivacyProvider, Priv, Data, snmpOptions);
                    break;
            }

            // Add to history...
            AddHostToHistory(Host);
            AddOIDToHistory(OID);
        }

        private void Finished()
        {
            IsWorking = false;

            // Stop timer and stopwatch
            _stopwatch.Stop();
            _dispatcherTimer.Stop();

            Duration = _stopwatch.Elapsed;
            EndTime = DateTime.Now;

            _stopwatch.Reset();
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
                lock (QueryResult)
                    QueryResult.Add(snmpReceivedInfo);
            }));

            Responses++;
        }

        private void Snmp_Timeout(object sender, EventArgs e)
        {
            StatusMessage = Resources.Localization.Strings.TimeoutOnSNMPQuery;
            DisplayStatusMessage = true;

            Finished();
        }

        private void Snmp_Error(object sender, EventArgs e)
        {
            StatusMessage = Mode == SNMPMode.Set ? Resources.Localization.Strings.ErrorInResponseCheckIfYouHaveWritePermissions : Resources.Localization.Strings.ErrorInResponse;

            DisplayStatusMessage = true;

            Finished();
        }

        private void Snmp_UserHasCanceled(object sender, EventArgs e)
        {
            StatusMessage = Resources.Localization.Strings.CanceledByUserMessage;
            DisplayStatusMessage = true;

            Finished();
        }

        private void Snmp_Complete(object sender, EventArgs e)
        {
            if (Mode == SNMPMode.Set)
            {
                StatusMessage = Resources.Localization.Strings.DataHasBeenUpdated;
                DisplayStatusMessage = true;
            }

            Finished();
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            Duration = _stopwatch.Elapsed;
        }

        private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SettingsInfo.SNMP_ShowStatistics))
                OnPropertyChanged(nameof(ShowStatistics));
        }
        #endregion
    }
}