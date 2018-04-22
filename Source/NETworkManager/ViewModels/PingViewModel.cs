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
using NETworkManager.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Threading;
using System.Windows.Data;
using Dragablz;
using NETworkManager.Controls;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels
{
    public class PingViewModel : ViewModelBase
    {
        #region Variables
        CancellationTokenSource cancellationTokenSource;

        private int _tabId;

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

        private AsyncObservableCollection<PingInfo> _pingResult = new AsyncObservableCollection<PingInfo>();
        public AsyncObservableCollection<PingInfo> PingResult
        {
            get { return _pingResult; }
            set
            {
                if (value == _pingResult)
                    return;

                _pingResult = value;
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
        #endregion

        #region Contructor, load settings    
        public PingViewModel(int tabId)
        {
            _tabId = tabId;

            _hostHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.Ping_HostHistory);

            LoadSettings();

            _isLoading = false;
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
                    tabablzControl.Items.OfType<DragablzPingTabItem>().First(x => x.ID == _tabId).Header = Host;
                }
            }

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
                    if (ipAddress == null)
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
                ExceptionCancelCount = SettingsManager.Current.Ping_ExceptionCancelCount
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
            PingResult.Add(pingInfo);

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
                }

                // I don't know if this can slow my application if the collection is to large
                AverageTime = (int)PingResult.Average(s => s.Time);
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
        #endregion
    }
}