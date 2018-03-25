using System.Net;
using System.Windows.Input;
using System.Windows;
using System;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using NETworkManager.Models.Settings;
using System.Collections.Generic;
using NETworkManager.Models.Network;
using System.Threading;
using NETworkManager.Helpers;
using System.Diagnostics;
using System.Windows.Threading;
using System.ComponentModel;
using System.Windows.Data;
using System.Linq;
using NETworkManager.Utils;

namespace NETworkManager.ViewModels
{
    public class TracerouteViewModel : ViewModelBase
    {
        #region Variables
        CancellationTokenSource cancellationTokenSource;

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

        private bool _isTraceRunning;
        public bool IsTraceRunning
        {
            get { return _isTraceRunning; }
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
            get { return _cancelTrace; }
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
            get { return _traceResult; }
            set
            {
                if (value == _traceResult)
                    return;

                _traceResult = value;
            }
        }

        private ICollectionView _traceResultView;
        public ICollectionView TraceResultView
        {
            get { return _traceResultView; }
        }

        private TracerouteHopInfo _selectedTraceResult;
        public TracerouteHopInfo SelectedTraceResult
        {
            get { return _selectedTraceResult; }
            set
            {
                if (value == _selectedTraceResult)
                    return;

                _selectedTraceResult = value;
                OnPropertyChanged();
            }
        }

        public bool ResolveHostname
        {
            get { return SettingsManager.Current.Traceroute_ResolveHostname; }
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

        private bool _expandStatistics;
        public bool ExpandStatistics
        {
            get { return _expandStatistics; }
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
            get { return _hops; }
            set
            {
                if (value == _hops)
                    return;

                _hops = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor, load settings
        public TracerouteViewModel()
        {
            // Set collection view
            _hostHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.Traceroute_HostHistory);

            // Result view
            _traceResultView = CollectionViewSource.GetDefaultView(TraceResult);
            _traceResultView.SortDescriptions.Add(new SortDescription(nameof(TracerouteHopInfo.Hop), ListSortDirection.Ascending));

            LoadSettings();

            SettingsManager.Current.PropertyChanged += Current_PropertyChanged;

            _isLoading = false;
        }

        private void Current_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SettingsInfo.Traceroute_ResolveHostname))
                OnPropertyChanged(nameof(ResolveHostname));
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
            Clipboard.SetText(SelectedTraceResult.Time1.ToString());
        }

        public ICommand CopySelectedTime2Command
        {
            get { return new RelayCommand(p => CopySelectedTime2Action()); }
        }

        private void CopySelectedTime2Action()
        {
            Clipboard.SetText(SelectedTraceResult.Time2.ToString());
        }

        public ICommand CopySelectedTime3Command
        {
            get { return new RelayCommand(p => CopySelectedTime3Action()); }
        }

        private void CopySelectedTime3Action()
        {
            Clipboard.SetText(SelectedTraceResult.Time3.ToString());
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
            cancellationTokenSource.Cancel();
        }

        private async void StartTrace()
        {
            DisplayStatusMessage = false;
            IsTraceRunning = true;

            // Measure the time
            StartTime = DateTime.Now;
            stopwatch.Start();
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            dispatcherTimer.Start();
            EndTime = null;

            TraceResult.Clear();
            Hops = 0;

            cancellationTokenSource = new CancellationTokenSource();
            
            // Try to parse the string into an IP-Address
            IPAddress.TryParse(Host, out IPAddress ipAddress);

            try
            {
                // Try to resolve the hostname
                if (ipAddress == null)
                {
                    IPHostEntry ipHostEntrys = await Dns.GetHostEntryAsync(Host);

                    foreach (IPAddress ip in ipHostEntrys.AddressList)
                    {
                        if (ip.AddressFamily == AddressFamily.InterNetwork && SettingsManager.Current.Traceroute_ResolveHostnamePreferIPv4)
                        {
                            ipAddress = ip;
                            continue;
                        }
                        else if (ip.AddressFamily == AddressFamily.InterNetworkV6 && !SettingsManager.Current.Traceroute_ResolveHostnamePreferIPv4)
                        {
                            ipAddress = ip;
                            continue;
                        }
                    }

                    // Fallback --> If we could not resolve our prefered ip protocol
                    if (ipAddress == null)
                    {
                        foreach (IPAddress ip in ipHostEntrys.AddressList)
                        {
                            ipAddress = ip;
                            continue;
                        }
                    }
                }
                
                TracerouteOptions tracerouteOptions = new TracerouteOptions
                {
                    Timeout = SettingsManager.Current.Traceroute_Timeout,
                    Buffer = SettingsManager.Current.Traceroute_Buffer,
                    MaximumHops = SettingsManager.Current.Traceroute_MaximumHops,
                    DontFragement = true,
                    ResolveHostname = SettingsManager.Current.Traceroute_ResolveHostname
                };

                Traceroute traceroute = new Traceroute();

                traceroute.HopReceived += Traceroute_HopReceived;
                traceroute.TraceComplete += Traceroute_TraceComplete;
                traceroute.MaximumHopsReached += Traceroute_MaximumHopsReached;
                traceroute.UserHasCanceled += Traceroute_UserHasCanceled;

                traceroute.TraceAsync(ipAddress, tracerouteOptions, cancellationTokenSource.Token);

                // Add the host to history
                AddHostToHistory(Host);
            }
            catch (SocketException) // This will catch DNS resolve errors
            {
                TracerouteFinished();

                StatusMessage = string.Format(Application.Current.Resources["String_CouldNotResolveHostnameFor"] as string, Host);
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
            stopwatch.Stop();
            dispatcherTimer.Stop();

            Duration = stopwatch.Elapsed;
            EndTime = DateTime.Now;

            stopwatch.Reset();

            CancelTrace = false;
            IsTraceRunning = false;
        }

        private void AddHostToHistory(string host)
        {
            // Create the new list
            List<string> list = ListHelper.Modify(SettingsManager.Current.Traceroute_HostHistory.ToList(), host, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.Traceroute_HostHistory.Clear();
            OnPropertyChanged(nameof(Host)); // Raise property changed again, after the collection has been cleared

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.Traceroute_HostHistory.Add(x));
        }

        public void OnShutdown()
        {
            if (IsTraceRunning)
                StopTrace();
        }
        #endregion

        #region Events
        private void Traceroute_HopReceived(object sender, TracerouteHopReceivedArgs e)
        {
            TracerouteHopInfo tracerouteInfo = TracerouteHopInfo.Parse(e);

            Application.Current.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                TraceResult.Add(tracerouteInfo);
            }));

            Hops++;
        }

        private void Traceroute_MaximumHopsReached(object sender, MaximumHopsReachedArgs e)
        {
            TracerouteFinished();

            StatusMessage = string.Format(Application.Current.Resources["String_MaximumNumberOfHopsReached"] as string, e.Hops);
            DisplayStatusMessage = true;
        }

        private void Traceroute_UserHasCanceled(object sender, System.EventArgs e)
        {
            TracerouteFinished();

            StatusMessage = Application.Current.Resources["String_CanceledByUser"] as string;
            DisplayStatusMessage = true;
        }

        private void Traceroute_TraceComplete(object sender, System.EventArgs e)
        {
            TracerouteFinished();
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            Duration = stopwatch.Elapsed;
        }
        #endregion               
    }
}