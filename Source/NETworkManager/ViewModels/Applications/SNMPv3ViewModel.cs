using NETworkManager.Helpers;
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

namespace NETworkManager.ViewModels.Applications
{
    public class SNMPv3ViewModel : ViewModelBase
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
                    SettingsManager.Current.SNMP_v3_Security = value;

                _security = value;
                OnPropertyChanged();
            }
        }

        private bool _walk;
        public bool Walk
        {
            get { return _walk; }
            set
            {
                if (value == _walk)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.SNMP_v3_Walk = value;

                _walk = value;
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
                    SettingsManager.Current.SNMP_v3_AuthenticationProvider = value;

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
                    SettingsManager.Current.SNMP_v3_PrivacyProvider = value;

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

        private bool _isQueryRunning;
        public bool IsQueryRunning
        {
            get { return _isQueryRunning; }
            set
            {
                if (value == _isQueryRunning)
                    return;

                _isQueryRunning = value;
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
                    SettingsManager.Current.SNMP_v1v2c_ExpandStatistics = value;

                _expandStatistics = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Contructor, load settings
        public SNMPv3ViewModel()
        {
            _hostHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.SNMP_v3_HostHistory);
            _oidHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.SNMP_v3_OIDHistory);

            // Result view
            _queryResultView = CollectionViewSource.GetDefaultView(QueryResult);
            _queryResultView.SortDescriptions.Add(new SortDescription(nameof(SNMPReceivedInfo.OID), ListSortDirection.Ascending));

            // Version v1 and v2c (default v2c)
            Securitys = new List<SNMPv3Security>() { SNMPv3Security.noAuthNoPriv, SNMPv3Security.authNoPriv, SNMPv3Security.authPriv };

            // Auth / Priv
            AuthenticationProviders = new List<SNMPv3AuthenticationProvider>() { SNMPv3AuthenticationProvider.MD5, SNMPv3AuthenticationProvider.SHA1 };
            PrivacyProviders = new List<SNMPv3PrivacyProvider>() { SNMPv3PrivacyProvider.DES, SNMPv3PrivacyProvider.AES };

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            Security = Securitys.FirstOrDefault(x => x == SettingsManager.Current.SNMP_v3_Security);
            AuthenticationProvider = AuthenticationProviders.FirstOrDefault(x => x == SettingsManager.Current.SNMP_v3_AuthenticationProvider);
            PrivacyProvider = PrivacyProviders.FirstOrDefault(x => x == SettingsManager.Current.SNMP_v3_PrivacyProvider);
            Walk = SettingsManager.Current.SNMP_v3_Walk;
            ExpandStatistics = SettingsManager.Current.SNMP_v3_ExpandStatistics;
        }
        #endregion

        #region ICommands & Actions
        public ICommand QueryCommand
        {
            get { return new RelayCommand(p => QueryAction()); }
        }

        private void QueryAction()
        {
            Query();
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
        private async void Query()
        {
            DisplayStatusMessage = false;
            IsQueryRunning = true;

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
                QueryFinished();

                StatusMessage = string.Format(Application.Current.Resources["String_CouldNotResolveHostnameFor"] as string, Host);
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

            if (Walk)
                snmp.Walkv3Async(ipAddress, OID, Security, Username, AuthenticationProvider, Auth, PrivacyProvider, Priv, snmpOptions, SettingsManager.Current.SNMP_WalkMode);
            else
                snmp.Getv3Async(ipAddress, OID, Security, Username, AuthenticationProvider, Auth, PrivacyProvider, Priv, snmpOptions);

            // Add to history...
            AddHostToHistory(Host);
            AddOIDToHistory(OID);
        }

        private void QueryFinished()
        {
            IsQueryRunning = false;

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
            List<string> list = HistoryListHelper.Modify(SettingsManager.Current.SNMP_v3_HostHistory.ToList(), host, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.SNMP_v3_HostHistory.Clear();
            OnPropertyChanged(nameof(Host)); // Raise property changed again, after the collection has been cleared

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.SNMP_v3_HostHistory.Add(x));
        }

        private void AddOIDToHistory(string oid)
        {
            // Create the new list
            List<string> list = HistoryListHelper.Modify(SettingsManager.Current.SNMP_v3_OIDHistory.ToList(), oid, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.SNMP_v3_OIDHistory.Clear();
            OnPropertyChanged(nameof(OID)); // Raise property changed again, after the collection has been cleared

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.SNMP_v3_OIDHistory.Add(x));
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
            StatusMessage = Application.Current.Resources["String_TimeoutOnSNMPQuery"] as string;
            DisplayStatusMessage = true;

            QueryFinished();
        }

        private void Snmp_Error(object sender, EventArgs e)
        {
            StatusMessage = Application.Current.Resources["String_ErrorInResponse"] as string;
            DisplayStatusMessage = true;

            QueryFinished();
        }

        private void Snmp_UserHasCanceled(object sender, EventArgs e)
        {
            StatusMessage = Application.Current.Resources["String_CanceledByUser"] as string;
            DisplayStatusMessage = true;

            QueryFinished();
        }

        private void Snmp_Complete(object sender, EventArgs e)
        {
            QueryFinished();
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            Duration = stopwatch.Elapsed;
        }
        #endregion
    }
}