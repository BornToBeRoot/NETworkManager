using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Export;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using Ping = NETworkManager.Models.Network.Ping;

namespace NETworkManager.ViewModels;

public class PingMonitorViewModel : ViewModelBase
{
    #region Contructor, load settings

    public PingMonitorViewModel(IDialogCoordinator instance, Guid hostId, Action<Guid> removeHostByGuid,
        (IPAddress ipAddress, string hostname) host, string group)
    {
        _dialogCoordinator = instance;

        HostId = hostId;
        _removeHostByGuid = removeHostByGuid;

        Title = string.IsNullOrEmpty(host.hostname) ? host.ipAddress.ToString() : $"{host.hostname} # {host.ipAddress}";

        IPAddress = host.ipAddress;
        Hostname = host.hostname;
        Group = group;

        InitialTimeChart();

        ExpandHostView = SettingsManager.Current.PingMonitor_ExpandHostView;
    }

    #endregion

    #region Variables

    private readonly IDialogCoordinator _dialogCoordinator;
    private CancellationTokenSource _cancellationTokenSource;

    public readonly Guid HostId;
    private readonly Action<Guid> _removeHostByGuid;

    private List<PingInfo> _pingInfoList;

    private string _title;

    public string Title
    {
        get => _title;
        private set
        {
            if (value == _title)
                return;

            _title = value;
            OnPropertyChanged();
        }
    }

    private string _hostname;

    public string Hostname
    {
        get => _hostname;
        private set
        {
            if (value == _hostname)
                return;

            _hostname = value;
            OnPropertyChanged();
        }
    }

    private readonly IPAddress _ipAddress;

    public IPAddress IPAddress
    {
        get => _ipAddress;
        private init
        {
            if (Equals(value, _ipAddress))
                return;

            _ipAddress = value;
            OnPropertyChanged();
        }
    }

    private string _group;

