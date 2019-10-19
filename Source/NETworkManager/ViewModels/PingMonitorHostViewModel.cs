using NETworkManager.Models.Network;
using NETworkManager.Models.Settings;
using System;
using System.Collections;
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
using System.Globalization;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models.Export;
using NETworkManager.Views;

namespace NETworkManager.ViewModels
{
    public class PingMonitorHostViewModel : ViewModelBase
    {
        #region Variables        
        private CancellationTokenSource _cancellationTokenSource;

        public readonly int HostId;
        private readonly Action<int> _closeCallback;
        private readonly PingMonitorOptions _pingMonitorOptions;
        private bool _firstLoad = true;

        private readonly DispatcherTimer _dispatcherTimer = new DispatcherTimer();
        private readonly Stopwatch _stopwatch = new Stopwatch();

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

        private IPAddress _ipAddress;
        public IPAddress IPAddress
        {
            get => _ipAddress;
            set
            {
                if (value == _ipAddress)
                    return;

                _ipAddress = value;
                OnPropertyChanged();
            }
        }

        private bool _isPingRunning;
        public bool IsPingRunning
        {
            get => _isPingRunning;
            set
            {
                if (value == _isPingRunning)
                    return;

                _isPingRunning = value;
                OnPropertyChanged();
            }
        }

        private bool _isReachable;
        public bool IsReachable
        {
            get => _isReachable;
            set
            {
                if (value == _isReachable)
                    return;

                _isReachable = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<PingInfo> _pingResults = new ObservableCollection<PingInfo>();
        public ObservableCollection<PingInfo> PingResults
        {
            get => _pingResults;
            set
            {
                if (value != null && value == _pingResults)
                    return;

                _pingResults = value;
            }
        }

        private int _pingsTransmitted;
        public int PingsTransmitted
        {
            get => _pingsTransmitted;
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
            get => _pingsReceived;
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
            get => _pingsLost;
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
            get => _minimumTime;
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
            get => _maximumTime;
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
            get => _averageTime;
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
        #endregion

        #region Contructor, load settings    
        public PingMonitorHostViewModel(int hostId, Action<int> closeCallback, PingMonitorOptions options)
        {
            HostId = hostId;
            _closeCallback = closeCallback;
            _pingMonitorOptions = options;

            Host = options.Host;
            IPAddress = options.IPAddress;
        }

        public void OnLoaded()
        {
            if (!_firstLoad)
                return;

            StartPing();

            _firstLoad = false;
        }
        #endregion

        #region ICommands & Actions
        /*
         // Start
         // Stop
         // Delete


        public ICommand PingCommand => new RelayCommand(p => PingAction(), Ping_CanExecute);

        private bool Ping_CanExecute(object paramter) => Application.Current.MainWindow != null && !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen;

        private void PingAction()
        {
            if (IsPingRunning)
                StopPing();
            else
                StartPing();
        }
        */

        public ICommand CloseCommand => new RelayCommand(p => CloseAction());

        private void CloseAction()
        {
            _closeCallback(HostId);
        }
        #endregion

        #region Methods      
        private void StartPing()
        {
            IsPingRunning = true;

            // Measure the time
            StartTime = DateTime.Now;
            _stopwatch.Start();
            _dispatcherTimer.Tick += DispatcherTimer_Tick;
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            _dispatcherTimer.Start();
            EndTime = null;

            // Reset the latest results
            PingResults.Clear();
            PingsTransmitted = 0;
            PingsReceived = 0;
            PingsLost = 0;
            AverageTime = 0;
            MinimumTime = 0;
            MaximumTime = 0;

            _cancellationTokenSource = new CancellationTokenSource();

            var ping = new Ping
            {
                Attempts = 0,
                Timeout = SettingsManager.Current.Ping_Timeout,
                Buffer = new byte[SettingsManager.Current.Ping_Buffer],
                TTL = SettingsManager.Current.Ping_TTL,
                DontFragment = SettingsManager.Current.Ping_DontFragment,
                WaitTime = SettingsManager.Current.Ping_WaitTime,
                ExceptionCancelCount = SettingsManager.Current.Ping_ExceptionCancelCount,
                Hostname = Host
            };

            ping.PingReceived += Ping_PingReceived;
            //ping.PingCompleted += Ping_PingCompleted;
            //ping.PingException += Ping_PingException;
            ping.UserHasCanceled += Ping_UserHasCanceled;

            ping.SendAsync(IPAddress, _cancellationTokenSource.Token);
        }

        private void StopPing()
        {
            _cancellationTokenSource?.Cancel();
        }

        private void UserHasCanceled()
        {
            PingFinished();
        }

        private void PingFinished()
        {
            IsPingRunning = false;

            // Stop timer and stopwatch
            _stopwatch.Stop();
            _dispatcherTimer.Stop();

            Duration = _stopwatch.Elapsed;
            EndTime = DateTime.Now;

            _stopwatch.Reset();
        }

        public void OnClose()
        {
            // Stop the ping
            if (IsPingRunning)
                StopPing();
        }
        #endregion

        #region Events
        private void Ping_PingReceived(object sender, PingReceivedArgs e)
        {
            var pingInfo = PingInfo.Parse(e);

            // Add the result to the collection
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
            {
                lock (PingResults)
                    PingResults.Add(pingInfo);
            }));

            // Calculate statistics
            PingsTransmitted++;

            if (pingInfo.Status == System.Net.NetworkInformation.IPStatus.Success)
            {
                IsReachable = true;

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
                    lock (PingResults)
                        AverageTime = (int)PingResults.Average(s => s.Time);
                }
            }
            else
            {
                IsReachable = false;

                PingsLost++;
            }
        }

        private void Ping_UserHasCanceled(object sender, EventArgs e)
        {
            UserHasCanceled();
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            Duration = _stopwatch.Elapsed;
        }
        #endregion
    }
}