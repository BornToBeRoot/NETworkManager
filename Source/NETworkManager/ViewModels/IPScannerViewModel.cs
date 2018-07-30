using NETworkManager.Models.Settings;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows.Input;
using System.Windows;
using System;
using System.Threading;
using System.Collections.Generic;
using NETworkManager.Models.Network;
using System.Windows.Threading;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Data;
using System.Linq;
using NETworkManager.Utilities;
using Dragablz;
using NETworkManager.Controls;

namespace NETworkManager.ViewModels
{
    public class IPScannerViewModel : ViewModelBase
    {
        #region Variables

        private CancellationTokenSource _cancellationTokenSource;

        private readonly int _tabId;
        private bool _firstLoad = true;

        private readonly DispatcherTimer _dispatcherTimer = new DispatcherTimer();
        private readonly Stopwatch _stopwatch = new Stopwatch();

        private readonly bool _isLoading;
            
        private string _ipRange;
        public string IPRange
        {
            get => _ipRange;
            set
            {
                if (value == _ipRange)
                    return;

                _ipRange = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView IPRangeHistoryView { get; }

        private bool _isScanRunning;
        public bool IsScanRunning
        {
            get => _isScanRunning;
            set
            {
                if (value == _isScanRunning)
                    return;

                _isScanRunning = value;
                OnPropertyChanged();
            }
        }

        private bool _cancelScan;
        public bool CancelScan
        {
            get => _cancelScan;
            set
            {
                if (value == _cancelScan)
                    return;

                _cancelScan = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<IPScannerHostInfo> _ipScanResult = new ObservableCollection<IPScannerHostInfo>();
        public ObservableCollection<IPScannerHostInfo> IPScanResult
        {
            get => _ipScanResult;
            set
            {
                if (value != null && value == _ipScanResult)
                    return;

                _ipScanResult = value;
            }
        }

        public ICollectionView IPScanResultView { get; }

        private IPScannerHostInfo _selectedIPScanResult;
        public IPScannerHostInfo SelectedIPScanResult
        {
            get => _selectedIPScanResult;
            set
            {
                if (value == _selectedIPScanResult)
                    return;

                _selectedIPScanResult = value;
                OnPropertyChanged();
            }
        }

        public bool ResolveHostname => SettingsManager.Current.IPScanner_ResolveHostname;

        public bool ResolveMACAddress => SettingsManager.Current.IPScanner_ResolveMACAddress;

        private int _ipAddressesToScan;
        public int IPAddressesToScan
        {
            get => _ipAddressesToScan;
            set
            {
                if (value == _ipAddressesToScan)
                    return;

                _ipAddressesToScan = value;
                OnPropertyChanged();
            }
        }

        private int _ipAddressesScanned;
        public int IPAddressesScanned
        {
            get => _ipAddressesScanned;
            set
            {
                if (value == _ipAddressesScanned)
                    return;

                _ipAddressesScanned = value;
                OnPropertyChanged();
            }
        }

        private int _hostsFound;
        public int HostsFound
        {
            get => _hostsFound;
            set
            {
                if (value == _hostsFound)
                    return;

                _hostsFound = value;
                OnPropertyChanged();
            }
        }

        private bool _preparingScan;
        public bool PreparingScan
        {
            get => _preparingScan;
            set
            {
                if (value == _preparingScan)
                    return;

                _preparingScan = value;
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

        private bool _expandStatistics;
        public bool ExpandStatistics
        {
            get => _expandStatistics;
            set
            {
                if (value == _expandStatistics)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.IPScanner_ExpandStatistics = value;

                _expandStatistics = value;
                OnPropertyChanged();
            }
        }

        public bool ShowStatistics => SettingsManager.Current.IPScanner_ShowStatistics;

        #endregion

        #region Constructor, load settings, shutdown
        public IPScannerViewModel(int tabId, string ipRange)
        {
            _isLoading = true;

            _tabId = tabId;
            IPRange = ipRange;

            // Set collection view
            IPRangeHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.IPScanner_IPRangeHistory);

            // Result view
            IPScanResultView = CollectionViewSource.GetDefaultView(IPScanResult);
            IPScanResultView.SortDescriptions.Add(new SortDescription(nameof(IPScannerHostInfo.PingInfo) + "." + nameof(PingInfo.IPAddressInt32), ListSortDirection.Ascending));

            LoadSettings();

            // Detect if settings have changed...
            SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;

            _isLoading = false;
        }

        public void OnLoaded()
        {
            if (_firstLoad)
            {
                if (!string.IsNullOrEmpty(IPRange))
                    StartScan();

                _firstLoad = false;
            }
        }

        private void LoadSettings()
        {
            ExpandStatistics = SettingsManager.Current.IPScanner_ExpandStatistics;
        }
        #endregion

        #region ICommands & Actions
        public ICommand ScanCommand
        {
            get { return new RelayCommand(p => ScanAction()); }
        }

        private void ScanAction()
        {
            if (IsScanRunning)
                StopScan();
            else
                StartScan();
        }

        public ICommand PerformPortScanCommand
        {
            get { return new RelayCommand(p => PerformPortScanAction()); }
        }

        private void PerformPortScanAction()
        {
            EventSystem.RedirectToApplication(ApplicationViewManager.Name.PortScanner, SelectedIPScanResult.PingInfo.IPAddress.ToString());
        }

        public ICommand PerformPingCommand
        {
            get { return new RelayCommand(p => PerformPingAction()); }
        }

        private void PerformPingAction()
        {
            EventSystem.RedirectToApplication(ApplicationViewManager.Name.Ping, SelectedIPScanResult.PingInfo.IPAddress.ToString());
        }

        public ICommand PerformTracerouteCommand
        {
            get { return new RelayCommand(p => PerformTracerouteAction()); }
        }

        private void PerformTracerouteAction()
        {
            EventSystem.RedirectToApplication(ApplicationViewManager.Name.Traceroute, SelectedIPScanResult.PingInfo.IPAddress.ToString());
        }

        public ICommand PerformDNSLookupIPAddressCommand
        {
            get { return new RelayCommand(p => PerformDNSLookupIPAddressAction()); }
        }

        private void PerformDNSLookupIPAddressAction()
        {
            EventSystem.RedirectToApplication(ApplicationViewManager.Name.DNSLookup, SelectedIPScanResult.PingInfo.IPAddress.ToString());
        }

        public ICommand PerformDNSLookupHostnameCommand
        {
            get { return new RelayCommand(p => PerformDNSLookupHostnameAction()); }
        }

        private void PerformDNSLookupHostnameAction()
        {
            EventSystem.RedirectToApplication(ApplicationViewManager.Name.DNSLookup, SelectedIPScanResult.Hostname);
        }

        public ICommand ConnectRemoteDesktopCommand
        {
            get { return new RelayCommand(p => ConnectRemoteDesktopAction()); }
        }

        private void ConnectRemoteDesktopAction()
        {
            EventSystem.RedirectToApplication(ApplicationViewManager.Name.RemoteDesktop, SelectedIPScanResult.PingInfo.IPAddress.ToString());
        }

        public ICommand ConnectPuTTYCommand
        {
            get { return new RelayCommand(p => ConnectPuTTYAction()); }
        }

        private void ConnectPuTTYAction()
        {
            EventSystem.RedirectToApplication(ApplicationViewManager.Name.PuTTY, SelectedIPScanResult.PingInfo.IPAddress.ToString());
        }

        public ICommand PerformSNMPCommand
        {
            get { return new RelayCommand(p => PerformSNMPAction()); }
        }

        private void PerformSNMPAction()
        {
            EventSystem.RedirectToApplication(ApplicationViewManager.Name.SNMP, SelectedIPScanResult.PingInfo.IPAddress.ToString());
        }

        public ICommand CopySelectedIPAddressCommand
        {
            get { return new RelayCommand(p => CopySelectedIPAddressAction()); }
        }

        private void CopySelectedIPAddressAction()
        {
            Clipboard.SetText(SelectedIPScanResult.PingInfo.IPAddress.ToString());
        }

        public ICommand CopySelectedHostnameCommand
        {
            get { return new RelayCommand(p => CopySelectedHostnameAction()); }
        }

        private void CopySelectedHostnameAction()
        {
            Clipboard.SetText(SelectedIPScanResult.Hostname);
        }

        public ICommand CopySelectedMACAddressCommand
        {
            get { return new RelayCommand(p => CopySelectedMACAddressAction()); }
        }

        private void CopySelectedMACAddressAction()
        {
            Clipboard.SetText(MACAddressHelper.GetDefaultFormat(SelectedIPScanResult.MACAddress.ToString()));
        }

        public ICommand CopySelectedVendorCommand
        {
            get { return new RelayCommand(p => CopySelectedVendorAction()); }
        }

        private void CopySelectedVendorAction()
        {
            Clipboard.SetText(SelectedIPScanResult.Vendor);
        }

        public ICommand CopySelectedBytesCommand
        {
            get { return new RelayCommand(p => CopySelectedBytesAction()); }
        }

        private void CopySelectedBytesAction()
        {
            Clipboard.SetText(SelectedIPScanResult.PingInfo.Bytes.ToString());
        }

        public ICommand CopySelectedTimeCommand
        {
            get { return new RelayCommand(p => CopySelectedTimeAction()); }
        }

        private void CopySelectedTimeAction()
        {
            Clipboard.SetText(SelectedIPScanResult.PingInfo.Time.ToString());
        }

        public ICommand CopySelectedTTLCommand
        {
            get { return new RelayCommand(p => CopySelectedTTLAction()); }
        }

        private void CopySelectedTTLAction()
        {
            Clipboard.SetText(SelectedIPScanResult.PingInfo.TTL.ToString());
        }

        public ICommand CopySelectedStatusCommand
        {
            get { return new RelayCommand(p => CopySelectedStatusAction()); }
        }

        private void CopySelectedStatusAction()
        {
            Clipboard.SetText(Resources.Localization.Strings.ResourceManager.GetString("String_IPStatus_" + SelectedIPScanResult.PingInfo.Status));
        }
        #endregion

        #region Methods
        private async void StartScan()
        {
            DisplayStatusMessage = false;
            IsScanRunning = true;
            PreparingScan = true;

            // Measure the time
            StartTime = DateTime.Now;
            _stopwatch.Start();
            _dispatcherTimer.Tick += DispatcherTimer_Tick;
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            _dispatcherTimer.Start();
            EndTime = null;

            IPScanResult.Clear();
            HostsFound = 0;

            // Change the tab title (not nice, but it works)
            var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

            if (window != null)
            {
                foreach (var tabablzControl in VisualTreeHelper.FindVisualChildren<TabablzControl>(window))
                {
                    tabablzControl.Items.OfType<DragablzTabItem>().First(x => x.Id == _tabId).Header = IPRange;
                }
            }

            _cancellationTokenSource = new CancellationTokenSource();

            var ipHostOrRanges = IPRange.Replace(" ", "").Split(';');

            // Resolve hostnames
            List<string> ipRanges;

            try
            {
                ipRanges = await IPScanRangeHelper.ResolveHostnamesInIPRangeAsync(ipHostOrRanges, _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                IpScanner_UserHasCanceled(this, EventArgs.Empty);
                return;
            }
            catch (AggregateException exceptions) // DNS error (could not resolve hostname...)
            {
                IpScanner_DnsResolveFailed(exceptions);
                return;
            }

            IPAddress[] ipAddresses;

            try
            {
                // Create a list of all ip addresses
                ipAddresses = await IPScanRangeHelper.ConvertIPRangeToIPAddressesAsync(ipRanges.ToArray(), _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                IpScanner_UserHasCanceled(this, EventArgs.Empty);
                return;
            }

            IPAddressesToScan = ipAddresses.Length;
            IPAddressesScanned = 0;

            PreparingScan = false;

            // Add the range to the history
            AddIPRangeToHistory(IPRange);

            var ipScannerOptions = new IPScannerOptions
            {
                Threads = SettingsManager.Current.IPScanner_Threads,
                ICMPTimeout = SettingsManager.Current.IPScanner_ICMPTimeout,
                ICMPBuffer = new byte[SettingsManager.Current.IPScanner_ICMPBuffer],
                ICMPAttempts = SettingsManager.Current.IPScanner_ICMPAttempts,
                ResolveHostname = SettingsManager.Current.IPScanner_ResolveHostname,
                UseCustomDNSServer = SettingsManager.Current.IPScanner_UseCustomDNSServer,
                CustomDNSServer = SettingsManager.Current.IPScanner_CustomDNSServer.Select(x => x.Trim()).ToList(),
                DNSPort = SettingsManager.Current.IPScanner_DNSPort,
                DNSTransportType = SettingsManager.Current.IPScanner_DNSTransportType,
                DNSRecursion = SettingsManager.Current.IPScanner_DNSRecursion,
                DNSUseResolverCache = SettingsManager.Current.IPScanner_DNSUseResolverCache,
                DNSAttempts = SettingsManager.Current.IPScanner_DNSAttempts,
                DNSTimeout = SettingsManager.Current.IPScanner_DNSTimeout,
                ResolveMACAddress = SettingsManager.Current.IPScanner_ResolveMACAddress,
                ShowScanResultForAllIPAddresses = SettingsManager.Current.IPScanner_ShowScanResultForAllIPAddresses
            };

            var ipScanner = new IPScanner();

            ipScanner.HostFound += IpScanner_HostFound;
            ipScanner.ScanComplete += IpScanner_ScanComplete;
            ipScanner.ProgressChanged += IpScanner_ProgressChanged;
            ipScanner.UserHasCanceled += IpScanner_UserHasCanceled;

            ipScanner.ScanAsync(ipAddresses, ipScannerOptions, _cancellationTokenSource.Token);
        }

        private void StopScan()
        {
            CancelScan = true;
            _cancellationTokenSource.Cancel();
        }

        private void ScanFinished()
        {
            // Stop timer and stopwatch
            _stopwatch.Stop();
            _dispatcherTimer.Stop();

            Duration = _stopwatch.Elapsed;
            EndTime = DateTime.Now;

            _stopwatch.Reset();

            CancelScan = false;
            IsScanRunning = false;
        }

        private void AddIPRangeToHistory(string ipRange)
        {
            // Create the new list
            List<string> list = ListHelper.Modify(SettingsManager.Current.IPScanner_IPRangeHistory.ToList(), ipRange, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.IPScanner_IPRangeHistory.Clear();
            OnPropertyChanged(nameof(IPRange)); // Raise property changed again, after the collection has been cleared

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.IPScanner_IPRangeHistory.Add(x));
        }

        public void OnClose()
        {
            // Stop scan
            if (IsScanRunning)
                StopScan();
        }
        #endregion

        #region Events
        private void IpScanner_HostFound(object sender, IPScannerHostFoundArgs e)
        {
            var ipScannerHostInfo = IPScannerHostInfo.Parse(e);

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
            {
                lock (IPScanResult)
                    IPScanResult.Add(ipScannerHostInfo);
            }));

            HostsFound++;
        }

        private void IpScanner_ScanComplete(object sender, EventArgs e)
        {
            ScanFinished();
        }

        private void IpScanner_ProgressChanged(object sender, ProgressChangedArgs e)
        {
            IPAddressesScanned = e.Value;
        }

        private void IpScanner_DnsResolveFailed(AggregateException e)
        {
            StatusMessage = $"{Resources.Localization.Strings.TheFollowingHostnamesCouldNotBeResolved} {string.Join(", ", e.Flatten().InnerExceptions.Select(x => x.Message))}";
            DisplayStatusMessage = true;

            ScanFinished();
        }

        private void IpScanner_UserHasCanceled(object sender, EventArgs e)
        {
            StatusMessage = Resources.Localization.Strings.CanceledByUserMessage;
            DisplayStatusMessage = true;

            ScanFinished();
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            Duration = _stopwatch.Elapsed;
        }

        private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SettingsInfo.IPScanner_ResolveMACAddress):
                    OnPropertyChanged(nameof(ResolveMACAddress));
                    break;
                case nameof(SettingsInfo.IPScanner_ResolveHostname):
                    OnPropertyChanged(nameof(ResolveHostname));
                    break;
                case nameof(SettingsInfo.IPScanner_ShowStatistics):
                    OnPropertyChanged(nameof(ShowStatistics));
                    break;
            }
        }
        #endregion
    }
}
