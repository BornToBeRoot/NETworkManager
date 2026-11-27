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
/// One target of the latency widget: resolves the host, pings it in a loop while the widget runs and keeps
/// the pings of the last minute for the value, the statistics and the chart.
/// </summary>
public class LatencyTargetViewModel : ViewModelBase
{
    #region Variables

    private static readonly ILog Log = LogManager.GetLogger(typeof(LatencyTargetViewModel));


    // Range of the interval in seconds (Settings > Dashboard > Latency)
    public const int MinInterval = 1;
    public const int MaxInterval = 10;

    // All times are UTC: a clock change (daylight saving time, NTP correction) doesn't move the time window

    /// <summary>
    /// Pings in the time window (<see cref="GlobalStaticConfiguration.Dashboard_LatencyWindow" />), the oldest
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
    /// Gets the host name or IP address of the target ("Gateway" for the default gateway, see
    /// <see cref="ResolvedIPAddress" />).
    /// </summary>
    public string Host { get; }

    /// <summary>
    /// Gets the target of the settings (the widget keeps the target when the list changes and it is the same).
    /// </summary>
    public LatencyTargetInfo Info { get; }

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
    /// Initializes a new instance of the <see cref="LatencyTargetViewModel" /> class.
    /// </summary>
    /// <param name="info">Target of the settings (a host or the default gateway).</param>
    public LatencyTargetViewModel(LatencyTargetInfo info)
    {
        Info = info;
        Host = info.IsGateway ? Strings.Gateway : info.Host?.Trim();
        Name = string.IsNullOrWhiteSpace(info.Name) ? Host : info.Name.Trim();

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

        UpdateXAxis(DateTime.UtcNow);
    }

    #endregion

    #region ICommands & Actions

    /// <summary>
    /// Gets the command to open the host in another tool (context menu, parameter: <see cref="ApplicationName" />).
    /// </summary>
    public ICommand OpenCommand => new RelayCommand(parameter =>
    {
        if (parameter is ApplicationName application && Target != null)
            NavigationManager.Open(application, Target);
    }, _ => Target != null);

    /// <summary>
    /// Gets the command to copy the host.
    /// </summary>
    public ICommand CopyCommand => new RelayCommand(_ => ClipboardHelper.SetClipboard(Target), _ => Target != null);

    /// <summary>
    /// Host that is opened / copied: the host, or the detected IP address of the gateway (<c>null</c> before it is
    /// detected).
    /// </summary>
    private string Target => Info.IsGateway ? ResolvedIPAddress : Host;

    #endregion

    #region Methods

