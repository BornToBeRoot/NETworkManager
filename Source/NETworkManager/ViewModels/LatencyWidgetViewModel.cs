using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using NETworkManager.Models.Network;
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
    /// Target list of the settings whose changes are followed, see <see cref="SubscribeTargets" />.
    /// </summary>
    private ObservableCollection<LatencyTargetInfo> _subscribedTargets;

    /// <summary>
    /// Gets the number of targets per line when they are side by side (own row): up to 3.
    /// </summary>
    public int WideColumns
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = 1;

    /// <summary>
    /// Gets a value indicating whether a target is configured.
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
        SubscribeTargets();
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
    /// Gets the command to open the settings of the dashboard (no targets configured).
    /// </summary>
    public ICommand ConfigureCommand => new RelayCommand(_ => NavigationManager.OpenSettings());

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
        var wasRunning = IsRunning;

        Stop();

        // Unchanged targets keep their pings of the last minute (e.g. another target was added)
        var previous = Targets.ToList();

        Targets.Clear();

        foreach (var info in SettingsManager.Current.Dashboard_LatencyTargets
                     .Where(x => x.IsGateway || !string.IsNullOrWhiteSpace(x.Host))
                     .Take(GlobalStaticConfiguration.Dashboard_LatencyMaxTargets))
        {
            var target = previous.FirstOrDefault(x => x.Info.IsSameTarget(info));

            if (target != null)
                previous.Remove(target);

            Targets.Add(target ?? new LatencyTargetViewModel(info));
        }

        HasTargets = Targets.Count > 0;

        // Side by side: up to 3 per line, more targets in a second line
        WideColumns = Math.Clamp(Targets.Count, 1, 3);

        if (wasRunning)
            Start();
    }

    private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            // Check disabled: no old values when it is enabled again (the dashboard stops the pings)
            case nameof(SettingsInfo.Dashboard_CheckLatency) when !SettingsManager.Current.Dashboard_CheckLatency:
                foreach (var target in Targets)
                    target.ClearHistory();
                break;
            // The list was replaced (e.g. settings imported or reset)
            case nameof(SettingsInfo.Dashboard_LatencyTargets):
                SubscribeTargets();
                LoadTargets();
                break;
        }
    }

    /// <summary>
    /// Follows the changes of the target list (add, edit, delete in the settings).
    /// </summary>
    private void SubscribeTargets()
    {
        if (_subscribedTargets != null)
            _subscribedTargets.CollectionChanged -= Targets_CollectionChanged;

        _subscribedTargets = SettingsManager.Current.Dashboard_LatencyTargets;
        _subscribedTargets.CollectionChanged += Targets_CollectionChanged;
    }

    private void Targets_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        LoadTargets();
    }

    #endregion
}
