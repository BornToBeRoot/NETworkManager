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
using System.Collections.Generic;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Views;
using NETworkManager.Models.Export;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace NETworkManager.ViewModels
{
    public class PingMonitorViewModel : ViewModelBase
    {
        #region Variables        
        private readonly IDialogCoordinator _dialogCoordinator;
        private CancellationTokenSource _cancellationTokenSource;

        public readonly int HostId;
        private readonly Action<int> _closeCallback;
        private readonly PingMonitorOptions _pingMonitorOptions;
        private bool _firstLoad = true;

        private List<PingInfo> _pingInfoList;

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

        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                if (value == _isRunning)
                    return;

                _isRunning = value;
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

        private int _transmitted;
        public int Transmitted
        {
            get => _transmitted;
            set
            {
                if (value == _transmitted)
                    return;

                _transmitted = value;
                OnPropertyChanged();
            }
        }

        private int _received;
        public int Received
        {
            get => _received;
            set
            {
                if (value == _received)
                    return;

                _received = value;
                OnPropertyChanged();
            }
        }

        private int _lost;
        public int Lost
        {
            get => _lost;
            set
            {
                if (value == _lost)
                    return;

                _lost = value;
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
        public PingMonitorViewModel(IDialogCoordinator instance, int hostId, Action<int> closeCallback, PingMonitorOptions options)
        {
            _dialogCoordinator = instance;

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
            if (IsRunning)
                StopPing();
            else
                StartPing();
        }

        private void StartPing()
        {
            IsRunning = true;

            // Reset history
            _pingInfoList = new List<PingInfo>();

            // Reset the latest results            
            StatusTime = DateTime.Now;
            Transmitted = 0;
            Received = 0;
            Lost = 0;

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
            IsRunning = false;
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

        public async Task Export()
        {    
            var customDialog = new CustomDialog
            {
                Title = Localization.Resources.Strings.Export
            };

            var exportViewModel = new ExportViewModel(async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                try
                {
                    ExportManager.Export(instance.FilePath, instance.FileType, new ObservableCollection<PingInfo> (_pingInfoList));
                }
                catch (Exception ex)
                {
                    var settings = AppearanceManager.MetroDialog;
                    settings.AffirmativeButtonText = Localization.Resources.Strings.OK;

                    await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.Error, Localization.Resources.Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine + Environment.NewLine + ex.Message, MessageDialogStyle.Affirmative, settings);
                }

                SettingsManager.Current.PingMonitor_ExportFileType = instance.FileType;
                SettingsManager.Current.PingMonitor_ExportFilePath = instance.FilePath;
            }, instance => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); }, new ExportManager.ExportFileType[] { ExportManager.ExportFileType.CSV, ExportManager.ExportFileType.XML, ExportManager.ExportFileType.JSON }, true, SettingsManager.Current.PingMonitor_ExportFileType, SettingsManager.Current.PingMonitor_ExportFilePath);

            customDialog.Content = new ExportDialog
            {
                DataContext = exportViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);            
        }

        public void OnClose()
        {
            // Stop the ping
            if (IsRunning)
                StopPing();
        }
        #endregion

        #region Events
        private void Ping_PingReceived(object sender, PingReceivedArgs e)
        {
            var pingInfo = PingInfo.Parse(e);

            // Calculate statistics
            Transmitted++;

            LvlChartsDefaultInfo timeInfo;

            if (pingInfo.Status == System.Net.NetworkInformation.IPStatus.Success)
            {
                if (!IsReachable)
                {
                    StatusTime = DateTime.Now;
                    IsReachable = true;
                }

                Received++;

                timeInfo = new LvlChartsDefaultInfo(pingInfo.Timestamp, pingInfo.Time);
            }
            else
            {
                if (IsReachable)
                {
                    StatusTime = DateTime.Now;
                    IsReachable = false;
                }

                Lost++;

                timeInfo = new LvlChartsDefaultInfo(pingInfo.Timestamp, double.NaN);
            }

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
            {
                Series[0].Values.Add(timeInfo);

                if (Series[0].Values.Count > 59)
                    Series[0].Values.RemoveAt(0);
            }));

            // Add to history
            _pingInfoList.Add(pingInfo);
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