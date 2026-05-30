using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows.Threading;

namespace NETworkManager.Models.Network;

public class BandwidthMeter
{
    #region Constructor

    public BandwidthMeter(string id)
    {
        _timer.Interval = TimeSpan.FromMilliseconds(UpdateInterval);
        _timer.Tick += Timer_Tick;

        _networkInterface = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
            .FirstOrDefault(x => x.Id == id);
    }

    #endregion

    #region Events

    private void Timer_Tick(object sender, EventArgs e)
    {
        Update();
    }

    #endregion

    #region Variables

    /// <summary>
    /// The default sample interval in milliseconds. Kept as a constant because the speed
    /// calculation and the consuming view model derive timing from it.
    /// </summary>
    public const double DefaultUpdateInterval = 1000;

    public double UpdateInterval
    {
        get;
        set
        {
            if (value == field)
                return;

            _timer.Interval = TimeSpan.FromMilliseconds(value);

            field = value;
        }
    } = DefaultUpdateInterval;

    public bool IsRunning => _timer.IsEnabled;

    private readonly DispatcherTimer _timer = new();
    private readonly System.Net.NetworkInformation.NetworkInterface _networkInterface;
    private readonly Stopwatch _stopwatch = new();
    private long _previousBytesSent;
    private long _previousBytesReceived;
    private bool _canUpdate; // Collect initial data for correct calculation

    #endregion

    #region Public events

    public event EventHandler<BandwidthMeterSpeedArgs> UpdateSpeed;

    protected virtual void OnUpdateSpeed(BandwidthMeterSpeedArgs e)
    {
        UpdateSpeed?.Invoke(this, e);
    }

    #endregion

    #region Methods

    public void Start()
    {
        _timer.Start();
    }

    public void Stop()
    {
        _timer.Stop();

        // Reset, so the next sample re-seeds the baseline (avoids a speed spike after a pause).
        _canUpdate = false;
    }

    private void Update()
    {
        // The interface may have been removed/disabled after this meter was created.
        if (_networkInterface == null)
            return;

        IPInterfaceStatistics stats;

        try
        {
            // IPStatistics covers both IPv4 and IPv6 traffic on the interface.
            stats = _networkInterface.GetIPStatistics();
        }
        catch (NetworkInformationException)
        {
            // Transient failure (e.g. adapter going down) - skip this tick.
            return;
        }

        var totalBytesSent = stats.BytesSent;
        var totalBytesReceived = stats.BytesReceived;

        // First sample after start/resume: seed the baseline and start timing, no speed yet.
        if (!_canUpdate)
        {
            _previousBytesSent = totalBytesSent;
            _previousBytesReceived = totalBytesReceived;
            _stopwatch.Restart();
            _canUpdate = true;

            return;
        }

        var elapsedSeconds = _stopwatch.Elapsed.TotalSeconds;
        _stopwatch.Restart();

        // Clamp negative deltas: cumulative counters can reset/wrap (driver reset, disable/enable).
        var deltaSent = Math.Max(0, totalBytesSent - _previousBytesSent);
        var deltaReceived = Math.Max(0, totalBytesReceived - _previousBytesReceived);

        // Derive a true per-second rate from the measured elapsed time (robust against timer jitter).
        var byteSentSpeed = elapsedSeconds > 0 ? (long)(deltaSent / elapsedSeconds) : 0;
        var byteReceivedSpeed = elapsedSeconds > 0 ? (long)(deltaReceived / elapsedSeconds) : 0;

        _previousBytesSent = totalBytesSent;
        _previousBytesReceived = totalBytesReceived;

        OnUpdateSpeed(new BandwidthMeterSpeedArgs(DateTime.Now, totalBytesReceived, totalBytesSent,
            byteReceivedSpeed, byteSentSpeed));
    }

    #endregion
}
