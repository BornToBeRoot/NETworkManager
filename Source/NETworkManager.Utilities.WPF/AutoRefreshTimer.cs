using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace NETworkManager.Utilities.WPF;

/// <summary>
///     Refreshes a view automatically (e.g. "Automatically update every" in Connections). The interval is the pause
///     after a refresh: the timer is stopped while refreshing, so refreshes never overlap. Paused while the view is
///     hidden (see <see cref="Pause" />). Provides the time until the next refresh for a countdown
///     (<see cref="RemainingTime" />).
/// </summary>
public sealed class AutoRefreshTimer : INotifyPropertyChanged
{
    private readonly DispatcherTimer _timer = new();
    private readonly DispatcherTimer _countdownTimer = new() { Interval = TimeSpan.FromSeconds(1) };
    private readonly Func<Task> _refresh;
    private readonly Func<bool> _canRefresh;

    private bool _isPaused;
    private bool _isRefreshing;

    /// <summary>
    ///     Creates a new auto refresh timer (disabled). Must be created on the UI thread.
    /// </summary>
    /// <param name="refresh">Refreshes the view.</param>
    /// <param name="canRefresh">
    ///     Optional: false skips a refresh (e.g. a manual refresh is running), it is tried again after the interval.
    /// </param>
    public AutoRefreshTimer(Func<Task> refresh, Func<bool> canRefresh = null)
    {
        _refresh = refresh;
        _canRefresh = canRefresh;

        _timer.Tick += Timer_Tick;
        _countdownTimer.Tick += (_, _) => UpdateRemainingTime();
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the view is refreshed automatically. Enabling starts the interval.
    /// </summary>
    public bool IsEnabled
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();

            Restart();
        }
    }

    /// <summary>
    ///     Gets or sets the time between the end of a refresh and the next one. A change starts the interval again.
    /// </summary>
    public TimeSpan Interval
    {
        get => _timer.Interval;
        set
        {
            if (value == _timer.Interval)
                return;

            _timer.Interval = value;
            OnPropertyChanged();

            Restart();
        }
    }

    /// <summary>
    ///     Gets the time of the next refresh, null if no refresh is planned (disabled, paused or refreshing).
    /// </summary>
    public DateTime? NextRefresh
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
    ///     Gets the time until the next refresh (updated every second, for a countdown), null if no refresh is planned.
    /// </summary>
    public TimeSpan? RemainingTime
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

    /// <inheritdoc />
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    ///     Pauses the timer (the view was hidden). <see cref="Resume" /> starts the interval again.
    /// </summary>
    public void Pause()
    {
        _isPaused = true;

        Restart();
    }

    /// <summary>
    ///     Resumes the timer after <see cref="Pause" /> (the view is visible again) with the full interval.
    /// </summary>
    public void Resume()
    {
        _isPaused = false;

        Restart();
    }

    /// <summary>
    ///     Starts the interval again if a refresh should be planned (enabled, not paused, not refreshing), otherwise
    ///     stops the timer.
    /// </summary>
    private void Restart()
    {
        _timer.Stop();

        // While refreshing, the timer is started when the refresh is finished (see Timer_Tick)
        if (IsEnabled && !_isPaused && !_isRefreshing && _timer.Interval > TimeSpan.Zero)
        {
            _timer.Start();

            NextRefresh = DateTime.Now + _timer.Interval;
            _countdownTimer.Start();
        }
        else
        {
            NextRefresh = null;
            _countdownTimer.Stop();
        }

        UpdateRemainingTime();
    }

    private void UpdateRemainingTime()
    {
        if (NextRefresh is not { } nextRefresh)
        {
            RemainingTime = null;
            return;
        }

        // Rounded up to full seconds: the countdown shows 00:01 until the refresh starts, not 00:00
        var remaining = nextRefresh - DateTime.Now;

        RemainingTime = remaining > TimeSpan.Zero
            ? TimeSpan.FromSeconds(Math.Ceiling(remaining.TotalSeconds))
            : TimeSpan.Zero;
    }

    private async void Timer_Tick(object sender, EventArgs e)
    {
        // Try again after the interval (e.g. a manual refresh is running)
        if (_canRefresh?.Invoke() == false)
        {
            Restart();
            return;
        }

        _isRefreshing = true;
        Restart();

        try
        {
            await _refresh();
        }
        finally
        {
            _isRefreshing = false;

            // Unless auto refresh was disabled or the view was hidden in the meantime
            Restart();
        }
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
