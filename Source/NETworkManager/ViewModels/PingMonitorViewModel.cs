using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using log4net;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Export;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    /// <param name="hostId">The unique identifier for the host.</param>
    /// <param name="removeHostByGuid">Action to remove the host by its GUID.</param>
    /// <param name="host">Tuple containing the IP address and hostname.</param>
    /// <param name="group">The group name the host belongs to.</param>
    public PingMonitorViewModel(Guid hostId, Action<Guid> removeHostByGuid,
        (IPAddress ipAddress, string hostname) host, string group)
    {
        HostId = hostId;
        _removeHostByGuid = removeHostByGuid;

        Title = string.IsNullOrEmpty(host.hostname) ? host.ipAddress.ToString() : $"{host.hostname} # {host.ipAddress}";

        IPAddress = host.ipAddress;
        Hostname = host.hostname;
        Group = group;

        InitialTimeChart();

        ExpandHostView = SettingsManager.Current.PingMonitor_ExpandHostView;

        SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;
    }

    #endregion

    #region Variables
    private static readonly ILog Log = LogManager.GetLogger(typeof(PingMonitorViewModel));

    private CancellationTokenSource _cancellationTokenSource;
    private int _maxPingValues;
    private int _sessionId;
    private bool _updatingAxisFromCode;

    public readonly Guid HostId;
    private readonly Action<Guid> _removeHostByGuid;

    private List<PingInfo> _pingInfoList = [];

    /// <summary>
    /// Gets the title of the monitor, typically "Hostname # IP".
    /// </summary>
    public string Title
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the hostname of the monitored host.
    /// </summary>
    public string Hostname
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the IP address of the monitored host.
    /// </summary>
    public IPAddress IPAddress
    {
        get;
        private init
        {
            if (Equals(value, field))
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the group the monitored host belongs to.
    /// </summary>
    public string Group
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the ping monitoring is currently running.
    /// </summary>
    public bool IsRunning
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the host is reachable (responds to ping).
    /// </summary>
    public bool IsReachable
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the time of the last status update.
    /// </summary>
    public DateTime StatusTime
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the total number of ping packets transmitted.
    /// </summary>
    public int Transmitted
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the total number of ping packets received.
    /// </summary>
    public int Received
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the total number of ping packets lost.
    /// </summary>
    public int Lost
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the percentage of packet loss.
    /// </summary>
    public double PacketLoss
    {
        get;
        set
        {
            if (Math.Abs(value - field) < 0.01)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the round-trip time in milliseconds of the last ping.
    /// </summary>
    public long TimeMs
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<LvlChartsDefaultInfo> _pingValues;

    /// <summary>
    /// Initializes the time chart configuration.
    /// </summary>
    private void InitialTimeChart()
    {
        _pingValues = [];

        var chartColor = SKColor.Parse("#1ba1e2");

        var labelColor = Application.Current?.TryFindResource("MahApps.Brushes.Gray5") is System.Windows.Media.SolidColorBrush gray5
            ? new SKColor(gray5.Color.R, gray5.Color.G, gray5.Color.B, gray5.Color.A)
            : new SKColor(0x68, 0x68, 0x68);

        var separatorColor = Application.Current?.TryFindResource("MahApps.Brushes.Gray8") is System.Windows.Media.SolidColorBrush gray8
            ? new SKColor(gray8.Color.R, gray8.Color.G, gray8.Color.B, gray8.Color.A)
            : new SKColor(0x80, 0x80, 0x80);

        Series =
        [
            new LineSeries<LvlChartsDefaultInfo>
            {
                Name = Strings.Time,
                Values = _pingValues,
                Mapping = (info, _) => double.IsNaN(info.Value)
                    ? Coordinate.Empty
                    : new((info.DateTime - _sessionStartTime).TotalSeconds, info.Value),
                GeometrySize = 0,
                LineSmoothness = 0.3,
                DataPadding = new LvcPoint(0, 0),
                Stroke = new SolidColorPaint(chartColor) { StrokeThickness = 1.5f },
                Fill = new SolidColorPaint(chartColor.WithAlpha(0x33))
            }
        ];

        PingXAxes =
        [
            new Axis
            {
                Labeler = value => DateTimeHelper.DateTimeToTimeString(
                    _sessionStartTime.AddSeconds(value)),
                TextSize = 10,
                Padding = new Padding(0, 4, 0, 0),
                LabelsPaint = new SolidColorPaint(labelColor),
                SeparatorsPaint = new SolidColorPaint(separatorColor)
                {
                    StrokeThickness = 1,
                    PathEffect = new DashEffect([10f, 10f])
                },
                MinStep = SettingsManager.Current.PingMonitor_ChartTime / 4.0,
                ForceStepToMin = true
            }
        ];

        PingYAxes =
        [
            new Axis
            {
                MinLimit = 0,
                MinStep = 25,
                ForceStepToMin = true,
                Labeler = value => $"{value} ms",
                TextSize = 11,
                Padding = new Padding(4, 0),
                LabelsPaint = new SolidColorPaint(labelColor),
                SeparatorsPaint = new SolidColorPaint(separatorColor)
                {
                    StrokeThickness = 1,
                    PathEffect = new DashEffect([10f, 10f])
                }
            }
        ];

        PingXAxes[0].PropertyChanged += (_, args) =>
        {
            if (_updatingAxisFromCode)
                return;

            if (args.PropertyName is not (nameof(Axis.MinLimit) or nameof(Axis.MaxLimit)))
                return;

            IsLiveMode = false;

            var axis = PingXAxes[0];
            if (axis.MinLimit.HasValue && axis.MaxLimit.HasValue)
            {
                _updatingAxisFromCode = true;
                axis.MinStep = (axis.MaxLimit.Value - axis.MinLimit.Value) / 4.0;
                _updatingAxisFromCode = false;
            }
        };
    }

    /// <summary>
    /// Gets or sets the series collection for the chart.
    /// </summary>
    public ISeries[] Series { get; private set; }

    /// <summary>
    /// Gets the X-axes configuration for the ping time chart.
    /// </summary>
    public Axis[] PingXAxes { get; private set; }

    /// <summary>
    /// Gets the Y-axes configuration for the ping time chart.
    /// </summary>
    public Axis[] PingYAxes { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether the chart is in live (auto-scrolling) mode.
    /// </summary>
    public bool IsLiveMode
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the error message if an error occurs.
    /// </summary>
    public string ErrorMessage
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the error message is displayed.
    /// </summary>
    public bool IsErrorMessageDisplayed
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the host view is expanded.
    /// </summary>
    public bool ExpandHostView
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
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

    /// <summary>
    /// Gets the command to export the ping monitoring results of this host.
    /// </summary>
    public ICommand ExportCommand => new RelayCommand(_ => ExportAction());

    private void ExportAction()
    {
        _ = Export();
    }

    /// <summary>
    /// Gets the command to return the chart to live (auto-scrolling) mode.
    /// </summary>
    public ICommand GoLiveCommand => new RelayCommand(_ => GoLiveAction());

    private void GoLiveAction()
    {
        IsLiveMode = true;

        // Samples received while inspecting were not added to the chart, so rebuild the
        // rolling buffer from the most recent history to resume at the current time.
        var recent = _pingInfoList
            .Skip(Math.Max(0, _pingInfoList.Count - _maxPingValues))
            .Select(info => new LvlChartsDefaultInfo(
                info.Timestamp, info.Status == IPStatus.Success ? info.Time : double.NaN));

        _pingValues = new ObservableCollection<LvlChartsDefaultInfo>(recent);
        ((LineSeries<LvlChartsDefaultInfo>)Series[0]).Values = _pingValues;

        UpdateXAxisWindow(DateTime.Now);
        UpdateYAxis();
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
        IsLiveMode = true;

        _sessionId++;

        // Reset history
        _pingInfoList = [];

        // Reset the latest results
        StatusTime = DateTime.Now;
        Transmitted = 0;
        Received = 0;
        Lost = 0;
        PacketLoss = 0;

        // Reset chart
        _sessionStartTime = DateTime.Now;
        ResetTimeChart();

        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        UpdateMaxPingValues();

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

    private DateTime _sessionStartTime;

    private void ResetTimeChart()
    {
        if (_pingValues == null)
            return;

        _pingValues.Clear();
        UpdateXAxisWindow(DateTime.Now);
    }

    private void UpdateXAxisWindow(DateTime now)
    {
        _updatingAxisFromCode = true;
        var axis = PingXAxes[0];
        var chartTime = SettingsManager.Current.PingMonitor_ChartTime;
        var elapsed = (now - _sessionStartTime).TotalSeconds;
        axis.MinStep = chartTime / 4.0;
        axis.MinLimit = elapsed - chartTime;
        axis.MaxLimit = elapsed;
        _updatingAxisFromCode = false;
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

                await DialogHelper.ShowMessageAsync(Application.Current.MainWindow, Strings.Error,
                   Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine +
                   Environment.NewLine + ex.Message, ChildWindowIcon.Error);
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

        return Application.Current.MainWindow.ShowChildWindowAsync(childWindow);
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
        var capturedSession = _sessionId;
        Application.Current?.Dispatcher.BeginInvoke(DispatcherPriority.Normal, () =>
        {
            if (_sessionId != capturedSession)
                return;

            // While the user is inspecting the chart (zoomed/panned, i.e. not live), keep
            // the chart frozen: ignore new samples here so the view does not move and the
            // buffer does not grow. The sample is still recorded in the history below and
            // becomes visible again when the user returns to live mode (see GoLiveAction).
            if (!IsLiveMode)
                return;

            _pingValues.Add(timeInfo);
            if (_pingValues.Count > _maxPingValues)
                _pingValues.RemoveAt(0);

            UpdateXAxisWindow(timeInfo.DateTime);
            UpdateYAxis();
        });

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

    /// <summary>
    /// Unsubscribes from global settings changes. Must be called when the host is removed,
    /// so this transient view model is not kept alive by the long-lived settings singleton.
    /// </summary>
    public void Cleanup()
    {
        SettingsManager.Current.PropertyChanged -= SettingsManager_PropertyChanged;
    }

    private void SettingsManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SettingsInfo.PingMonitor_ChartTime):
                UpdateMaxPingValues();
                if (IsRunning && IsLiveMode)
                    UpdateXAxisWindow(DateTime.Now);
                break;
            case nameof(SettingsInfo.PingMonitor_WaitTime):
                UpdateMaxPingValues();
                break;
        }
    }

    /// <summary>
    /// Extra share of samples kept beyond the visible window so the chart has a little
    /// off-screen history (smoother left edge while live + minor pan-back headroom).
    /// </summary>
    private const double PingValuesHeadroom = 1.1;

    private void UpdateMaxPingValues()
    {
        var visibleValues = (double)SettingsManager.Current.PingMonitor_ChartTime * 1000 /
                            SettingsManager.Current.PingMonitor_WaitTime;

        _maxPingValues = (int)Math.Ceiling(visibleValues * PingValuesHeadroom);
    }

    /// <summary>
    /// Rescales the Y axis to the current values. The step is derived from a 20% padded
    /// max and MaxLimit is set to step * 3, so the 4th label lands exactly on MaxLimit and
    /// is never cut off by rounding.
    /// </summary>
    private void UpdateYAxis()
    {
        var maxVal = _pingValues.Where(p => !double.IsNaN(p.Value)).Select(p => p.Value).DefaultIfEmpty(0).Max();
        if (!(maxVal > 0))
            return;

        var yAxis = PingYAxes[0];
        var step = Math.Ceiling(maxVal * 1.2 / 3.0);
        yAxis.MinStep = step;
        yAxis.MaxLimit = step * 3;
    }

    #endregion
}
