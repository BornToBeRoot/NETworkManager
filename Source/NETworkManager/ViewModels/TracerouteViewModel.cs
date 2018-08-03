using System.Net;
using System.Windows.Input;
using System.Windows;
using System;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using NETworkManager.Models.Settings;
using NETworkManager.Models.Network;
using System.Threading;
using NETworkManager.Utilities;
using System.Diagnostics;
using System.Windows.Threading;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Linq;
using Dragablz;
using NETworkManager.Controls;

namespace NETworkManager.ViewModels
{
    public class TracerouteViewModel : ViewModelBase
    {
        #region Variables
        private CancellationTokenSource _cancellationTokenSource;

        private readonly int _tabId;
        private bool _firstLoad = true;

        private readonly DispatcherTimer _dispatcherTimer = new DispatcherTimer();
        private readonly Stopwatch _stopwatch = new Stopwatch();

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

        private bool _isTraceRunning;
        public bool IsTraceRunning
        {
            get => _isTraceRunning;
            set
            {
                if (value == _isTraceRunning)
                    return;

                _isTraceRunning = value;
                OnPropertyChanged();
            }
        }

        private bool _cancelTrace;
        public bool CancelTrace
        {
            get => _cancelTrace;
            set
            {
                if (value == _cancelTrace)
                    return;

                _cancelTrace = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<TracerouteHopInfo> _traceResult = new ObservableCollection<TracerouteHopInfo>();
        public ObservableCollection<TracerouteHopInfo> TraceResult
        {
            get => _traceResult;
            set
            {
                if (Equals(value, _traceResult))
                    return;

                _traceResult = value;
            }
        }

        public ICollectionView TraceResultView { get; }

        private TracerouteHopInfo _selectedTraceResult;
        public TracerouteHopInfo SelectedTraceResult
        {
            get => _selectedTraceResult;
            set
            {
                if (value == _selectedTraceResult)
                    return;

                _selectedTraceResult = value;
                OnPropertyChanged();
            }
        }

        public bool ResolveHostname => SettingsManager.Current.Traceroute_ResolveHostname;

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
                    SettingsManager.Current.Traceroute_ExpandStatistics = value;

                _expandStatistics = value;
                OnPropertyChanged();
            }
        }

        private int _hops;
        public int Hops
        {
            get => _hops;
            set
            {
                if (value == _hops)
                    return;

                _hops = value;
                OnPropertyChanged();
            }
        }

        public bool ShowStatistics => SettingsManager.Current.Traceroute_ShowStatistics;

        #endregion

        #region Constructor, load settings
        public TracerouteViewModel(int tabId, string host)
        {
            _isLoading = true;

            _tabId = tabId;
            Host = host;

            // Set collection view
            HostHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.Traceroute_HostHistory);

            // Result view
            TraceResultView = CollectionViewSource.GetDefaultView(TraceResult);
            TraceResultView.SortDescriptions.Add(new SortDescription(nameof(TracerouteHopInfo.Hop), ListSortDirection.Ascending));

            LoadSettings();

            SettingsManager.Current.PropertyChanged += Current_PropertyChanged;

            _isLoading = false;
        } 

        public void OnLoaded()
        {
            if (!_firstLoad)
                return;

            if (!string.IsNullOrEmpty(Host))
                StartTrace();

            _firstLoad = false;
        }

        private void LoadSettings()
        {
            ExpandStatistics = SettingsManager.Current.Traceroute_ExpandStatistics;
        }
        #endregion

        #region ICommands & Actions
        public ICommand TraceCommand
        {
            get { return new RelayCommand(p => TraceAction()); }
        }

        private void TraceAction()
        {
            if (IsTraceRunning)
                StopTrace();
            else
                StartTrace();
        }

        public ICommand PerformIPScannerCommand
        {
            get { return new RelayCommand(p => PerformIPScannerAction()); }
        }

        private void PerformIPScannerAction()
        {
            EventSystem.RedirectToApplication(ApplicationViewManager.Name.IPScanner, SelectedTraceResult.IPAddress.ToString());
        }

        public ICommand PerformPortScanCommand
        {
            get { return new RelayCommand(p => PerformPortScanAction()); }
        }

        private void PerformPortScanAction()
        {
            EventSystem.RedirectToApplication(ApplicationViewManager.Name.PortScanner, SelectedTraceResult.IPAddress.ToString());
        }