    public string Group
    {
        get => _group;
        set
        {
            if (value == _group)
                return;

            _group = value;
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
        private set
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
        private set
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

    private double _packetLoss;

    public double PacketLoss
    {
        get => _packetLoss;
        set
        {
            if (Math.Abs(value - _packetLoss) < 0.01)
                return;

            _packetLoss = value;
            OnPropertyChanged();
        }
    }

    private long _timeMs;

    public long TimeMs
    {
        get => _timeMs;
        set
        {
            if (value == _timeMs)
                return;

            _timeMs = value;
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

        FormatterDate = value =>
            DateTimeHelper.DateTimeToTimeString(new DateTime((long)(value * TimeSpan.FromHours(1).Ticks)));
        FormatterPingTime = value => $"{value} ms";
    }

    public Func<double, string> FormatterDate { get; set; }
    public Func<double, string> FormatterPingTime { get; set; }
    public SeriesCollection Series { get; set; }

    private string _errorMessage;

    public string ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (value == _errorMessage)
                return;

            _errorMessage = value;
            OnPropertyChanged();
        }
    }

    private bool _isErrorMessageDisplayed;

    public bool IsErrorMessageDisplayed
    {
        get => _isErrorMessageDisplayed;
        set
        {
            if (value == _isErrorMessageDisplayed)
                return;

            _isErrorMessageDisplayed = value;
            OnPropertyChanged();
        }
    }

    private bool _expandHostView;

    public bool ExpandHostView
    {
        get => _expandHostView;
        set
        {
            if (value == _expandHostView)
                return;

            _expandHostView = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region ICommands & Actions

    public ICommand PingCommand => new RelayCommand(_ => PingAction());

    private void PingAction()
    {
        if (IsRunning)
            Stop();
        else
            Start();
    }

    public ICommand CloseCommand => new RelayCommand(_ => CloseAction());

    private void CloseAction()
    {
        _removeHostByGuid(HostId);
    }

    #endregion

    #region Methods

    public void Start()
    {
        IsErrorMessageDisplayed = false;
        IsRunning = true;

        // Reset history
        _pingInfoList = [];

        // Reset the latest results            
        StatusTime = DateTime.Now;
        Transmitted = 0;
        Received = 0;
        Lost = 0;
        PacketLoss = 0;

        // Reset chart
        ResetTimeChart();

        _cancellationTokenSource = new CancellationTokenSource();

        var ping = new Ping
        {
            Timeout = SettingsManager.Current.PingMonitor_Timeout,
            Buffer = new byte[SettingsManager.Current.PingMonitor_Buffer],
            TTL = SettingsManager.Current.PingMonitor_TTL,
            DontFragment = SettingsManager.Current.PingMonitor_DontFragment,
            WaitTime = SettingsManager.Current.PingMonitor_WaitTime
        };

        ping.PingReceived += Ping_PingReceived;
        ping.PingException += Ping_PingException;
        ping.HostnameResolved += Ping_HostnameResolved;
        ping.UserHasCanceled += Ping_UserHasCanceled;

        ping.SendAsync(IPAddress, _cancellationTokenSource.Token);
    }

    public void Stop()
    {
        if (!IsRunning)
            return;

        _cancellationTokenSource?.Cancel();
    }

    private void ResetTimeChart()
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
            Title = Strings.Export
        };

        var exportViewModel = new ExportViewModel(async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                try
                {
                    ExportManager.Export(instance.FilePath, instance.FileType,
                        new ObservableCollection<PingInfo>(_pingInfoList));
                }
                catch (Exception ex)
                {
                    var settings = AppearanceManager.MetroDialog;
                    settings.AffirmativeButtonText = Strings.OK;

                    await _dialogCoordinator.ShowMessageAsync(this, Strings.Error,
                        Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine +
                        Environment.NewLine + ex.Message, MessageDialogStyle.Affirmative, settings);
                }

                SettingsManager.Current.PingMonitor_ExportFileType = instance.FileType;
                SettingsManager.Current.PingMonitor_ExportFilePath = instance.FilePath;
            }, _ => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); },
            [
                ExportFileType.Csv, ExportFileType.Xml, ExportFileType.Json
            ], false,
            SettingsManager.Current.PingMonitor_ExportFileType,
            SettingsManager.Current.PingMonitor_ExportFilePath);

        customDialog.Content = new ExportDialog
        {
            DataContext = exportViewModel
        };

        await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
    }

    #endregion

    #region Events

    private void Ping_PingReceived(object sender, PingReceivedArgs e)
    {
        // Calculate statistics
        Transmitted++;

        LvlChartsDefaultInfo timeInfo;

        if (e.Args.Status == IPStatus.Success)
        {
            if (!IsReachable)
            {
                StatusTime = DateTime.Now;
                IsReachable = true;
            }

            Received++;

            timeInfo = new LvlChartsDefaultInfo(e.Args.Timestamp, e.Args.Time);
        }
        else
        {
            if (IsReachable)
            {
                StatusTime = DateTime.Now;
                IsReachable = false;
            }

            Lost++;

            timeInfo = new LvlChartsDefaultInfo(e.Args.Timestamp, double.NaN);
        }

        PacketLoss = Math.Round((double)Lost / Transmitted * 100, 2);
        TimeMs = e.Args.Time;

        // Null exception may occur when the application is closing        
        Application.Current?.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
        {
            Series[0].Values.Add(timeInfo);

            if (Series[0].Values.Count > 59)
                Series[0].Values.RemoveAt(0);
        }));

        // Add to history
        _pingInfoList.Add(e.Args);
    }

    private void Ping_UserHasCanceled(object sender, EventArgs e)
    {
        IsRunning = false;
    }

    private void Ping_HostnameResolved(object sender, HostnameArgs e)
    {
        // Update title if name was not set in the constructor
        if (string.IsNullOrEmpty(Hostname))
            Title = $"{e.Hostname.TrimEnd('.')} # {IPAddress}";

        Hostname = e.Hostname;
    }

    private void Ping_PingException(object sender, PingExceptionArgs e)
    {
        IsRunning = false;

        ErrorMessage = e.Message;
        IsErrorMessageDisplayed = true;
    }

    #endregion
}