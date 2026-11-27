using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using log4net;
using NETworkManager.Controls;
using NETworkManager.Localization;
using NETworkManager.Localization.Resources;
using NETworkManager.Models;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

/// <summary>
/// One target of the internet latency widget: resolves the host, pings it in a loop while the widget runs and keeps
/// the pings of the last minute for the value, the statistics and the chart.
/// </summary>
public class InternetLatencyTargetViewModel : ViewModelBase
{
    #region Variables

    private static readonly ILog Log = LogManager.GetLogger(typeof(InternetLatencyTargetViewModel));

    /// <summary>
    /// Pings in the time window (<see cref="GlobalStaticConfiguration.Dashboard_InternetLatencyWindow" />), the oldest
    /// first.
    /// </summary>
    private readonly List<LatencySample> _samples = [];

    /// <summary>
    /// Values of the chart (round-trip time, <c>null</c> for a lost ping: a gap in the line).
    /// </summary>
    private readonly ObservableCollection<DateTimePoint> _chartValues = [];

    /// <summary>
    /// X axis of the chart, the time window up to now.
    /// </summary>
    private readonly DateTimeAxis _xAxis = new(TimeSpan.FromSeconds(1), _ => string.Empty) { IsVisible = false };

    /// <summary>
    /// Gets the name of the target (the host if no name is configured).
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the host name or IP address of the target.
    /// </summary>
    public string Host { get; }

    /// <summary>
    /// Gets the pinged IP address (resolved from the host), <c>null</c> before it is resolved.
    /// </summary>
    public string ResolvedIPAddress
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
    /// Gets the value shown in the widget: the round-trip time of the last ping, the status of a failed ping or
    /// <c>-/-</c>.
    /// </summary>
    public string Value
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = "-/-";

    /// <summary>
    /// Gets the status of the target (dot in front of the name).
    /// </summary>
    public StatusLevel StatusLevel
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
    /// Gets the error why the target is not pinged (the host name could not be resolved), <c>null</c> otherwise.
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
    /// Gets the statistics of the pings in the time window (tooltip).
    /// </summary>
    public LatencyStatisticsInfo Statistics
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = LatencyStatistics.Calculate([]);

    /// <summary>
    /// Gets the series of the chart.
    /// </summary>
    public ISeries[] Series { get; }

    /// <summary>
    /// Gets the X axis of the chart.
    /// </summary>
    public ICartesianAxis[] XAxes { get; }

    /// <summary>
    /// Gets the Y axis of the chart (from 0 ms).
    /// </summary>
    public ICartesianAxis[] YAxes { get; } = [new Axis { IsVisible = false, MinLimit = 0 }];

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="InternetLatencyTargetViewModel" /> class.
    /// </summary>
    /// <param name="name">Name of the target, empty to show the host.</param>
    /// <param name="host">Host name or IP address of the target.</param>
    public InternetLatencyTargetViewModel(string name, string host)
    {
        Host = host;
        Name = string.IsNullOrWhiteSpace(name) ? host : name.Trim();

        XAxes = [_xAxis];

        Series =
        [
            new LineSeries<DateTimePoint>
            {
                Name = Name,
                Values = _chartValues,
                GeometrySize = 0,
                LineSmoothness = 0.3,
                DataPadding = new LvcPoint(0, 0),
                Stroke = ChartColors.LineStroke(ChartColors.Primary),
                Fill = ChartColors.AreaFill(ChartColors.Primary)
            }
        ];

        UpdateXAxis(DateTime.Now);
    }

    #endregion

    #region ICommands & Actions

    /// <summary>
    /// Gets the command to open the host in another tool (context menu, parameter: <see cref="ApplicationName" />).
    /// </summary>
    public ICommand OpenCommand => new RelayCommand(parameter =>
    {
        if (parameter is ApplicationName application)
            NavigationManager.Open(application, Host);
    });

    /// <summary>
    /// Gets the command to copy the host.
    /// </summary>
    public ICommand CopyCommand => new RelayCommand(_ => ClipboardHelper.SetClipboard(Host));

    #endregion

    #region Methods

    /// <summary>
    /// Resolves the host and pings it until the token is canceled. The pings of an earlier run are removed (no
    /// history while the widget doesn't run).
    /// </summary>
    /// <param name="cancellationToken">Stops the pings (dashboard hidden, check disabled, network changed).</param>
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        Clear();

