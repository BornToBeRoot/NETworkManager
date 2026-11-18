using log4net;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Export;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Utilities.WPF;
using NETworkManager.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace NETworkManager.ViewModels;

/// <summary>
///     View model of the mDNS / DNS-SD tool: browses the services advertised on the selected network interface
///     (see <see cref="MDNSBrowser" />) and shows them (Services) and the received DNS records (Records).
/// </summary>
public sealed class MDNSDiscoveryViewModel : ViewModelBase
{
    #region Variables

    private static readonly ILog Log = LogManager.GetLogger(typeof(MDNSDiscoveryViewModel));

    /// <summary>
    ///     Maximum effort (changed rows x all rows) to add / remove rows one by one, like the Port Scanner: WPF inserts
    ///     every row into the sorted view with a linear search. Above, the rows are replaced with one reset.
    /// </summary>
    private const long MaxRowChangesOneByOne = 10_000_000;

    private CancellationTokenSource _cancellationTokenSource;
    private Task _browseTask = Task.CompletedTask;

    // A browse until stopped is paused while the view is hidden (like the auto refresh of other tools) and resumed
    // with the same interface, service type and cache when it is shown again
    private MDNSInterfaceInfo _browseInterface;
    private IReadOnlyList<string> _browseServiceTypes;
    private MDNSServiceCache _browseCache;
    private bool _isPaused;

    // The browser reports detached snapshots from a worker thread; the latest one is merged into the rows by the
    // flush timer (like the results buffer of the scanners), so the dispatcher isn't called per snapshot.
    private MDNSDiscoverySnapshot _pendingSnapshot;
    private readonly DispatcherTimer _flushTimer;
    private readonly Stopwatch _browseStopwatch = new();

    // Rows by key (service: full name, record: id), so a new snapshot updates the existing rows
    private readonly Dictionary<string, MDNSServiceInfo> _serviceRows = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, MDNSRecordInfo> _recordRows = new(StringComparer.Ordinal);

    /// <summary>
    ///     Gets the network interfaces with multicast support.
    /// </summary>
    public ObservableCollection<MDNSInterfaceInfo> Interfaces { get; } = [];

    /// <summary>
    ///     Gets or sets the network interface to browse on.
    /// </summary>
    public MDNSInterfaceInfo SelectedInterface
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
    ///     Gets the suggested service types (* = all advertised service types).
    /// </summary>
    public IReadOnlyList<string> ServiceTypes { get; } = ["*", .. MDNSServiceCache.CommonServiceTypes];

    /// <summary>
    ///     Gets or sets the service type to browse (e.g. <c>_ipp._tcp</c> or <c>*</c>).
    /// </summary>
    public string ServiceType
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
    ///     Gets the durations of a browse in seconds (0 = until stopped).
    /// </summary>
    public IReadOnlyList<int> Durations { get; } = [10, 30, 60, 120, 300, 0];

