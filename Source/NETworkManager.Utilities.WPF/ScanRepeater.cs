using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NETworkManager.Utilities.WPF;

/// <summary>
///     Repeats a scan automatically ("Automatically scan every" in the IP Scanner and Port Scanner): after a completed
///     scan of the input, the next scan starts after the interval (the pause after a scan, the timer waits until the
///     scan it started is finished). A manual scan or a changed input is not repeated until it was scanned (see
///     <see cref="Disarm" />). The checkbox is per tab (not saved), the interval is saved.
/// </summary>
public sealed class ScanRepeater : INotifyPropertyChanged
{
    private readonly Func<Task> _startRepeatedScan;
    private readonly Func<bool> _isScanRunning;
    private readonly Action<AutoRefreshTimeInfo> _saveInterval;

    // The last scan of the input was completed - it is repeated (if enabled)
    private bool _isArmed;
    private bool _isClosed;

    // The input was changed while a scan was running: that scan is not repeated (the next one would scan the new
    // input without its confirmation, mixed with the rows of the old input)
    private bool _isInputChangedDuringScan;

    // Completed by OnScanEnded, so the timer waits for the end of the scan it started
    private TaskCompletionSource _scanCompletion;

    /// <summary>
    ///     Creates a new scan repeater (disabled). Must be created on the UI thread.
    /// </summary>
    /// <param name="startRepeatedScan">Starts the repeated scan (returns when it is running or didn't start).</param>
    /// <param name="isScanRunning">Checks if a scan (or rescan) is running - no repeated scan then.</param>
    /// <param name="savedInterval">Saved interval, the first interval if it is not in the list.</param>
    /// <param name="saveInterval">Saves a changed interval.</param>
    public ScanRepeater(Func<Task> startRepeatedScan, Func<bool> isScanRunning, AutoRefreshTimeInfo savedInterval,
        Action<AutoRefreshTimeInfo> saveInterval)
    {
        _startRepeatedScan = startRepeatedScan;
        _isScanRunning = isScanRunning;

        Timer = new AutoRefreshTimer(RepeatAsync, () => !_isScanRunning());

        Times = CollectionViewSource.GetDefaultView(AutoRefreshTime.GetScanDefaults);

        var times = Times.SourceCollection.Cast<AutoRefreshTimeInfo>().ToList();

        // Not in the list (e.g. a removed interval of an old settings file): the first one
        SelectedTime = times.FirstOrDefault(x =>
            x.Value == savedInterval?.Value && x.TimeUnit == savedInterval?.TimeUnit) ?? times.First();

        // Set after the first interval, so loading doesn't save it
        _saveInterval = saveInterval;
    }

    /// <summary>
    ///     Gets the timer, e.g. for the countdown to the next scan (<see cref="AutoRefreshTimer.RemainingTime" />).
    /// </summary>
    public AutoRefreshTimer Timer { get; }

    /// <summary>
    ///     Gets the intervals to choose from.
    /// </summary>
    public ICollectionView Times { get; }

    /// <summary>
    ///     Gets or sets a value indicating whether the scan is repeated (checkbox, per tab and not saved). Starts with
    ///     the next completed scan.
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

            Update();
        }
    }

    /// <summary>
    ///     Gets or sets the interval (the pause after a scan, saved).
    /// </summary>
    public AutoRefreshTimeInfo SelectedTime
    {
        get;
        set
        {
            if (value == field || value == null)
                return;

            field = value;
            OnPropertyChanged();

            Timer.Interval = AutoRefreshTime.CalculateTimeSpan(value);
            _saveInterval?.Invoke(value);
        }
    }

    /// <summary>
    ///     Stops repeating until the next completed scan: a manual scan is started.
    /// </summary>
    public void Disarm()
    {
        _isArmed = false;
        _isInputChangedDuringScan = false;

        Update();
    }

    /// <summary>
    ///     The input was changed (e.g. typed or selected from the history, also while a scan is running): not repeated
    ///     until it was scanned.
    /// </summary>
    public void OnInputChanged()
    {
        _isArmed = false;

        if (_isScanRunning())
            _isInputChangedDuringScan = true;

        Update();
    }

    /// <summary>
    ///     A scan ended (not a rescan of a part of the result). A completed scan is repeated; a repeated scan that
    ///     failed (e.g. a hostname could not be resolved once) is tried again after the interval, so a short network
    ///     problem doesn't end the monitoring. A canceled scan or a failed manual scan is not repeated.
    /// </summary>
    /// <param name="isCompleted">The scan of the input was completed (not canceled, no error).</param>
    /// <param name="isCanceled">The scan was canceled by the user.</param>
    public void OnScanEnded(bool isCompleted, bool isCanceled)
    {
        var isRepeatedScan = _scanCompletion != null;

        _isArmed = !_isInputChangedDuringScan && !isCanceled && (isCompleted || (isRepeatedScan && _isArmed));
        _isInputChangedDuringScan = false;

        Update();

        // The timer waits for the end of the scan (see RepeatAsync)
        _scanCompletion?.TrySetResult();
        _scanCompletion = null;
    }

    /// <summary>
    ///     The tab is closed: no repeated scan in the background.
    /// </summary>
    public void Close()
    {
        _isClosed = true;

        Update();

        _scanCompletion?.TrySetResult();
        _scanCompletion = null;
    }

    private void Update()
    {
        Timer.IsEnabled = IsEnabled && _isArmed && !_isClosed;
    }

    /// <summary>
    ///     Starts the repeated scan (tick of the timer) and waits until it is finished, so the interval starts after it.
    /// </summary>
    private async Task RepeatAsync()
    {
        var scanCompletion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _scanCompletion = scanCompletion;

        await _startRepeatedScan();

        // Not started (e.g. no probe enabled, a port profile was deleted - the message is shown): tried again after the
        // interval, like a failed scan (see OnScanEnded)
        if (!_isScanRunning() && !scanCompletion.Task.IsCompleted)
        {
            scanCompletion.TrySetResult();
            _scanCompletion = null;
        }

        await scanCompletion.Task;
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
