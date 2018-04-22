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

namespace NETworkManager.ViewModels
{
    public class SNMPViewModel : ViewModelBase
    {
        #region Variables
        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        Stopwatch stopwatch = new Stopwatch();

        private bool _isLoading = true;

        private string _host;
        public string Host
        {
            get { return _host; }
            set
            {
                if (value == _host)
                    return;

                _host = value;
                OnPropertyChanged();
            }
        }

        private ICollectionView _hostHistoryView;
        public ICollectionView HostHistoryView
        {
            get { return _hostHistoryView; }
        }

        public List<SNMPVersion> Versions { get; set; }

        private SNMPVersion _version;
        public SNMPVersion Version
        {
            get { return _version; }
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
            get { return _mode; }
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
            get { return _oid; }
            set
            {
                if (value == _oid)
                    return;

                _oid = value;
                OnPropertyChanged();
            }
        }

        private ICollectionView _oidHistoryView;
        public ICollectionView OIDHistoryView
        {
            get { return _oidHistoryView; }
        }

        public List<SNMPv3Security> Securitys { get; set; }

        private SNMPv3Security _security;
        public SNMPv3Security Security
        {
            get { return _security; }
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
            get { return _community; }
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
            get { return _username; }
            set
            {
                if (value == _username)
                    return;

                _username = value;
                OnPropertyChanged();
            }
        }

        public List<SNMPv3AuthenticationProvider> AuthenticationProviders { get; set; }

        private SNMPv3AuthenticationProvider _authenticationProvider;
        public SNMPv3AuthenticationProvider AuthenticationProvider
        {
            get { return _authenticationProvider; }
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
            get { return _auth; }
            set
            {
                if (value == _auth)
                    return;

                _auth = value;
                OnPropertyChanged();
            }
        }

        public List<SNMPv3PrivacyProvider> PrivacyProviders { get; set; }

        private SNMPv3PrivacyProvider _privacyProvider;
        public SNMPv3PrivacyProvider PrivacyProvider
        {
            get { return _privacyProvider; }
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
            get { return _priv; }
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
            get { return _data; }
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
            get { return _isWorking; }
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
            get { return _queryResult; }
            set
            {
                if (value == _queryResult)
                    return;

                _queryResult = value;
            }
        }

        private ICollectionView _queryResultView;
        public ICollectionView QueryResultView
        {
            get { return _queryResultView; }
        }

        private SNMPReceivedInfo _selectedQueryResult;
        public SNMPReceivedInfo SelectedQueryResult
        {
            get { return _selectedQueryResult; }
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
            get { return _displayStatusMessage; }
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
            get { return _statusMessage; }
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
            get { return _startTime; }
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
            get { return _duration; }
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
            get { return _endTime; }
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
            get { return _responses; }
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
            get { return _expandStatistics; }
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
        #endregion

        #region Contructor, load settings
        public SNMPViewModel()
        {
            // Set collection view
            _hostHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.SNMP_HostHistory);
            _oidHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.SNMP_OIDHistory);

            // Result view
            _queryResultView = CollectionViewSource.GetDefaultView(QueryResult);
            _queryResultView.SortDescriptions.Add(new SortDescription(nameof(SNMPReceivedInfo.OID), ListSortDirection.Ascending));

            // Versions (v1, v2c, v3)
            Versions = Enum.GetValues(typeof(SNMPVersion)).Cast<SNMPVersion>().ToList();

            // Modes
            Modes = new List<SNMPMode>() { SNMPMode.Get, SNMPMode.Walk, SNMPMode.Set };

            // Security
            Securitys = new List<SNMPv3Security>() { SNMPv3Security.noAuthNoPriv, SNMPv3Security.authNoPriv, SNMPv3Security.authPriv };

            // Auth / Priv
            AuthenticationProviders = new List<SNMPv3AuthenticationProvider>() { SNMPv3AuthenticationProvider.MD5, SNMPv3AuthenticationProvider.SHA1 };
            PrivacyProviders = new List<SNMPv3PrivacyProvider>() { SNMPv3PrivacyProvider.DES, SNMPv3PrivacyProvider.AES };

            LoadSettings();

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
            stopwatch.Start();
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            dispatcherTimer.Start();
            EndTime = null;

            QueryResult.Clear();
            Responses = 0;

            // Try to parse the string into an IP-Address
            IPAddress.TryParse(Host, out IPAddress ipAddress);

