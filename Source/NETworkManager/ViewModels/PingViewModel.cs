using NETworkManager.Models.Network;
using NETworkManager.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Threading;
using System.Windows.Data;
using Dragablz;
using NETworkManager.Controls;
using NETworkManager.Utilities;
using System.Collections.ObjectModel;

namespace NETworkManager.ViewModels
{
    public class PingViewModel : ViewModelBase
    {
        #region Variables
        CancellationTokenSource cancellationTokenSource;

        private int _tabId;
        private bool _firstLoad = true;

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
              
        private bool _isPingRunning;
        public bool IsPingRunning
        {
            get { return _isPingRunning; }
            set
            {
                if (value == _isPingRunning)
                    return;

                _isPingRunning = value;
                OnPropertyChanged();
            }
        }

        private bool _cancelPing;
        public bool CancelPing
        {
            get { return _cancelPing; }
            set
            {
                if (value == _cancelPing)
                    return;

                _cancelPing = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<PingInfo> _pingResult = new ObservableCollection<PingInfo>();
        public ObservableCollection<PingInfo> PingResult
        {
            get { return _pingResult; }
            set
            {
                if (value == _pingResult)
                    return;

                _pingResult = value;
            }
        }

        private ICollectionView _pingResultView;
        public ICollectionView PingResultView
        {
            get { return _pingResultView; }
        }

        private PingInfo _selectedPingResult;
        public PingInfo SelectedPingResult
        {
            get { return _selectedPingResult; }
            set
            {
                if (value == _selectedPingResult)
                    return;

                _selectedPingResult = value;
                OnPropertyChanged();
            }
        }

        private int _pingsTransmitted;
        public int PingsTransmitted
        {
            get { return _pingsTransmitted; }
            set
            {
                if (value == _pingsTransmitted)
                    return;

                _pingsTransmitted = value;
                OnPropertyChanged();
            }
        }

        private int _pingsReceived;
        public int PingsReceived
        {
            get { return _pingsReceived; }
            set
            {
                if (value == _pingsReceived)
                    return;

                _pingsReceived = value;
                OnPropertyChanged();
            }
        }

        private int _pingsLost;
        public int PingsLost
        {
            get { return _pingsLost; }
            set
            {
                if (value == _pingsLost)
                    return;

                _pingsLost = value;
                OnPropertyChanged();
            }
        }

        private long _minimumTime;
        public long MinimumTime
        {
            get { return _minimumTime; }
            set
            {
                if (value == _minimumTime)
                    return;

                _minimumTime = value;
                OnPropertyChanged();
            }
        }

        private long _maximumTime;
        public long MaximumTime
        {
            get { return _maximumTime; }
            set
            {
                if (value == _maximumTime)
                    return;

                _maximumTime = value;
                OnPropertyChanged();
            }
        }

        private int _averageTime;
        public int AverageTime
        {
            get { return _averageTime; }
            set
            {
                if (value == _averageTime)
                    return;

                _averageTime = value;
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
                    SettingsManager.Current.Ping_ExpandStatistics = value;

                _expandStatistics = value;
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

        public bool ShowStatistics
        {
            get { return SettingsManager.Current.Ping_ShowStatistics; }
        }
        #endregion

        #region Contructor, load settings    
        public PingViewModel(int tabId, string host)
        {
            _tabId = tabId;
            Host = host;

            // Set collection view
            _hostHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.Ping_HostHistory);

            // Result view
            _pingResultView = CollectionViewSource.GetDefaultView(PingResult);

            LoadSettings();

            // Detect if settings have changed...
            SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;

            _isLoading = false;
        }

        public void OnLoaded()
        {
            if (_firstLoad)
            {
                if (!string.IsNullOrEmpty(Host))
                    StartPing();

                _firstLoad = false;
            }
        }

        private void LoadSettings()
        {
            ExpandStatistics = SettingsManager.Current.Ping_ExpandStatistics;
        }
        #endregion

        #region ICommands & Actions
        public ICommand PingCommand
        {
            get { return new RelayCommand(p => PingAction()); }
        }

        private void PingAction()
        {
            if (IsPingRunning)
                StopPing();
            else
                StartPing();
        }

        public ICommand CopySelectedTimestampCommand
        {
            get { return new RelayCommand(p => CopySelectedTimestampAction()); }
        }

        private void CopySelectedTimestampAction()
        {
            Clipboard.SetText(SelectedPingResult.Timestamp.ToString());
        }

        public ICommand CopySelectedIPAddressCommand
        {
            get { return new RelayCommand(p => CopySelectedIPAddressAction()); }
        }

        private void CopySelectedIPAddressAction()
        {
            Clipboard.SetText(SelectedPingResult.IPAddress.ToString());
        }

        public ICommand CopySelectedHostnameCommand
        {
            get { return new RelayCommand(p => CopySelectedHostnameAction()); }
        }

        private void CopySelectedHostnameAction()
        {
            Clipboard.SetText(SelectedPingResult.Hostname);
        }

        public ICommand CopySelectedBytesCommand
        {
            get { return new RelayCommand(p => CopySelectedBytesAction()); }
        }

        private void CopySelectedBytesAction()
        {
            Clipboard.SetText(SelectedPingResult.Bytes.ToString());
        }

        public ICommand CopySelectedTimeCommand
        {
            get { return new RelayCommand(p => CopySelectedTimeAction()); }
        }

        private void CopySelectedTimeAction()
        {
            Clipboard.SetText(SelectedPingResult.Time.ToString());
        }

        public ICommand CopySelectedTTLCommand
        {
            get { return new RelayCommand(p => CopySelectedTTLAction()); }
        }

        private void CopySelectedTTLAction()
        {
            Clipboard.SetText(SelectedPingResult.TTL.ToString());
        }

        public ICommand CopySelectedStatusCommand
        {
            get { return new RelayCommand(p => CopySelectedStatusAction()); }
        }

        private void CopySelectedStatusAction()
        {
            Clipboard.SetText(LocalizationManager.GetStringByKey("String_IPStatus_" + SelectedPingResult.Status.ToString()));
        }
        #endregion

        #region Methods      
        private async void StartPing()
        {
            DisplayStatusMessage = false;
            IsPingRunning = true;

            // Measure the time
            StartTime = DateTime.Now;
            stopwatch.Start();
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            dispatcherTimer.Start();
            EndTime = null;

            // Reset the latest results
            PingResult.Clear();
            PingsTransmitted = 0;
            PingsReceived = 0;
            PingsLost = 0;
            AverageTime = 0;
            MinimumTime = 0;
            MaximumTime = 0;

            // Change the tab title (not nice, but it works)
            Window window = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

            if (window != null)
            {
                foreach (TabablzControl tabablzControl in VisualTreeHelper.FindVisualChildren<TabablzControl>(window))
                {
                    tabablzControl.Items.OfType<DragablzTabItem>().First(x => x.Id == _tabId).Header = Host;
                }
            }

            // Try to parse the string into an IP-Address
            bool hostIsIP = IPAddress.TryParse(Host, out IPAddress ipAddress);

            try
            {
                // Try to resolve the hostname
                if (!hostIsIP)
                {
                    IPHostEntry ipHostEntrys = await Dns.GetHostEntryAsync(Host);

                    foreach (IPAddress ip in ipHostEntrys.AddressList)
                    {
                        if (ip.AddressFamily == AddressFamily.InterNetwork && SettingsManager.Current.Ping_ResolveHostnamePreferIPv4)
                        {
                            ipAddress = ip;
                            continue;
                        }
                        else if (ip.AddressFamily == AddressFamily.InterNetworkV6 && !SettingsManager.Current.Ping_ResolveHostnamePreferIPv4)
                        {
                            ipAddress = ip;
                            continue;
                        }
                    }

                    // Fallback --> If we could not resolve our prefered ip protocol for the hostname
                    if (!hostIsIP)
                    {
                        foreach (IPAddress ip in ipHostEntrys.AddressList)
                        {
                            ipAddress = ip;
                            continue;
                        }
                    }
                }
            }
            catch (SocketException) // This will catch DNS resolve errors
            {
                if (CancelPing)
                    UserHasCanceled();
                else
                    PingFinished();

                StatusMessage = string.Format(LocalizationManager.GetStringByKey("String_CouldNotResolveHostnameFor"), Host);
                DisplayStatusMessage = true;

                return;
            }                                            

            // Add the hostname or ip address to the history
            AddHostToHistory(Host);

            cancellationTokenSource = new CancellationTokenSource();

            PingOptions pingOptions = new PingOptions()
            {
                Attempts = SettingsManager.Current.Ping_Attempts,
                Timeout = SettingsManager.Current.Ping_Timeout,
                Buffer = new byte[SettingsManager.Current.Ping_Buffer],
                TTL = SettingsManager.Current.Ping_TTL,
                DontFragment = SettingsManager.Current.Ping_DontFragment,
                WaitTime = SettingsManager.Current.Ping_WaitTime,
                ExceptionCancelCount = SettingsManager.Current.Ping_ExceptionCancelCount,
                Hostname = hostIsIP ? string.Empty : Host
            };

            Ping ping = new Ping();

            ping.PingReceived += Ping_PingReceived;
            ping.PingCompleted += Ping_PingCompleted;
            ping.PingException += Ping_PingException;
            ping.UserHasCanceled += Ping_UserHasCanceled;

            ping.SendAsync(ipAddress, pingOptions, cancellationTokenSource.Token);
        }

        private void StopPing()
        {
            CancelPing = true;

            if (cancellationTokenSource != null)
                cancellationTokenSource.Cancel();
        }

        private void UserHasCanceled()
        {
            CancelPing = false;

            PingFinished();
        }

        private void PingFinished()
        {
            IsPingRunning = false;

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
            List<string> list = ListHelper.Modify(SettingsManager.Current.Ping_HostHistory.ToList(), host, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.Ping_HostHistory.Clear();
            OnPropertyChanged(nameof(Host)); // Raise property changed again, after the collection has been cleared

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.Ping_HostHistory.Add(x));
        }

        public void OnClose()
        {
            // Stop the ping
            if (IsPingRunning)
                PingAction();
        }
        #endregion

        #region Events
        private void Ping_PingReceived(object sender, PingReceivedArgs e)
        {
            PingInfo pingInfo = PingInfo.Parse(e);

            // Add the result to the collection
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate ()
            {
                lock (PingResult)
                    PingResult.Add(pingInfo);
            }));

            // Calculate statistics
            PingsTransmitted++;

            if (pingInfo.Status == System.Net.NetworkInformation.IPStatus.Success)
            {
                PingsReceived++;

                if (PingsReceived == 1)
                {
                    MinimumTime = pingInfo.Time;
                    MaximumTime = pingInfo.Time;
                }
                else
                {
                    if (MinimumTime > pingInfo.Time)
                        MinimumTime = pingInfo.Time;

                    if (MaximumTime < pingInfo.Time)
                        MaximumTime = pingInfo.Time;

                    // lock, because the collection is changed from another thread...
                    // I hope this won't slow the application or causes a hight cpu load
                    lock (PingResult)
                        AverageTime = (int)PingResult.Average(s => s.Time);
                }
            }
            else
            {
                PingsLost++;
            }
        }

        private void Ping_PingCompleted(object sender, EventArgs e)
        {
            PingFinished();
        }

        private void Ping_PingException(object sender, PingExceptionArgs e)
        {
            // Get the error code and change the message (maybe we can help the user with troubleshooting)
            Win32Exception w32ex = e.InnerException as Win32Exception;

            string errorMessage = string.Empty;

            switch (w32ex.NativeErrorCode)
            {
                case 1231:
                    errorMessage = LocalizationManager.GetStringByKey("String_NetworkLocationCannotBeReached");
                    break;
                default:
                    errorMessage = e.InnerException.Message;
                    break;
            }

            PingFinished();

            StatusMessage = errorMessage;
            DisplayStatusMessage = true;
        }

        private void Ping_UserHasCanceled(object sender, EventArgs e)
        {
            UserHasCanceled();
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            Duration = stopwatch.Elapsed;
        }

        private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SettingsInfo.Ping_ShowStatistics))
                OnPropertyChanged(nameof(ShowStatistics));
        }
        #endregion
    }
}