        try
        {
            var ipAddress = await ResolveAsync(cancellationToken);

            if (ipAddress == null)
                return;

            var buffer = new byte[GlobalStaticConfiguration.Dashboard_PingBuffer];

            while (!cancellationToken.IsCancellationRequested)
            {
                // Continues on the UI thread (the values are bound)
                var pingInfo = await PingProbe.PingAsync(ipAddress, GlobalStaticConfiguration.Dashboard_PingTimeout, 1,
                    buffer, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    return;

                AddSample(new LatencySample(DateTime.Now, pingInfo.Status == System.Net.NetworkInformation.IPStatus.Success,
                    pingInfo.Time), pingInfo.Status);

                await Task.Delay(TimeSpan.FromSeconds(SettingsManager.Current.Dashboard_InternetLatencyInterval),
                    cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Stopped
        }
    }

    /// <summary>
    /// Resolves the host name with the DNS settings of the application (an IP address is used as it is).
    /// </summary>
    /// <returns>IP address, <c>null</c> if the host name could not be resolved (the error is shown).</returns>
    private async Task<IPAddress> ResolveAsync(CancellationToken cancellationToken)
    {
        if (IPAddress.TryParse(Host, out var ipAddress))
        {
            ResolvedIPAddress = ipAddress.ToString();
            return ipAddress;
        }

        var dnsResult = await DNSClientHelper.ResolveAorAaaaAsync(Host,
            SettingsManager.Current.Network_ResolveHostnamePreferIPv4);

        cancellationToken.ThrowIfCancellationRequested();

        if (dnsResult.HasError)
        {
            // Checked again with the next start (e.g. a network change or when the dashboard is shown again)
            ErrorMessage = DNSClientHelper.FormatDNSClientResultError(Host, dnsResult);
            StatusLevel = StatusLevel.Error;

            Log.Warn($"Internet latency: {ErrorMessage}");

            return null;
        }

        ResolvedIPAddress = dnsResult.Value.ToString();

        return dnsResult.Value;
    }

    /// <summary>
    /// Adds a ping: updates the value, the status, the statistics and the chart.
    /// </summary>
    /// <param name="sample">Ping.</param>
    /// <param name="status">Status of the ping (shown if it failed).</param>
    private void AddSample(LatencySample sample, System.Net.NetworkInformation.IPStatus status)
    {
        var window = GlobalStaticConfiguration.Dashboard_InternetLatencyWindow;

        _samples.Add(sample);
        LatencyStatistics.RemoveOutdated(_samples, sample.Timestamp, window);

        _chartValues.Add(new DateTimePoint(sample.Timestamp, sample.IsSuccess ? sample.Time : null));

        // Keep one point before the window, so the line starts at the left edge
        while (_chartValues.Count > 1 && _chartValues[1].DateTime < sample.Timestamp - window)
            _chartValues.RemoveAt(0);

        UpdateXAxis(sample.Timestamp);

        Value = sample.IsSuccess
            ? FormatTime(sample.Time)
            : ResourceTranslator.Translate(ResourceIdentifier.IPStatus, status);

        StatusLevel = LatencyStatistics.GetState(_samples) switch
        {
            LatencyState.Ok => StatusLevel.Success,
            LatencyState.Lost => StatusLevel.Warning,
            LatencyState.Failed => StatusLevel.Error,
            _ => StatusLevel.None
        };

        Statistics = LatencyStatistics.Calculate(_samples);
    }

    /// <summary>
    /// Shows samples without pinging (screenshot mode).
    /// </summary>
    /// <param name="samples">Pings, the oldest first.</param>
    public void SetSamples(IEnumerable<LatencySample> samples)
    {
        Clear();

        ResolvedIPAddress = Host;

        foreach (var sample in samples)
            AddSample(sample, sample.IsSuccess
                ? System.Net.NetworkInformation.IPStatus.Success
                : System.Net.NetworkInformation.IPStatus.TimedOut);
    }

    /// <summary>
    /// Applies the chart colors of the current theme.
    /// </summary>
    public void ApplyChartColors()
    {
        if (Series[0] is not LineSeries<DateTimePoint> series)
            return;

        series.Stroke = ChartColors.LineStroke(ChartColors.Primary);
        series.Fill = ChartColors.AreaFill(ChartColors.Primary);
    }

    /// <summary>
    /// Text of the copy all button: name, host, IP address, last value and statistics.
    /// </summary>
    public string ToCopyText()
    {
        var host = ResolvedIPAddress == null || ResolvedIPAddress == Host ? Host : $"{Host}, {ResolvedIPAddress}";
        var value = ErrorMessage ?? Value;
        var stats = Statistics;

        return stats.Count == 0
            ? $"{Name} ({host}): {value}"
            : $"{Name} ({host}): {value} - {Strings.Minimum} {FormatTime(stats.Minimum)}, " +
              $"{Strings.AverageTime} {FormatTime(stats.Average)}, " +
              $"{Strings.Maximum} {FormatTime(stats.Maximum)}, " +
              $"{Strings.PacketLoss} {stats.PacketLoss.ToString("F0", CultureInfo.CurrentCulture)} %";
    }

    private static string FormatTime(double? milliseconds) =>
        milliseconds.HasValue ? $"{Math.Round(milliseconds.Value).ToString(CultureInfo.CurrentCulture)} ms" : "-/-";

    /// <summary>
    /// Moves the X axis to the time window up to <paramref name="now" />.
    /// </summary>
    private void UpdateXAxis(DateTime now)
    {
        _xAxis.MinLimit = (now - GlobalStaticConfiguration.Dashboard_InternetLatencyWindow).Ticks;
        _xAxis.MaxLimit = now.Ticks;
    }

    /// <summary>
    /// Removes the pings, the value, the status and the error.
    /// </summary>
    private void Clear()
    {
        _samples.Clear();
        _chartValues.Clear();
        UpdateXAxis(DateTime.Now);

        Value = "-/-";
        StatusLevel = StatusLevel.None;
        ErrorMessage = null;
        Statistics = LatencyStatistics.Calculate([]);
    }

    #endregion
}
