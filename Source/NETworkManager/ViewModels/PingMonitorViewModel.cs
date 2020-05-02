using NETworkManager.Models.Network;
using NETworkManager.Settings;
using System;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using NETworkManager.Utilities;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;

namespace NETworkManager.ViewModels
{
    public class PingMonitorViewModel : ViewModelBase
    {
        #region Variables        
        private CancellationTokenSource _cancellationTokenSource;

        public readonly int HostId;
        private readonly Action<int> _closeCallback;
        private readonly PingMonitorOptions _pingMonitorOptions;
        private bool _firstLoad = true;

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

        private DateTime _statusTime;
        public DateTime StatusTime
        {
            get => _statusTime;
            set
            {
                if (value == _statusTime)
                    return;

                _statusTime = value;
                OnPropertyChanged();
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

        private void InitialTimeChart()
        {
            var dayConfig = Mappers.Xy<LvlChartsDefaultInfo>()
                .X(dayModel => (double)dayModel.DateTime.Ticks / TimeSpan.FromHours(1).Ticks)
                .Y(dayModel => dayModel.Value);

            Series = new SeriesCollection(dayConfig)
            {
                new LineSeries
                {
                    Title = "Time",
                    Values = new ChartValues<LvlChartsDefaultInfo>(),
                    PointGeometry = null
                }
            };

            FormatterDate = value => new DateTime((long)(value * TimeSpan.FromHours(1).Ticks)).ToString("hh:mm:ss");
            FormatterPingTime = value => $"{value} ms";
        }

        public Func<double, string> FormatterDate { get; set; }
        public Func<double, string> FormatterPingTime { get; set; }
        public SeriesCollection Series { get; set; }
        #endregion

        #region Contructor, load settings    
        public PingMonitorViewModel(int hostId, Action<int> closeCallback, PingMonitorOptions options)
        {
            HostId = hostId;
            _closeCallback = closeCallback;
            _pingMonitorOptions = options;

            Host = _pingMonitorOptions.Host;
            IPAddress = _pingMonitorOptions.IPAddress;

            InitialTimeChart();
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
        public ICommand PingCommand => new RelayCommand(p => PingAction());

        private void PingAction()
        {
            Ping();
        }

        public ICommand CloseCommand => new RelayCommand(p => CloseAction());

        private void CloseAction()
        {
            _closeCallback(HostId);
        }
        #endregion

        #region Methods      
        private void Ping()
        {
            if (IsPingRunning)
                StopPing();
            else
                StartPing();
        }

        private void StartPing()
        {
            IsPingRunning = true;

            // Reset the latest results
            StatusTime = DateTime.Now;
            PingsTransmitted = 0;
            PingsReceived = 0;
            PingsLost = 0;

            // Reset chart
            ResetTimeChart();

            _cancellationTokenSource = new CancellationTokenSource();

            var ping = new Ping
            {
                Timeout = SettingsManager.Current.PingMonitor_Timeout,
                Buffer = new byte[SettingsManager.Current.PingMonitor_Buffer],
                TTL = SettingsManager.Current.PingMonitor_TTL,
                DontFragment = SettingsManager.Current.PingMonitor_DontFragment,
                WaitTime = SettingsManager.Current.PingMonitor_WaitTime,
                Hostname = Host
            };

            ping.PingReceived += Ping_PingReceived;
            ping.PingException += Ping_PingException;
            ping.UserHasCanceled += Ping_UserHasCanceled;

            ping.SendAsync(IPAddress, _cancellationTokenSource.Token);
        }

        private void StopPing()
        {
            _cancellationTokenSource?.Cancel();
        }

        private void PingFinished()
        {
            IsPingRunning = false;
        }

        public void ResetTimeChart()
        {
            if (Series == null)
                return;

            Series[0].Values.Clear();

            var currentDateTime = DateTime.Now;

            for (var i = 30; i > 0; i--)
            {
                var bandwidthInfo = new LvlChartsDefaultInfo(currentDateTime.AddSeconds(-i), double.NaN);

                Series[0].Values.Add(bandwidthInfo);
            }
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

            // Calculate statistics
            PingsTransmitted++;

            LvlChartsDefaultInfo timeInfo;

            if (pingInfo.Status == System.Net.NetworkInformation.IPStatus.Success)
            {
                if (!IsReachable)
                {
                    StatusTime = DateTime.Now;
                    IsReachable = true;
                }

                PingsReceived++;

                timeInfo = new LvlChartsDefaultInfo(pingInfo.Timestamp, pingInfo.Time);
            }
            else
            {
                if (IsReachable)
                {
                    StatusTime = DateTime.Now;
                    IsReachable = false;
                }

                PingsLost++;

                timeInfo = new LvlChartsDefaultInfo(pingInfo.Timestamp, double.NaN);
            }

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
            {
                Series[0].Values.Add(timeInfo);

                if (Series[0].Values.Count > 59)
                    Series[0].Values.RemoveAt(0);
            }));
        }

        private void Ping_UserHasCanceled(object sender, EventArgs e)
        {
            PingFinished();
        }

        private void Ping_PingException(object sender, PingExceptionArgs e)
        {
            PingFinished();
        }
        #endregion
    }
}