        public ICommand PerformPingCommand
        {
            get { return new RelayCommand(p => PerformPingAction()); }
        }

        private void PerformPingAction()
        {
            EventSystem.RedirectToApplication(ApplicationViewManager.Name.Ping, SelectedTraceResult.IPAddress.ToString());
        }

        public ICommand PerformDNSLookupIPAddressCommand
        {
            get { return new RelayCommand(p => PerformDNSLookupIPAddressAction()); }
        }

        private void PerformDNSLookupIPAddressAction()
        {
            EventSystem.RedirectToApplication(ApplicationViewManager.Name.DNSLookup, SelectedTraceResult.IPAddress.ToString());
        }

        public ICommand PerformDNSLookupHostnameCommand
        {
            get { return new RelayCommand(p => PerformDNSLookupHostnameAction()); }
        }

        private void PerformDNSLookupHostnameAction()
        {
            EventSystem.RedirectToApplication(ApplicationViewManager.Name.DNSLookup, SelectedTraceResult.Hostname);
        }

        public ICommand ConnectRemoteDesktopCommand
        {
            get { return new RelayCommand(p => ConnectRemoteDesktopAction()); }
        }

        private void ConnectRemoteDesktopAction()
        {
            EventSystem.RedirectToApplication(ApplicationViewManager.Name.RemoteDesktop, SelectedTraceResult.IPAddress.ToString());
        }

        public ICommand ConnectPuTTYCommand
        {
            get { return new RelayCommand(p => ConnectPuTTYAction()); }
        }

        private void ConnectPuTTYAction()
        {
            EventSystem.RedirectToApplication(ApplicationViewManager.Name.PuTTY, SelectedTraceResult.IPAddress.ToString());
        }

        public ICommand PerformSNMPCommand
        {
            get { return new RelayCommand(p => PerformSNMPAction()); }
        }

        private void PerformSNMPAction()
        {
            EventSystem.RedirectToApplication(ApplicationViewManager.Name.SNMP, SelectedTraceResult.IPAddress.ToString());
        }

        public ICommand CopySelectedHopCommand
        {
            get { return new RelayCommand(p => CopySelectedHopAction()); }
        }

        private void CopySelectedHopAction()
        {
            Clipboard.SetText(SelectedTraceResult.Hop.ToString());
        }

        public ICommand CopySelectedTime1Command
        {
            get { return new RelayCommand(p => CopySelectedTime1Action()); }
        }

        private void CopySelectedTime1Action()
        {
            Clipboard.SetText(SelectedTraceResult.Time1.ToString(CultureInfo.CurrentCulture));
        }

        public ICommand CopySelectedTime2Command
        {
            get { return new RelayCommand(p => CopySelectedTime2Action()); }
        }

        private void CopySelectedTime2Action()
        {
            Clipboard.SetText(SelectedTraceResult.Time2.ToString(CultureInfo.CurrentCulture));
        }

        public ICommand CopySelectedTime3Command
        {
            get { return new RelayCommand(p => CopySelectedTime3Action()); }
        }

        private void CopySelectedTime3Action()
        {
            Clipboard.SetText(SelectedTraceResult.Time3.ToString(CultureInfo.CurrentCulture));
        }

        public ICommand CopySelectedIPAddressCommand
        {
            get { return new RelayCommand(p => CopySelectedIPAddressAction()); }
        }

        private void CopySelectedIPAddressAction()
        {
            Clipboard.SetText(SelectedTraceResult.IPAddress.ToString());
        }

        public ICommand CopySelectedHostnameCommand
        {
            get { return new RelayCommand(p => CopySelectedHostnameAction()); }
        }

        private void CopySelectedHostnameAction()
        {
            Clipboard.SetText(SelectedTraceResult.Hostname);
        }
        #endregion

        #region Methods
        private void StopTrace()
        {
            CancelTrace = true;
            _cancellationTokenSource.Cancel();
        }

