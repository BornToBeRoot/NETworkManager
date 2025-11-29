using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using log4net;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Export;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
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
using Ping = NETworkManager.Models.Network.Ping;

namespace NETworkManager.ViewModels;

/// <summary>
/// ViewModel for the Ping Monitor feature, representing a single monitored host.
/// </summary>
public class PingMonitorViewModel : ViewModelBase
{
    #region Contructor, load settings

    /// <summary>
    /// Initializes a new instance of the <see cref="PingMonitorViewModel"/> class.
    /// </summary>
    /// <param name="instance">The dialog coordinator instance.</param>
    /// <param name="hostId">The unique identifier for the host.</param>
    /// <param name="removeHostByGuid">Action to remove the host by its GUID.</param>
    /// <param name="host">Tuple containing the IP address and hostname.</param>
    /// <param name="group">The group name the host belongs to.</param>
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
    private static readonly ILog Log = LogManager.GetLogger(typeof(PingMonitorViewModel));

    private readonly IDialogCoordinator _dialogCoordinator;
    private CancellationTokenSource _cancellationTokenSource;

    public readonly Guid HostId;
    private readonly Action<Guid> _removeHostByGuid;

    private List<PingInfo> _pingInfoList;

    private string _title;

    /// <summary>
    /// Gets the title of the monitor, typically "Hostname # IP".
    /// </summary>
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

    /// <summary>
    /// Gets the hostname of the monitored host.
    /// </summary>
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

    /// <summary>
    /// Gets the IP address of the monitored host.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the group the monitored host belongs to.
    /// </summary>
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

    /// <summary>
    /// Gets or sets a value indicating whether the ping monitoring is currently running.
    /// </summary>
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

    /// <summary>
    /// Gets or sets a value indicating whether the host is reachable (responds to ping).
    /// </summary>
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

    /// <summary>
    /// Gets the time of the last status update.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the total number of ping packets transmitted.
    /// </summary>
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

    /// <summary>
    /// Gets the total number of ping packets received.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the total number of ping packets lost.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the percentage of packet loss.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the round-trip time in milliseconds of the last ping.
    /// </summary>
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

    /// <summary>
    /// Initializes the time chart configuration.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the formatter for the date axis.
    /// </summary>
    public Func<double, string> FormatterDate { get; set; }

    /// <summary>
    /// Gets or sets the formatter for the ping time axis.
    /// </summary>
    public Func<double, string> FormatterPingTime { get; set; }

    /// <summary>
    /// Gets or sets the series collection for the chart.
    /// </summary>
    public SeriesCollection Series { get; set; }

    private string _errorMessage;

    /// <summary>
    /// Gets the error message if an error occurs.
    /// </summary>
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

    /// <summary>
    /// Gets or sets a value indicating whether the error message is displayed.
    /// </summary>
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

    /// <summary>
    /// Gets or sets a value indicating whether the host view is expanded.
    /// </summary>
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

    /// <summary>
    /// Gets the command to start or stop the ping monitoring.
    /// </summary>
    public ICommand PingCommand => new RelayCommand(_ => PingAction());

    /// <summary>
    /// Action to start or stop the ping monitoring.
    /// </summary>
    private void PingAction()
    {
        if (IsRunning)
            Stop();
        else
            Start();
    }

    /// <summary>
    /// Gets the command to close the monitor and remove the host.
    /// </summary>
    public ICommand CloseCommand => new RelayCommand(_ => CloseAction());

    private void CloseAction()
    {
        _removeHostByGuid(HostId);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Starts the ping monitoring.
    /// </summary>
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

    /// <summary>
    /// Stops the ping monitoring.
    /// </summary>
    public void Stop()
    {
        if (!IsRunning)
            return;

        _cancellationTokenSource?.Cancel();
    }

    /// <summary>
    /// Resets the time chart with empty values.
    /// </summary>
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

    /// <summary>
    /// Exports the ping monitoring results to a file.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task Export()
    {
        var childWindow = new ExportChildWindow();

        var childWindowViewModel = new ExportViewModel(async instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            try
            {
                ExportManager.Export(instance.FilePath, instance.FileType,
                    new ObservableCollection<PingInfo>(_pingInfoList));
            }
            catch (Exception ex)
            {
                Log.Error("Error while exporting data as " + instance.FileType, ex);

                var settings = AppearanceManager.MetroDialog;
                settings.AffirmativeButtonText = Strings.OK;

                await _dialogCoordinator.ShowMessageAsync(this, Strings.Error,
                    Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine +
                    Environment.NewLine + ex.Message, MessageDialogStyle.Affirmative, settings);
            }

            SettingsManager.Current.PingMonitor_ExportFileType = instance.FileType;
            SettingsManager.Current.PingMonitor_ExportFilePath = instance.FilePath;
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        }, [
            ExportFileType.Csv, ExportFileType.Xml, ExportFileType.Json
        ], false,
        SettingsManager.Current.PingMonitor_ExportFileType,
        SettingsManager.Current.PingMonitor_ExportFilePath);

        childWindow.Title = Strings.Export;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        return (Application.Current.MainWindow as MainWindow).ShowChildWindowAsync(childWindow);
    }

    #endregion

    #region Events

    /// <summary>
    /// Handles the PingReceived event. Updates statistics and the chart.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PingReceivedArgs"/> instance containing the event data.</param>
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

    /// <summary>
    /// Handles the UserHasCanceled event. Stops the monitoring.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void Ping_UserHasCanceled(object sender, EventArgs e)
    {
        IsRunning = false;
    }

    /// <summary>
    /// Handles the HostnameResolved event. Updates the hostname and title if necessary.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="HostnameArgs"/> instance containing the event data.</param>
    private void Ping_HostnameResolved(object sender, HostnameArgs e)
    {
        // Update title if name was not set in the constructor
        if (string.IsNullOrEmpty(Hostname))
            Title = $"{e.Hostname.TrimEnd('.')} # {IPAddress}";

        Hostname = e.Hostname;
    }

    /// <summary>
    /// Handles the PingException event. Stops the monitoring and displays the error.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PingExceptionArgs"/> instance containing the event data.</param>
    private void Ping_PingException(object sender, PingExceptionArgs e)
    {
        IsRunning = false;

        ErrorMessage = e.Message;
        IsErrorMessageDisplayed = true;
    }

    #endregion
}