    /// <summary>
    ///     Gets or sets the duration of a browse in seconds (0 = until stopped).
    /// </summary>
    public int SelectedDuration
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
    ///     Gets a value indicating whether a browse is running.
    /// </summary>
    public bool IsRunning
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
    ///     Gets a value indicating whether the interfaces are reloaded (refresh button, reload animation).
    /// </summary>
    public bool IsInterfacesLoading
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
    ///     Gets a value indicating whether a running browse is being stopped.
    /// </summary>
    public bool IsCanceling
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
    ///     Gets a value indicating whether a browse was started (counters, empty state).
    /// </summary>
    public bool IsBrowsed
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
    ///     Gets the seconds since the browse was started (progress bar).
    /// </summary>
    public double SecondsElapsed
    {
        get;
        private set
        {
            if (Math.Abs(value - field) < 0.01)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Gets the duration of the running browse in seconds (0 = until stopped, progress bar).
    /// </summary>
    public int BrowseDuration
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
    ///     Gets the time when the last browse ended.
    /// </summary>
    public DateTime? LastScanTime
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
    ///     Gets or sets a value indicating whether the status message is displayed.
    /// </summary>
    public bool IsStatusMessageDisplayed
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
    ///     Gets the status message to display.
    /// </summary>
    public string StatusMessage
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
    ///     Level of the status message (icon and color, see <see cref="StatusMessageText" />).
    /// </summary>
    public StatusLevel StatusMessageLevel
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
    ///     Delays filtering the results while the user is typing (see <see cref="Search" />).
    /// </summary>
    private readonly SearchDelay _searchDelay;

    /// <summary>
    ///     Gets or sets the search text for filtering the services (name, type, hostname, port, addresses, TXT
    ///     records) and the records (name, type, value).
    /// </summary>
    public string Search
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            _searchDelay.Restart();

            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Gets the discovered services.
    /// </summary>
    public ObservableRangeCollection<MDNSServiceInfo> Results { get; } = [];

    /// <summary>
    ///     Gets the filtered and sorted view of the services.
    /// </summary>
    public ICollectionView ResultsView { get; }

    /// <summary>
    ///     Gets or sets the selected service.
    /// </summary>
    public MDNSServiceInfo SelectedResult
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
    ///     Gets or sets the selected services.
    /// </summary>
    public IList SelectedResults
    {
        get;
        set
        {
            if (Equals(value, field))
                return;

            field = value;
            OnPropertyChanged();
        }
    } = new ArrayList();

    /// <summary>
    ///     Gets the received DNS records.
    /// </summary>
    public ObservableRangeCollection<MDNSRecordInfo> Records { get; } = [];

    /// <summary>
    ///     Gets the filtered and sorted view of the records.
    /// </summary>
    public ICollectionView RecordsView { get; }

    /// <summary>
    ///     Gets or sets the selected record.
    /// </summary>
    public MDNSRecordInfo SelectedRecord
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
    ///     Gets or sets the selected records.
    /// </summary>
    public IList SelectedRecords
    {
        get;
        set
        {
            if (Equals(value, field))
                return;

            field = value;
            OnPropertyChanged();
        }
    } = new ArrayList();

    /// <summary>
    ///     Gets or sets a value indicating whether the raw DNS records are shown instead of the services (like the raw
    ///     JSON of the RDAP). <see cref="ExportCommand" /> exports what is shown.
    /// </summary>
    public bool ShowRecords
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

    #region Constructor, load settings

    /// <summary>
    ///     Initializes a new instance of the <see cref="MDNSDiscoveryViewModel" /> class.
    /// </summary>
    public MDNSDiscoveryViewModel()
    {
        // Services sorted by type and name, filtered by the search
        ResultsView = CollectionViewSource.GetDefaultView(Results);
        ResultsView.SortDescriptions.Add(new SortDescription(nameof(MDNSServiceInfo.ServiceType), ListSortDirection.Ascending));
        ResultsView.SortDescriptions.Add(new SortDescription(nameof(MDNSServiceInfo.Name), ListSortDirection.Ascending));
        ResultsView.Filter = o =>
        {
            if (string.IsNullOrEmpty(Search))
                return true;

            if (o is not MDNSServiceInfo info)
                return false;

            bool Matches(string value) => value?.Contains(Search, StringComparison.OrdinalIgnoreCase) == true;

            return Matches(info.Name) ||
                   Matches(info.ServiceType) ||
                   Matches(info.Hostname) ||
                   Matches(info.Port?.ToString()) ||
                   Matches(info.Addresses) ||
                   Matches(info.TXTRecords);
        };

        // Hostname, port, addresses and TXT records can arrive after the service was added - filter it again when
        // they change. Only enabled while searching, see ApplySearch.
        var liveShaping = (ICollectionViewLiveShaping)ResultsView;
        liveShaping.LiveFilteringProperties.Add(nameof(MDNSServiceInfo.Hostname));
        liveShaping.LiveFilteringProperties.Add(nameof(MDNSServiceInfo.Port));
        liveShaping.LiveFilteringProperties.Add(nameof(MDNSServiceInfo.Addresses));
        liveShaping.LiveFilteringProperties.Add(nameof(MDNSServiceInfo.TXTRecords));

        // Records sorted by name and type, their values don't change (only the TTL)
        RecordsView = CollectionViewSource.GetDefaultView(Records);
        RecordsView.SortDescriptions.Add(new SortDescription(nameof(MDNSRecordInfo.Name), ListSortDirection.Ascending));
        RecordsView.SortDescriptions.Add(new SortDescription(nameof(MDNSRecordInfo.Type), ListSortDirection.Ascending));
        RecordsView.Filter = o =>
        {
            if (string.IsNullOrEmpty(Search))
                return true;

            if (o is not MDNSRecordInfo info)
                return false;

            return info.Name.Contains(Search, StringComparison.OrdinalIgnoreCase) ||
                   info.Type.Contains(Search, StringComparison.OrdinalIgnoreCase) ||
                   info.Value.Contains(Search, StringComparison.OrdinalIgnoreCase);
        };

        _searchDelay = new SearchDelay(GlobalStaticConfiguration.SearchDispatcherTimerTimeSpan, ApplySearch);

        _flushTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(250)
        };

        _flushTimer.Tick += (_, _) => FlushSnapshot();

        LoadSettings();

        LoadInterfaces();
    }

    private void LoadSettings()
    {
        ServiceType = MDNSServiceType.TryNormalize(SettingsManager.Current.MDNSDiscovery_ServiceType, out _)
            ? SettingsManager.Current.MDNSDiscovery_ServiceType
            : GlobalStaticConfiguration.MDNSDiscovery_ServiceType;

        SelectedDuration = Durations.Contains(SettingsManager.Current.MDNSDiscovery_Duration)
            ? SettingsManager.Current.MDNSDiscovery_Duration
            : GlobalStaticConfiguration.MDNSDiscovery_Duration;
    }

    #endregion

    #region ICommands & Actions

    /// <summary>
    ///     Gets the command to start or stop the browse.
    /// </summary>
    public ICommand BrowseCommand => new RelayCommand(_ => BrowseAction(), Browse_CanExecute);

    private bool Browse_CanExecute(object parameter)
    {
        return !DialogHelper.IsAnyDialogOpen() && !IsInterfacesLoading && (IsRunning || SelectedInterface != null);
    }

    private void BrowseAction()
    {
        if (IsRunning)
            Stop();
        else
            _browseTask = Browse();
    }

    /// <summary>
    ///     Gets the command to reload the network interfaces.
    /// </summary>
    public ICommand RefreshInterfacesCommand => new RelayCommand(_ => RefreshInterfacesAction(), RefreshInterfaces_CanExecute);

    private bool RefreshInterfaces_CanExecute(object parameter)
    {
        return !IsRunning && !IsInterfacesLoading && !DialogHelper.IsAnyDialogOpen();
    }

    private void RefreshInterfacesAction()
    {
        _ = RefreshInterfaces();
    }

    /// <summary>
    ///     Gets the command to filter the results right away (Enter in the search box, see
    ///     <see cref="SearchTextBoxHelper" />).
    /// </summary>
    public ICommand ApplySearchCommand => _searchDelay.ApplyCommand;

    /// <summary>
    ///     Gets the command to copy a value of all selected services (one per line, in the order shown in the grid).
    ///     Parameter: <c>Name</c>, <c>ServiceType</c>, <c>Hostname</c>, <c>Port</c>, <c>Addresses</c>,
    ///     <c>TXTRecords</c> or <c>Details</c> (all values of a service).
    /// </summary>
    public ICommand CopySelectedResultsCommand => new RelayCommand(CopySelectedResultsAction);

    private void CopySelectedResultsAction(object parameter)
    {
        Func<MDNSServiceInfo, string> selector = parameter switch
        {
            "Name" => x => x.FullName,
            "ServiceType" => x => x.ServiceType,
            "Hostname" => x => x.Hostname,
            "Port" => x => x.Port?.ToString(CultureInfo.InvariantCulture),
            "Addresses" => x => x.Addresses,
            "TXTRecords" => x => x.TXTRecords,
            "Details" => x => string.Join(Environment.NewLine,
                new[] { x.FullName, $"{x.Hostname}\t{x.Port}\t{x.Addresses}", x.TXTRecords }
                    .Where(value => !string.IsNullOrEmpty(value?.Trim()))),
            _ => null
        };

        if (selector == null)
            return;

        var separator = parameter is "Details" or "TXTRecords" ? Environment.NewLine + Environment.NewLine : Environment.NewLine;

        ClipboardHelper.SetClipboard(string.Join(separator,
            ResultsView.GetSelectedItemsInViewOrder<MDNSServiceInfo>(SelectedResults).Select(selector)
                .Where(x => !string.IsNullOrEmpty(x)).Distinct()));
    }

    /// <summary>
    ///     Gets the command to copy a value of all selected records (one per line, in the order shown in the grid).
    ///     Parameter: <c>Name</c>, <c>Value</c> or <c>Details</c> (name, type, class, TTL and value).
    /// </summary>
    public ICommand CopySelectedRecordsCommand => new RelayCommand(CopySelectedRecordsAction);

    private void CopySelectedRecordsAction(object parameter)
    {
        Func<MDNSRecordInfo, string> selector = parameter switch
        {
            "Name" => x => x.Name,
            "Value" => x => x.Value,
            "Details" => x => $"{x.Name}\t{x.Type}\t{x.Class}\t{x.TTL}\t{x.Value}",
            _ => null
        };

        if (selector == null)
            return;

        var values = RecordsView.GetSelectedItemsInViewOrder<MDNSRecordInfo>(SelectedRecords).Select(selector);

        // Every record is one line; names are listed once
        ClipboardHelper.SetClipboard(string.Join(Environment.NewLine,
            parameter is "Name" ? values.Distinct() : values));
    }

    /// <summary>
    ///     Gets the command to export the services or the records (what is shown, see <see cref="ShowRecords" />).
    /// </summary>
    public ICommand ExportCommand => new RelayCommand(_ => ExportAction());

    private void ExportAction()
    {
        _ = Export();
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Loads the network interfaces right away (constructor, view shown: a run command may browse right after).
    /// </summary>
    private void LoadInterfaces()
    {
        try
        {
            SetInterfaces(MDNSBrowser.GetInterfaces());
        }
        catch (Exception ex)
        {
            LoadInterfacesFailed(ex);
        }
    }

    /// <summary>
    ///     Reloads the network interfaces with the refresh button: takes at least the refresh interval, so the reload
    ///     animation is visible and the reload can't be spammed (like the other refresh buttons).
    /// </summary>
    private async Task RefreshInterfaces()
    {
        IsInterfacesLoading = true;

        try
        {
            var interfaces = await TaskHelper.WithMinimumDurationAsync(() => Task.Run(MDNSBrowser.GetInterfaces),
                GlobalStaticConfiguration.ApplicationUIRefreshInterval);

            SetInterfaces(interfaces);
        }
        catch (Exception ex)
        {
            LoadInterfacesFailed(ex);
        }
        finally
        {
            IsInterfacesLoading = false;
        }
    }

    /// <summary>
    ///     Shows the interfaces and selects the previous (or last used) one.
    /// </summary>
    private void SetInterfaces(IReadOnlyList<MDNSInterfaceInfo> interfaces)
    {
        var id = SelectedInterface?.Id ?? SettingsManager.Current.MDNSDiscovery_InterfaceId;

        Interfaces.Clear();

        foreach (var networkInterface in interfaces)
            Interfaces.Add(networkInterface);

        SelectedInterface = Interfaces.FirstOrDefault(x => x.Id == id) ?? Interfaces.FirstOrDefault();

        Log.Debug($"Found {Interfaces.Count} network interfaces with multicast support, selected: \"{SelectedInterface?.Name}\".");

        if (SelectedInterface == null)
            SetStatusMessage(Strings.MDNSDiscovery_NoInterface, StatusLevel.Warning);
        else if (StatusMessage is not null &&
                 (StatusMessage == Strings.MDNSDiscovery_NoInterface || StatusMessage.StartsWith(Strings.MDNSDiscovery_Failed)))
            IsStatusMessageDisplayed = false;
    }

    private void LoadInterfacesFailed(Exception ex)
    {
        Log.Error("Cannot load the network interfaces with multicast support.", ex);

        SetStatusMessage(Strings.MDNSDiscovery_Failed + Environment.NewLine + ex.Message, StatusLevel.Error);
    }

    /// <summary>
    ///     Shows a message below the input (replaces the previous one).
    /// </summary>
    private void SetStatusMessage(string message, StatusLevel level)
    {
        StatusMessage = message;
        StatusMessageLevel = level;
        IsStatusMessageDisplayed = true;
    }

    /// <summary>
    ///     Browses on the selected interface until the duration expires or <see cref="Stop" /> is called. The results
    ///     are updated while browsing and kept afterward.
    /// </summary>
    private async Task Browse()
    {
        if (IsRunning || SelectedInterface == null || !MDNSServiceType.TryNormalize(ServiceType, out var serviceType))
            return;

        _isPaused = false;

        _browseInterface = SelectedInterface;
        _browseServiceTypes = [serviceType];
        _browseCache = new MDNSServiceCache(_browseServiceTypes, _browseInterface.Name);

        SettingsManager.Current.MDNSDiscovery_ServiceType = ServiceType;
        SettingsManager.Current.MDNSDiscovery_Duration = SelectedDuration;
        SettingsManager.Current.MDNSDiscovery_InterfaceId = _browseInterface.Id;

        IsStatusMessageDisplayed = false;
        StatusMessage = string.Empty;
        StatusMessageLevel = StatusLevel.None;

        Results.Clear();
        Records.Clear();
        _serviceRows.Clear();
        _recordRows.Clear();
        Interlocked.Exchange(ref _pendingSnapshot, null);

        await Run(SelectedDuration);
    }

    /// <summary>
    ///     Runs the browse of <see cref="_browseInterface" /> / <see cref="_browseServiceTypes" /> with
    ///     <see cref="_browseCache" /> (new browse or resumed after a pause, the rows are kept).
    /// </summary>
    /// <param name="duration">Duration in seconds, 0 = until stopped.</param>
    private async Task Run(int duration)
    {
        var networkInterface = _browseInterface;
        var serviceTypes = _browseServiceTypes;
        var cache = _browseCache;

        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        using var timeout = new CancellationTokenSource();

        if (duration > 0)
            timeout.CancelAfter(TimeSpan.FromSeconds(duration));

        using var linked =
            CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token, timeout.Token);

        BrowseDuration = duration;
        SecondsElapsed = 0;
        IsBrowsed = true;
        IsCanceling = false;
        IsRunning = true;

        _browseStopwatch.Restart();
        _flushTimer.Start();

        try
        {
            await Task.Run(() => MDNSBrowser.BrowseAsync(networkInterface, serviceTypes,
                snapshot => Interlocked.Exchange(ref _pendingSnapshot, snapshot), linked.Token, cache));
        }
        catch (OperationCanceledException) when (linked.IsCancellationRequested)
        {
            // Stopped before the duration expired (until stopped / paused: the normal end)
            if (_cancellationTokenSource.IsCancellationRequested && duration > 0)
                SetStatusMessage(Strings.CanceledByUserMessage, StatusLevel.None);
        }
        catch (Exception ex)
        {
            Log.Error("mDNS / DNS-SD service discovery failed.", ex);

            SetStatusMessage(Strings.MDNSDiscovery_Failed + Environment.NewLine + ex.Message, StatusLevel.Error);
        }
        finally
        {
            _flushTimer.Stop();
            _browseStopwatch.Stop();

            FlushSnapshot();

            LastScanTime = DateTime.Now;

            IsCanceling = false;
            IsRunning = false;
        }
    }

    /// <summary>
    ///     Stops the running browse (the results are kept).
    /// </summary>
    private void Stop()
    {
        if (!IsRunning)
            return;

        IsCanceling = true;
        _cancellationTokenSource?.Cancel();
    }

    /// <summary>
    ///     Opens the tool with a service type (run command). <see cref="ApplicationRequest.Start" /> = false only
    ///     fills in the service type.
    /// </summary>
    /// <param name="request">Request with the service type as target.</param>
    public async Task OpenAsync(ApplicationRequest request)
    {
        if (!MDNSServiceType.TryNormalize(request.Target, out _))
            return;

        Stop();

        await _browseTask;

        ServiceType = request.Target;

        if (request.Start)
            await (_browseTask = Browse());
    }

    /// <summary>
    ///     Called when the view is shown: resumes a paused browse (see <see cref="OnViewHide" />), otherwise reloads
    ///     the interfaces (not while browsing).
    /// </summary>
    public void OnViewVisible()
    {
        if (_isPaused)
        {
            _ = Resume();

            return;
        }

        if (!IsRunning && !IsInterfacesLoading)
            LoadInterfaces();
    }

    /// <summary>
    ///     Called when the view is hidden. A browse with a duration continues in the background like the scanners, a
    ///     browse until stopped is paused (like the auto refresh of other tools) and resumed when the view is shown.
    /// </summary>
    public void OnViewHide()
    {
        if (!IsRunning || BrowseDuration != 0)
            return;

        _isPaused = true;

        Stop();
    }

    /// <summary>
    ///     Resumes a paused browse with the same cache: the rows are kept, expired records are removed and the due
    ///     questions are sent right away.
    /// </summary>
    private async Task Resume()
    {
        // The pause may still be ending (shown again right away)
        await _browseTask;

        // Not if a new browse was started meanwhile (e.g. run command)
        if (!_isPaused || IsRunning)
            return;

        _isPaused = false;

        await (_browseTask = Run(0));
    }

    private Task Export()
    {
        var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

        var exportRecords = ShowRecords;

        var childWindow = new ExportChildWindow();

        var childWindowViewModel = new ExportViewModel(async instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            try
            {
                // Copy the rows on the UI thread (they are updated while browsing) and export them in the
                // background. In the order shown in the grid.
                if (exportRecords)
                {
                    var records = RecordsView.GetItemsInViewOrder<MDNSRecordInfo>(instance.ExportAll, SelectedRecords)
                        .Select(x => new MDNSRecordInfo(x.Id, x.Name, x.Type, x.Value, x.NetworkInterface,
                            x.CacheFlush, x.TTL)).ToArray();

                    await Task.Run(() => ExportManager.Export(instance.FilePath, instance.FileType, records));
                }
                else
                {
                    var services = ResultsView.GetItemsInViewOrder<MDNSServiceInfo>(instance.ExportAll, SelectedResults)
                        .Select(x => new MDNSServiceInfo(x.FullName, x.Name, x.ServiceType, x.NetworkInterface,
                            x.Hostname, x.Port, x.Priority, x.Weight, x.Addresses, x.TXTRecords, x.TTL)).ToArray();

                    await Task.Run(() => ExportManager.Export(instance.FilePath, instance.FileType, services));
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error while exporting data as " + instance.FileType, ex);

                await DialogHelper.ShowMessageAsync(window, Strings.Error,
                    Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine +
                    Environment.NewLine + ex.Message, ChildWindowIcon.Error);
            }

            SettingsManager.Current.MDNSDiscovery_ExportFileType = instance.FileType;
            SettingsManager.Current.MDNSDiscovery_ExportFilePath = instance.FilePath;
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        }, [
            ExportFileType.Csv, ExportFileType.Xml, ExportFileType.Json
        ], true, SettingsManager.Current.MDNSDiscovery_ExportFileType,
        SettingsManager.Current.MDNSDiscovery_ExportFilePath);

        childWindow.Title = Strings.Export;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        return window.ShowChildWindowAsync(childWindow);
    }

    #endregion

    #region Events

    /// <summary>
    ///     Filters the results with the current <see cref="Search" /> right away (e.g. Enter in the search box).
    /// </summary>
    private void ApplySearch()
    {
        // Live filtering only while searching - without a search every service is shown anyway, but it would still
        // listen to the filtered properties of every row (see the constructor)
        var liveShaping = (ICollectionViewLiveShaping)ResultsView;
        var isSearching = !string.IsNullOrEmpty(Search);

        if (liveShaping.IsLiveFiltering != isSearching)
            liveShaping.IsLiveFiltering = isSearching;

        ResultsView.Refresh();
        RecordsView.Refresh();
    }

    /// <summary>
    ///     Merges the latest snapshot of the browser into the rows (UI thread, flush timer and end of the browse).
    /// </summary>
    private void FlushSnapshot()
    {
        SecondsElapsed = _browseStopwatch.Elapsed.TotalSeconds;

        var snapshot = Interlocked.Exchange(ref _pendingSnapshot, null);

        if (snapshot == null)
            return;

        Merge(Results, _serviceRows, snapshot.Services, x => x.FullName, (row, value) => row.UpdateFrom(value));
        Merge(Records, _recordRows, snapshot.Records, x => x.Id, (row, value) => row.UpdateFrom(value));
    }

    /// <summary>
    ///     Merges a snapshot into the rows: existing rows are updated (selection and expanded details are kept), rows
    ///     that are no longer in the snapshot are removed and new ones added - one by one, or with one reset if there
    ///     are many changes (see <see cref="MaxRowChangesOneByOne" />).
    /// </summary>
    private static void Merge<T>(ObservableRangeCollection<T> rows, Dictionary<string, T> index,
        IReadOnlyList<T> values, Func<T, string> getKey, Action<T, T> update)
    {
        var keys = values.Select(getKey).ToHashSet(index.Comparer);
        var removed = index.Where(x => !keys.Contains(x.Key)).ToArray();
        var added = new List<T>();

        foreach (var value in values)
        {
            if (index.TryGetValue(getKey(value), out var row))
                update(row, value);
            else if (index.TryAdd(getKey(value), value))
                added.Add(value);
        }

        foreach (var row in removed)
            index.Remove(row.Key);

        if (removed.Length == 0 && added.Count == 0)
            return;

        if ((long)(removed.Length + added.Count) * (rows.Count + added.Count) > MaxRowChangesOneByOne)
        {
            var removedRows = removed.Select(x => x.Value).ToHashSet();

            rows.ReplaceAll(rows.Where(x => !removedRows.Contains(x)).Concat(added));

            return;
        }

        foreach (var row in removed)
            rows.Remove(row.Value);

        foreach (var row in added)
            rows.Add(row);
    }

    #endregion
}
