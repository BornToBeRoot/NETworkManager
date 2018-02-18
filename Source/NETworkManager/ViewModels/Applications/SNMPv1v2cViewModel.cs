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
    public class SNMPv1v2cViewModel : ViewModelBase
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
                    SettingsManager.Current.SNMP_v1v2c_Version = value;

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
                    SettingsManager.Current.SNMP_v1v2c_Mode = value;

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
        public SNMPv1v2cViewModel()
        {
            // Set collection view
            _hostHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.SNMP_v1v2c_HostHistory);
            _oidHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.SNMP_v1v2c_OIDHistory);

            // Result view
            _queryResultView = CollectionViewSource.GetDefaultView(QueryResult);
            _queryResultView.SortDescriptions.Add(new SortDescription(nameof(SNMPReceivedInfo.OID), ListSortDirection.Ascending));

            // Version v1 and v2c (default v2c)
            Versions = new List<SNMPVersion>() { SNMPVersion.v1, SNMPVersion.v2c };

            // Modes
            Modes = Enum.GetValues(typeof(SNMPMode)).Cast<SNMPMode>().ToList();

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            Version = Versions.FirstOrDefault(x => x == SettingsManager.Current.SNMP_v1v2c_Version);
            Mode = Modes.FirstOrDefault(x => x ==  SettingsManager.Current.SNMP_v1v2c_Mode);
            ExpandStatistics = SettingsManager.Current.SNMP_v1v2c_ExpandStatistics;
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

            switch(Mode)
            {
                case SNMPMode.Get:
                    snmp.Getv1v2cAsync(Version, ipAddress, Community, OID, snmpOptions);
                    break;
                case SNMPMode.Walk:
                    snmp.Walkv1v2cAsync(Version, ipAddress, Community, OID, snmpOptions, SettingsManager.Current.SNMP_WalkMode);
                    break;
            }

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
            List<string> list = ListHelper.Modify(SettingsManager.Current.SNMP_v1v2c_HostHistory.ToList(), host, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.SNMP_v1v2c_HostHistory.Clear();
            OnPropertyChanged(nameof(Host)); // Raise property changed again, after the collection has been cleared

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.SNMP_v1v2c_HostHistory.Add(x));
        }
        
        private void AddOIDToHistory(string oid)
        {
            // Create the new list
            List<string> list = ListHelper.Modify(SettingsManager.Current.SNMP_v1v2c_OIDHistory.ToList(), oid, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.SNMP_v1v2c_OIDHistory.Clear();
            OnPropertyChanged(nameof(OID)); // Raise property changed again, after the collection has been cleared

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.SNMP_v1v2c_OIDHistory.Add(x));
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