        private async void StartTrace()
        {
            DisplayStatusMessage = false;
            IsTraceRunning = true;

            // Measure the time
            StartTime = DateTime.Now;
            _stopwatch.Start();
            _dispatcherTimer.Tick += DispatcherTimer_Tick;
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            _dispatcherTimer.Start();
            EndTime = null;

            TraceResult.Clear();
            Hops = 0;

            // Change the tab title (not nice, but it works)
            var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

            if (window != null)
            {
                foreach (var tabablzControl in VisualTreeHelper.FindVisualChildren<TabablzControl>(window))
                {
                    tabablzControl.Items.OfType<DragablzTabItem>().First(x => x.Id == _tabId).Header = Host;
                }
            }

            _cancellationTokenSource = new CancellationTokenSource();

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
                            case AddressFamily.InterNetwork when SettingsManager.Current.Traceroute_ResolveHostnamePreferIPv4:
                                ipAddress = ipAddr;
                                break;
                            case AddressFamily.InterNetworkV6 when SettingsManager.Current.Traceroute_ResolveHostnamePreferIPv4:
                                ipAddress = ipAddr;
                                break;
                        }
                    }

                    // Fallback --> If we could not resolve our prefered ip protocol
                    if (ipAddress == null)
                    {
                        foreach (var ip in ipHostEntrys.AddressList)
                        {
                            ipAddress = ip;
                            break;
                        }
                    }
                }

                var tracerouteOptions = new TracerouteOptions
                {
                    Timeout = SettingsManager.Current.Traceroute_Timeout,
                    Buffer = SettingsManager.Current.Traceroute_Buffer,
                    MaximumHops = SettingsManager.Current.Traceroute_MaximumHops,
                    DontFragement = true,
                    ResolveHostname = SettingsManager.Current.Traceroute_ResolveHostname
                };

                var traceroute = new Traceroute();

                traceroute.HopReceived += Traceroute_HopReceived;
                traceroute.TraceComplete += Traceroute_TraceComplete;
                traceroute.MaximumHopsReached += Traceroute_MaximumHopsReached;
                traceroute.UserHasCanceled += Traceroute_UserHasCanceled;

                traceroute.TraceAsync(ipAddress, tracerouteOptions, _cancellationTokenSource.Token);

                // Add the host to history
                AddHostToHistory(Host);
            }
            catch (SocketException) // This will catch DNS resolve errors
            {
                TracerouteFinished();

                StatusMessage = string.Format( Resources.Localization.Strings.CouldNotResolveHostnameFor, Host);
                DisplayStatusMessage = true;
            }
            catch (Exception ex) // This will catch any exception
            {
                TracerouteFinished();

                StatusMessage = ex.Message;
                DisplayStatusMessage = true;
            }
        }

        private void TracerouteFinished()
        {
            // Stop timer and stopwatch
            _stopwatch.Stop();
            _dispatcherTimer.Stop();

            Duration = _stopwatch.Elapsed;
            EndTime = DateTime.Now;

            _stopwatch.Reset();

            CancelTrace = false;
            IsTraceRunning = false;
        }

        private void AddHostToHistory(string host)
        {
            // Create the new list
            var list = ListHelper.Modify(SettingsManager.Current.Traceroute_HostHistory.ToList(), host, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.Traceroute_HostHistory.Clear();
            OnPropertyChanged(nameof(Host)); // Raise property changed again, after the collection has been cleared

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.Traceroute_HostHistory.Add(x));
        }

        public void OnClose()
        {
            if (IsTraceRunning)
                StopTrace();
        }
        #endregion

        #region Events
        private void Traceroute_HopReceived(object sender, TracerouteHopReceivedArgs e)
        {
            var tracerouteInfo = TracerouteHopInfo.Parse(e);

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
            {
                lock (TraceResult)
                    TraceResult.Add(tracerouteInfo);
            }));

            Hops++;
        }

        private void Traceroute_MaximumHopsReached(object sender, MaximumHopsReachedArgs e)
        {
            TracerouteFinished();

            StatusMessage = string.Format(Resources.Localization.Strings.MaximumNumberOfHopsReached, e.Hops);
            DisplayStatusMessage = true;
        }

        private void Traceroute_UserHasCanceled(object sender, EventArgs e)
        {
            TracerouteFinished();

            StatusMessage = Resources.Localization.Strings.CanceledByUserMessage;
            DisplayStatusMessage = true;
        }

        private void Traceroute_TraceComplete(object sender, EventArgs e)
        {
            TracerouteFinished();
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            Duration = _stopwatch.Elapsed;
        }

        private void Current_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SettingsInfo.Traceroute_ResolveHostname):
                    OnPropertyChanged(nameof(ResolveHostname));
                    break;
                case nameof(SettingsInfo.Traceroute_ShowStatistics):
                    OnPropertyChanged(nameof(ShowStatistics));
                    break;
            }
        }
        #endregion               
    }
}