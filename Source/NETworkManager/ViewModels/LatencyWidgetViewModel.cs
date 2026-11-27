using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

/// <summary>
/// View model for the latency widget: pings up to three targets live while the dashboard is shown. The
/// dashboard starts and stops it (<see cref="Start" />, <see cref="Stop" />).
/// </summary>
public class LatencyWidgetViewModel : ViewModelBase
{
    #region Variables

    /// <summary>
    /// Stops the pings of the targets, <c>null</c> while the widget doesn't run.
    /// </summary>
    private CancellationTokenSource _cancellationTokenSource;

    /// <summary>
    /// The network changed while the widget didn't run: the next start removes the pings of the last minute.
    /// </summary>
    private bool _isHistoryOutdated;

    /// <summary>
    /// Gets the configured targets (a target without a host is not shown).
    /// </summary>
    public ObservableCollection<LatencyTargetViewModel> Targets { get; } = [];

    /// <summary>
    /// Gets a value indicating whether a target is configured.
    /// </summary>
    public bool HasTargets
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
    /// Gets a value indicating whether the targets are pinged.
    /// </summary>
    public bool IsRunning => _cancellationTokenSource != null;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="LatencyWidgetViewModel" /> class.
    /// </summary>
    public LatencyWidgetViewModel()
    {
        LoadTargets();

        SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;
        AppearanceManager.ThemeResourcesChanged += (_, _) =>
        {
            foreach (var target in Targets)
                target.ApplyChartColors();
        };
    }

    #endregion

    #region ICommands & Actions

    /// <summary>
    /// Gets the command to enable the check (setting <c>Dashboard_CheckLatency</c>, off by default). The
    /// dashboard starts the widget when the setting changes.
    /// </summary>
    public ICommand EnableCheckCommand =>
        new RelayCommand(_ => SettingsManager.Current.Dashboard_CheckLatency = true);

    /// <summary>
    /// Gets the command to copy the values and statistics of all targets as text.
    /// </summary>
    public ICommand CopyAllCommand => new RelayCommand(_ =>
            ClipboardHelper.SetClipboard(string.Join(System.Environment.NewLine,
                Targets.Select(x => x.ToCopyText()))),
        _ => HasTargets);

    #endregion

    #region Methods

    /// <summary>
    /// Starts the pings of all targets (if the check is enabled). Called by the dashboard when it is shown. The pings
    /// of the last minute are kept (the pause is a gap in the chart).
    /// </summary>
    public void Start()
    {
        Start(true);
    }

    /// <summary>
    /// Starts the pings of all targets (if the check is enabled).
    /// </summary>
    /// <param name="keepHistory">Keep the pings of the last minute, see
    ///     <see cref="LatencyTargetViewModel.RunAsync" />.</param>
    private void Start(bool keepHistory)
    {
        if (IsRunning || !SettingsManager.Current.Dashboard_CheckLatency)
            return;

        // Sample values instead of real pings
        if (ScreenshotData.IsEnabled)
        {
            foreach (var (target, index) in Targets.Select((x, i) => (x, i)))
                target.SetSamples(ScreenshotData.CreateLatencySamples(index));

            return;
        }

        _cancellationTokenSource = new CancellationTokenSource();

        keepHistory &= !_isHistoryOutdated;
        _isHistoryOutdated = false;

        foreach (var target in Targets)
            _ = target.RunAsync(keepHistory, _cancellationTokenSource.Token);
    }

    /// <summary>
    /// Stops the pings (dashboard hidden, window minimized, widget hidden or check disabled).
    /// </summary>
    public void Stop()
    {
        if (!IsRunning)
            return;

        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = null;
    }

    /// <summary>
    /// Starts again if it runs: the host names are resolved again and the pings of the last minute are removed (e.g.
    /// after a network change, the targets may be reached via another route). If it doesn't run, the next start
    /// removes them.
    /// </summary>
    public void Restart()
    {
        if (!IsRunning)
        {
            _isHistoryOutdated = true;
            return;
        }

        Stop();
        Start(false);
    }

    /// <summary>
    /// Creates the targets from the settings.
    /// </summary>
    private void LoadTargets()
    {
        var settings = SettingsManager.Current;

        Targets.Clear();

        foreach (var (name, host) in new[]
                 {
                     (settings.Dashboard_LatencyTarget1Name, settings.Dashboard_LatencyTarget1Host),
                     (settings.Dashboard_LatencyTarget2Name, settings.Dashboard_LatencyTarget2Host),
                     (settings.Dashboard_LatencyTarget3Name, settings.Dashboard_LatencyTarget3Host)
                 })
        {
            if (!string.IsNullOrWhiteSpace(host))
                Targets.Add(new LatencyTargetViewModel(name, host.Trim()));
        }

        HasTargets = Targets.Count > 0;
    }

    private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        // Check disabled: no old values when it is enabled again (the dashboard stops the pings)
        if (e.PropertyName == nameof(SettingsInfo.Dashboard_CheckLatency) &&
            !SettingsManager.Current.Dashboard_CheckLatency)
        {
            foreach (var target in Targets)
                target.ClearHistory();

            return;
        }

        // A changed target: create the targets again, a running widget pings the new ones right away
        if (e.PropertyName == null || !e.PropertyName.StartsWith("Dashboard_LatencyTarget"))
            return;

        var wasRunning = IsRunning;

        Stop();
        LoadTargets();

        if (wasRunning)
            Start();
    }

    #endregion
}