            try
            {
                // Try to resolve the hostname
                if (ipAddress == null)
                {
                    IPHostEntry ipHostEntrys = await Dns.GetHostEntryAsync(Host);

                    foreach (IPAddress ipAddr in ipHostEntrys.AddressList)
                    {
                        if (ipAddr.AddressFamily == AddressFamily.InterNetwork && SettingsManager.Current.SNMP_ResolveHostnamePreferIPv4)
                        {
                            ipAddress = ipAddr;
                            continue;
                        }
                        else if (ipAddr.AddressFamily == AddressFamily.InterNetworkV6 && !SettingsManager.Current.SNMP_ResolveHostnamePreferIPv4)
                        {
                            ipAddress = ipAddr;
                            continue;
                        }
                    }

                    // Fallback --> If we could not resolve our prefered ip protocol for the hostname
                    if (ipAddress == null)
                    {
                        foreach (IPAddress ipAddr in ipHostEntrys.AddressList)
                        {
                            ipAddress = ipAddr;
                            continue;
                        }
                    }
                }
            }
            catch (SocketException) // This will catch DNS resolve errors
            {
                Finished();

                StatusMessage = string.Format(LocalizationManager.GetStringByKey("String_CouldNotResolveHostnameFor"), Host);
                DisplayStatusMessage = true;

                return;
            }

            // SNMP...
            SNMPOptions snmpOptions = new SNMPOptions()
            {
                Port = SettingsManager.Current.SNMP_Port,
                Timeout = SettingsManager.Current.SNMP_Timeout
            };

            SNMP snmp = new SNMP();

            snmp.Received += Snmp_Received;
            snmp.Timeout += Snmp_Timeout;
            snmp.Error += Snmp_Error;
            snmp.UserHasCanceled += Snmp_UserHasCanceled;
            snmp.Complete += Snmp_Complete;

            switch (Mode)
            {
                case SNMPMode.Get:
                    if (Version != SNMPVersion.v3)
                        snmp.Getv1v2cAsync(Version, ipAddress, Community, OID, snmpOptions);
                    else
                        snmp.Getv3Async(ipAddress, OID, Security, Username, AuthenticationProvider, Auth, PrivacyProvider, Priv, snmpOptions);

                    break;
                case SNMPMode.Walk:
                    if (Version != SNMPVersion.v3)
                        snmp.Walkv1v2cAsync(Version, ipAddress, Community, OID, SettingsManager.Current.SNMP_WalkMode, snmpOptions);
                    else
                        snmp.Walkv3Async(ipAddress, OID, Security, Username, AuthenticationProvider, Auth, PrivacyProvider, Priv, SettingsManager.Current.SNMP_WalkMode, snmpOptions);

                    break;
                case SNMPMode.Set:
                    if (Version != SNMPVersion.v3)
                        snmp.Setv1v2cAsync(Version, ipAddress, Community, OID, Data, snmpOptions);
                    else
                        snmp.Setv3Async(ipAddress, OID, Security, Username, AuthenticationProvider, Auth, PrivacyProvider, Priv, Data, snmpOptions);
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
            stopwatch.Stop();
            dispatcherTimer.Stop();

            Duration = stopwatch.Elapsed;
            EndTime = DateTime.Now;

            stopwatch.Reset();
        }

        private void AddHostToHistory(string host)
        {
            // Create the new list
            List<string> list = ListHelper.Modify(SettingsManager.Current.SNMP_HostHistory.ToList(), host, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.SNMP_HostHistory.Clear();
            OnPropertyChanged(nameof(Host)); // Raise property changed again, after the collection has been cleared

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.SNMP_HostHistory.Add(x));
        }

        private void AddOIDToHistory(string oid)
        {
            // Create the new list
            List<string> list = ListHelper.Modify(SettingsManager.Current.SNMP_OIDHistory.ToList(), oid, SettingsManager.Current.General_HistoryListEntries);

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
            SNMPReceivedInfo snmpReceivedInfo = SNMPReceivedInfo.Parse(e);

            Application.Current.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                QueryResult.Add(snmpReceivedInfo);
            }));

            Responses++;
        }

        private void Snmp_Timeout(object sender, EventArgs e)
        {
            StatusMessage = LocalizationManager.GetStringByKey("String_TimeoutOnSNMPQuery");
            DisplayStatusMessage = true;

            Finished();
        }

        private void Snmp_Error(object sender, EventArgs e)
        {
            if (Mode == SNMPMode.Set)
                StatusMessage = LocalizationManager.GetStringByKey("String_ErrorInResponseCheckIfYouHaveWritePermissions");
            else
                StatusMessage = LocalizationManager.GetStringByKey("String_ErrorInResponse");

            DisplayStatusMessage = true;

            Finished();
        }

        private void Snmp_UserHasCanceled(object sender, EventArgs e)
        {
            StatusMessage = LocalizationManager.GetStringByKey("String_CanceledByUser");
            DisplayStatusMessage = true;

            Finished();
        }

        private void Snmp_Complete(object sender, EventArgs e)
        {
            if (Mode == SNMPMode.Set)
            {
                StatusMessage = LocalizationManager.GetStringByKey("String_DataHasBeenUpdated");
                DisplayStatusMessage = true;
            }

            Finished();
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            Duration = stopwatch.Elapsed;
        }
        #endregion
    }
}