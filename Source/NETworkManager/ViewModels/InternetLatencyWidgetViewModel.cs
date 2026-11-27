using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

/// <summary>
/// View model for the internet latency widget: pings up to three targets live while the dashboard is shown. The
/// dashboard starts and stops it (<see cref="Start" />, <see cref="Stop" />).
/// </summary>
public class InternetLatencyWidgetViewModel : ViewModelBase
{
    #region Variables

    /// <summary>
    /// Stops the pings of the targets, <c>null</c> while the widget doesn't run.
    /// </summary>
    private CancellationTokenSource _cancellationTokenSource;

    /// <summary>
    /// Gets the configured targets (a target without a host is not shown).
    /// </summary>
    public ObservableCollection<InternetLatencyTargetViewModel> Targets { get; } = [];

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
    /// Initializes a new instance of the <see cref="InternetLatencyWidgetViewModel" /> class.
    /// </summary>
    public InternetLatencyWidgetViewModel()
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
    /// Gets the command to enable the check (setting <c>Dashboard_CheckInternetLatency</c>, off by default). The
    /// dashboard starts the widget when the setting changes.
    /// </summary>
    public ICommand EnableCheckCommand =>
        new RelayCommand(_ => SettingsManager.Current.Dashboard_CheckInternetLatency = true);

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
    /// Starts the pings of all targets (if the check is enabled). Called by the dashboard when it is shown.
    /// </summary>
    public void Start()
    {
        if (IsRunning || !SettingsManager.Current.Dashboard_CheckInternetLatency)
            return;

        // Sample values instead of real pings
        if (ScreenshotData.IsEnabled)
        {
            foreach (var (target, index) in Targets.Select((x, i) => (x, i)))
                target.SetSamples(ScreenshotData.CreateInternetLatencySamples(index));

            return;
        }

        _cancellationTokenSource = new CancellationTokenSource();

        foreach (var target in Targets)
            _ = target.RunAsync(_cancellationTokenSource.Token);
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
    /// Starts again if it runs: the host names are resolved again and the history is removed (e.g. after a network
    /// change, the targets may be reached via another route).
    /// </summary>
    public void Restart()
    {
        if (!IsRunning)
            return;

        Stop();
        Start();
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
                     (settings.Dashboard_InternetLatencyTarget1Name, settings.Dashboard_InternetLatencyTarget1Host),
                     (settings.Dashboard_InternetLatencyTarget2Name, settings.Dashboard_InternetLatencyTarget2Host),
                     (settings.Dashboard_InternetLatencyTarget3Name, settings.Dashboard_InternetLatencyTarget3Host)
                 })
        {
            if (!string.IsNullOrWhiteSpace(host))
                Targets.Add(new InternetLatencyTargetViewModel(name, host.Trim()));
        }

        HasTargets = Targets.Count > 0;
    }

    private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        // A changed target: create the targets again, a running widget pings the new ones right away
        if (e.PropertyName == null || !e.PropertyName.StartsWith("Dashboard_InternetLatencyTarget"))
            return;

        var wasRunning = IsRunning;

        Stop();
        LoadTargets();

        if (wasRunning)
            Start();
    }

    #endregion
}