    /// <summary>
    /// Resolves the host and pings it until the token is canceled.
    /// </summary>
    /// <param name="keepHistory">Keep the pings of the last minute (resumed after a pause, e.g. another tool was
    ///     shown): the pause is a gap in the chart and doesn't count as lost pings. Otherwise they are removed (e.g.
    ///     after a network change the latencies of another route would be mixed).</param>
    /// <param name="cancellationToken">Stops the pings (dashboard hidden, check disabled, network changed).</param>
    public async Task RunAsync(bool keepHistory, CancellationToken cancellationToken)
    {
        if (keepHistory)
            ResumeHistory();
        else
            ClearHistory();

        try
        {
            var ipAddress = await ResolveAsync(cancellationToken);

            // Not resolved (e.g. the DNS server of a new network is not reachable yet right after a network change):
            // try again until it works, 5, 15, 30 s, then every minute (the error is shown meanwhile)
            for (var failedAttempts = 1; ipAddress == null; failedAttempts++)
            {
                await Task.Delay(LatencyStatistics.GetRetryDelay(failedAttempts), cancellationToken);

                ipAddress = await ResolveAsync(cancellationToken);
            }

            var buffer = new byte[GlobalStaticConfiguration.Dashboard_PingBuffer];

            while (!cancellationToken.IsCancellationRequested)
            {
                PingInfo pingInfo;

                try
                {
                    // Continues on the UI thread (the values are bound)
                    pingInfo = await PingProbe.PingAsync(ipAddress, GlobalStaticConfiguration.Dashboard_PingTimeout, 1,
                        buffer, cancellationToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    // An unexpected error is a failed ping: the loop goes on (the task is not awaited by anyone)
                    Log.Error($"Latency: ping of {ipAddress} failed.", ex);
                    pingInfo = new PingInfo(DateTime.UtcNow, ipAddress, System.Net.NetworkInformation.IPStatus.Unknown);
                }

                if (cancellationToken.IsCancellationRequested)
                    return;

                AddSample(new LatencySample(DateTime.UtcNow, pingInfo.Status == System.Net.NetworkInformation.IPStatus.Success,
                    pingInfo.Time), pingInfo.Status);

                // Range of the settings (a value edited in the settings file: no tight loop, no invalid delay)
                await Task.Delay(TimeSpan.FromSeconds(Math.Clamp(SettingsManager.Current.Dashboard_LatencyInterval,
                    MinInterval, MaxInterval)), cancellationToken);
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
        if (Info.IsGateway)
            return await DetectGatewayAsync(cancellationToken);

        if (IPAddress.TryParse(Host, out var ipAddress))
        {
            ResolvedIPAddress = ipAddress.ToString();
            return ipAddress;
        }

        string error;

        try
        {
            var dnsResult = await DNSClientHelper.ResolveAorAaaaAsync(Host,
                SettingsManager.Current.Network_ResolveHostnamePreferIPv4);

            cancellationToken.ThrowIfCancellationRequested();

            if (!dnsResult.HasError)
            {
                ErrorMessage = null;
                ResolvedIPAddress = dnsResult.Value.ToString();

                // Status from the pings of the time window again (not red until the next ping)
                UpdateStatistics();

                return dnsResult.Value;
            }

            error = DNSClientHelper.FormatDNSClientResultError(Host, dnsResult);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // E.g. the DNS client is not configured: an error like a failed lookup (the task is not awaited by anyone)
            Log.Error($"Latency: resolving {Host} failed.", ex);

            error = string.Format(Strings.CouldNotResolveIPAddressFor, Host);
        }

        // Resolved again later (see RunAsync); no IP address or value of an earlier run next to the error
        ErrorMessage = error;
        ResolvedIPAddress = null;
        Value = "-/-";
        StatusLevel = StatusLevel.Error;

        Log.Warn($"Latency: {ErrorMessage}");

        return null;
    }

    /// <summary>
    /// Detects the default gateway of the network adapter that is used for the internet (route to the public IPv4
    /// address of the network connection settings), like the network connection check.
    /// </summary>
    /// <returns>Gateway, <c>null</c> if there is none (e.g. a VPN without a gateway; shown as information, detected
    ///     again later).</returns>
    private async Task<IPAddress> DetectGatewayAsync(CancellationToken cancellationToken)
    {
        IPAddress gateway = null;

        try
        {
            var remote = IPAddress.TryParse(SettingsManager.Current.Dashboard_PublicIPv4Address, out var address)
                ? address
                : IPAddress.Parse(GlobalStaticConfiguration.Dashboard_PublicIPv4Address);

            gateway = await NetworkInterface.DetectGatewayBasedOnRoutingAsync(remote);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Log.Error("Latency: detecting the default gateway failed.", ex);
        }

        cancellationToken.ThrowIfCancellationRequested();

        if (gateway != null)
        {
            ErrorMessage = null;
            ResolvedIPAddress = gateway.ToString();
            UpdateStatistics();

            return gateway;
        }

        // Not an error of the target: gray, like the network connection without a gateway
        ErrorMessage = Strings.ConnectionCheckReason_NoGateway;
        ResolvedIPAddress = null;
        Value = "-/-";
        StatusLevel = StatusLevel.None;

        return null;
    }

    /// <summary>
    /// Adds a ping: updates the value, the status, the statistics and the chart.
    /// </summary>
    /// <param name="sample">Ping.</param>
    /// <param name="status">Status of the ping (shown if it failed).</param>
    private void AddSample(LatencySample sample, System.Net.NetworkInformation.IPStatus status)
    {
        _samples.Add(sample);
        _chartValues.Add(new DateTimePoint(sample.Timestamp, sample.IsSuccess ? sample.Time : null));

        TrimToWindow(sample.Timestamp);
        UpdateXAxis(sample.Timestamp);

        Value = sample.IsSuccess
            ? FormatTime(sample.Time)
            : ResourceTranslator.Translate(ResourceIdentifier.IPStatus, status);

        UpdateStatistics();
    }

    /// <summary>
    /// Continues after a pause: removes the pings older than the time window and ends the line of the chart at the
    /// last ping (a gap until the next one). The last value stays until the next ping, unless it is outside the
    /// window.
    /// </summary>
    private void ResumeHistory()
    {
        var now = DateTime.UtcNow;

        ErrorMessage = null;

        TrimToWindow(now);

        if (_chartValues.Count > 0 && _chartValues[^1].Value != null)
            _chartValues.Add(new DateTimePoint(_chartValues[^1].DateTime.AddMilliseconds(1), null));

        UpdateXAxis(now);

        if (_samples.Count == 0)
            Value = "-/-";

        UpdateStatistics();
    }

    /// <summary>
    /// Removes the pings and chart points older than the time window.
    /// </summary>
    private void TrimToWindow(DateTime now)
    {
        var window = GlobalStaticConfiguration.Dashboard_LatencyWindow;

        LatencyStatistics.RemoveOutdated(_samples, now, window);

        // Keep one point before the window, so the line starts at the left edge
        while (_chartValues.Count > 1 && _chartValues[1].DateTime < now - window)
            _chartValues.RemoveAt(0);
    }

    /// <summary>
    /// Updates the status dot and the statistics from the pings in the time window.
    /// </summary>
    private void UpdateStatistics()
    {
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
        ClearHistory();

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

    // Like the tooltip ("F0": halves are rounded up, not to the even number like Math.Round)
    private static string FormatTime(double? milliseconds) =>
        milliseconds.HasValue ? $"{milliseconds.Value.ToString("F0", CultureInfo.CurrentCulture)} ms" : "-/-";

    /// <summary>
    /// Moves the X axis to the time window up to <paramref name="now" />.
    /// </summary>
    private void UpdateXAxis(DateTime now)
    {
        _xAxis.MinLimit = (now - GlobalStaticConfiguration.Dashboard_LatencyWindow).Ticks;
        _xAxis.MaxLimit = now.Ticks;
    }

    /// <summary>
    /// Removes the pings, the value, the status and the error (e.g. the check was disabled).
    /// </summary>
    public void ClearHistory()
    {
        _samples.Clear();
        _chartValues.Clear();
        UpdateXAxis(DateTime.UtcNow);

        Value = "-/-";
        StatusLevel = StatusLevel.None;
        ErrorMessage = null;
        ResolvedIPAddress = null;
        Statistics = LatencyStatistics.Calculate([]);
    }

    #endregion
}
