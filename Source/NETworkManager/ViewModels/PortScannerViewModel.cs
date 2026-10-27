using log4net;
using MahApps.Metro.Controls;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Controls;
using NETworkManager.Localization;
using NETworkManager.Localization.Resources;
using NETworkManager.Models;
using NETworkManager.Models.Export;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace NETworkManager.ViewModels;

/// <summary>
/// ViewModel for the Port Scanner feature.
/// </summary>
public class PortScannerViewModel : ViewModelBase
{
    #region Variables
    private static readonly ILog Log = LogManager.GetLogger(typeof(PortScannerViewModel));

    /// <summary>
    /// Maximum number of rows that are removed / added one by one when the results of hosts are replaced (rescan),
    /// see <see cref="ReplaceResults"/>.
    /// </summary>
    private const int MaxRowsChangedOneByOne = 1000;

    private CancellationTokenSource _cancellationTokenSource;

    private readonly Guid _tabId;
    private bool _firstLoad = true;
    private bool _closed;

    // Background PortScanned events append here instead of hopping to the UI thread per item -
    // a DispatcherTimer periodically flushes this into the Results collection instead, so a large
    // scan doesn't flood the dispatcher queue with one BeginInvoke per port.
    private readonly List<PortScannerPortInfo> _resultsBuffer = [];
    private readonly Lock _resultsBufferLock = new();
    private DispatcherTimer _resultsFlushTimer;

    // Same reasoning as the results buffer above - ProgressChanged fires once per port
    // (unconditionally, unlike PortScanned), so it's flushed to the bound property on the same
    // timer instead of updating it directly from the background thread on every event.
    private int _latestPortsScanned;
    private int _latestPortsOpen;
    private int _latestPortsClosed;

    // Ports of the last scan, to rescan hosts with the same ports (and ScanOptions)
    private int[] _lastPorts;

    // Rescan of shown hosts (see RescanSelectedHosts): their rows are removed and added again by the rescan. The
    // counters are the ones of the other hosts (base) plus the ones of the rescan; the progress of the last scan is
    // restored afterwards.
    private bool _isRescan;
    private int _rescanBasePortsOpen;
    private int _rescanBasePortsClosed;
    private int _rescanPortsToScan;
    private int _rescanPortsScanned;

    // Rows and counters of the rescanned hosts before the rescan, restored if the rescan is canceled or fails
    private HashSet<IPAddress> _rescanIPAddresses;
    private List<PortScannerPortInfo> _rescanRemovedRows;
    private int _rescanPortsOpen;
    private int _rescanPortsClosed;

    /// <summary>
    /// Gets or sets the host to scan.
    /// </summary>
    public string Host
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
    /// Gets the collection view for the host history.
    /// </summary>
    public ICollectionView HostHistoryView { get; }

    /// <summary>
    /// Gets or sets the ports to scan (e.g., "80, 443, 1-100").
    /// </summary>
    public string Ports
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
    /// Gets the collection view for the ports history.
    /// </summary>
    public ICollectionView PortsHistoryView { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the scan is currently running.
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
    /// Gets or sets a value indicating whether the scan is being canceled.
    /// </summary>
    public bool IsCanceling
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
    /// Gets or sets the collection of scan results.
    /// </summary>
    public ObservableRangeCollection<PortScannerPortInfo> Results
    {
        get;
        set
        {
            if (field != null && value == field)
                return;

            field = value;
        }
    } = [];

    /// <summary>
    /// Gets the collection view for the scan results.
    /// </summary>
    public ICollectionView ResultsView { get; }

    /// <summary>
    /// Gets or sets the currently selected scan result.
    /// </summary>
    public PortScannerPortInfo SelectedResult
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsWebConsoleAvailable));
        }
    }

    /// <summary>
    /// Gets a value indicating whether the selected result is an open web port (80, 443, 8080, 8443).
    /// </summary>
    public bool IsWebConsoleAvailable => SelectedResult is { State: PortState.Open } selectedResult &&
                                         WebConsoleUrlHelper.IsWebPort(selectedResult.Port);

    /// <summary>
    /// Gets or sets the list of currently selected scan results (for multi-selection).
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
    /// Gets or sets the total number of ports to scan.
    /// </summary>
    public int PortsToScan
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
    /// Gets or sets the number of ports already scanned.
    /// </summary>
    public int PortsScanned
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
    /// Gets or sets the number of ports found to be open so far.
    /// </summary>
    public int PortsOpen
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
    /// Gets or sets the number of ports found to be closed so far.
    /// </summary>
    public int PortsClosed
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
    /// Gets or sets a value indicating whether the scan is being prepared.
    /// </summary>
    public bool PreparingScan
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
    /// Gets or sets a value indicating whether the status message is displayed.
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
    /// Delays filtering the results while the user is typing (see <see cref="Search"/>), so a large result is not
    /// filtered on every key press.
    /// </summary>
    private readonly DispatcherTimer _searchDispatcherTimer = new();

    /// <summary>
    /// Gets or sets the search text for filtering the results (IP address, hostname, port, protocol, service, status).
    /// The results are filtered after a short delay (see <see cref="_searchDispatcherTimer"/>).
    /// </summary>
    public string Search
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            _searchDispatcherTimer.Stop();
            _searchDispatcherTimer.Start();

            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the status message to display.
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
    /// Gets the options of the scan whose results are shown. Columns and context menu follow them instead of
    /// the current settings, which may have been changed since. While no results are shown, they follow the
    /// current settings.
    /// </summary>
    public PortScannerOptions ScanOptions
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

    #endregion

    #region Constructor, load settings, shutdown

    /// <summary>
    /// Initializes a new instance of the <see cref="PortScannerViewModel"/> class.
    /// </summary>
    /// <param name="tabId">The unique identifier for the tab.</param>
    /// <param name="host">The initial host to scan.</param>
    /// <param name="port">The initial ports to scan.</param>
    /// <param name="start">Starts it on the first load. If false, the input is only filled in.</param>
    public PortScannerViewModel(Guid tabId, string host, string port, bool start = true)
    {
        _firstLoad = start;

        ConfigurationManager.Current.PortScannerTabCount++;

        _tabId = tabId;
        Host = host;
        Ports = port;

        // Set collection view
        HostHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.PortScanner_HostHistory);
        PortsHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.PortScanner_PortHistory);

        // Columns etc. until the first scan
        ScanOptions = CreateScanOptions();

        SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;

        // Result view
        ResultsView = CollectionViewSource.GetDefaultView(Results);
        ResultsView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(PortScannerPortInfo.HostAsString)));

        // Sort by IP address and port - results are added in the order the ports are finished
        ((ListCollectionView)ResultsView).CustomSort = Comparer<PortScannerPortInfo>.Create((x, y) =>
        {
            var result = IPAddressHelper.CompareIPAddresses(x.IPAddress, y.IPAddress);

            return result != 0 ? result : x.Port.CompareTo(y.Port);
        });

        // Results don't change after they were added, no live filtering needed
        ResultsView.Filter = o =>
        {
            if (string.IsNullOrEmpty(Search))
                return true;

            if (o is not PortScannerPortInfo info)
                return false;

            bool Matches(string value) => value?.Contains(Search, StringComparison.OrdinalIgnoreCase) == true;

            // The status is shown translated, so the translated text is searched as well
            return Matches(info.IPAddress.ToString()) ||
                   Matches(info.Hostname) ||
                   Matches(info.Port.ToString()) ||
                   Matches(info.LookupInfo.Protocol.ToString()) ||
                   Matches(info.LookupInfo.Service) ||
                   Matches(info.LookupInfo.Description) ||
                   Matches(info.State.ToString()) ||
                   Matches(ResourceTranslator.Translate(ResourceIdentifier.PortState, info.State));
        };

        _searchDispatcherTimer.Interval = GlobalStaticConfiguration.SearchDispatcherTimerTimeSpan;
        _searchDispatcherTimer.Tick += SearchDispatcherTimer_Tick;

        LoadSettings();
    }

    private void LoadSettings()
    {
    }

    public void OnLoaded()
    {
        if (!_firstLoad)
            return;

        if (!string.IsNullOrEmpty(Host) && !string.IsNullOrEmpty(Ports))
            _ = Start();

        _firstLoad = false;
    }

    public void OnClose()
    {
        // Prevent multiple calls
        if (_closed)
            return;

        _closed = true;

        SettingsManager.Current.PropertyChanged -= SettingsManager_PropertyChanged;

        // Stop scan
        if (IsRunning)
            Stop();

        ConfigurationManager.Current.PortScannerTabCount--;
    }

    #endregion

    #region ICommands & Actions

    public ICommand OpenPortProfileSelectionCommand =>
        new RelayCommand(_ => OpenPortProfileSelectionAction(), OpenPortProfileSelection_CanExecute);

    private bool OpenPortProfileSelection_CanExecute(object parameter)
    {
        return Application.Current.MainWindow != null &&
               !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen &&
               !ConfigurationManager.Current.IsChildWindowOpen;
    }

    private void OpenPortProfileSelectionAction()
    {
        _ = OpenPortProfileSelection();
    }

    public ICommand ScanCommand => new RelayCommand(_ => ScanAction(), Scan_CanExecute);

    private bool Scan_CanExecute(object parameter)
    {
        return Application.Current.MainWindow != null &&
               !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen &&
               !ConfigurationManager.Current.IsChildWindowOpen;
    }

    private void ScanAction()
    {
        if (IsRunning)
            Stop();
        else
            _ = Start();
    }

    /// <summary>
    /// Gets the command to rescan the selected hosts (with the ports and options of the last scan).
    /// </summary>
    public ICommand RescanSelectedHostsCommand => new RelayCommand(_ => RescanSelectedHosts());

    /// <summary>
    /// Gets the command to open the selected open web port in the Web Console.
    /// </summary>
    public ICommand OpenWebConsoleCommand => new RelayCommand(_ => OpenWebConsoleAction());

    private void OpenWebConsoleAction()
    {
        if (!IsWebConsoleAvailable)
            return;

        // The hostname of the Port Scanner is always a DNS hostname (PTR)
        var host = !string.IsNullOrEmpty(SelectedResult.Hostname)
            ? SelectedResult.Hostname
            : SelectedResult.IPAddress.ToString();

        NavigationManager.Open(new ApplicationRequest(ApplicationName.WebConsole)
        {
            Target = WebConsoleUrlHelper.CreateUrl(host, SelectedResult.Port)
        });
    }

    /// <summary>
    /// Gets the command to copy a value of all selected results (one per line, in the order shown in the grid).
    /// Parameter: <c>IPAddress</c>, <c>Hostname</c>, <c>Port</c> or <c>IPAddressPort</c> (<c>ip:port</c>).
    /// </summary>
    public ICommand CopySelectedResultsCommand => new RelayCommand(CopySelectedResultsAction);

    private void CopySelectedResultsAction(object parameter)
    {
        Func<PortScannerPortInfo, string> selector = parameter switch
        {
            "IPAddress" => x => x.IPAddress.ToString(),
            "Hostname" => x => x.Hostname,
            "Port" => x => x.Port.ToString(),
            // IPv6 in brackets, so the port can be separated
            "IPAddressPort" => x => new IPEndPoint(x.IPAddress, x.Port).ToString(),
            _ => null
        };

        if (selector == null)
            return;

        ClipboardHelper.SetClipboard(string.Join(Environment.NewLine,
            GetSelectedResultsInGridOrder().Select(selector).Where(x => !string.IsNullOrEmpty(x)).Distinct()));
    }

    public ICommand ExportCommand => new RelayCommand(_ => ExportAction());

    private void ExportAction()
    {
        _ = Export();
    }

    #endregion

    #region Methods

    private async Task OpenPortProfileSelection()
    {
        var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

        var childWindow = new PortProfilesChildWindow(window);

        var childWindowViewModel = new PortProfilesViewModel(async instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            Ports = string.Join("; ", instance.GetSelectedPortProfiles().Select(x => x.Ports));
        }, async _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        });

        childWindow.Title = Strings.SelectPortProfile;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        await window.ShowChildWindowAsync(childWindow);
    }

    private async Task Start()
    {
        // Every port is kept in memory - ask before a large scan and refuse a too large one
        // (e.g. 10.0.0.0/8 with 1-65535) instead of running out of memory
        long hostCount;
        int portsPerHost;

        try
        {
            hostCount = HostRangeHelper.GetHostCount(HostRangeHelper.CreateListFromInput(Host));
            portsPerHost = PortRangeHelper.ConvertPortRangeToIntArray(Ports).Length;
        }
        catch (Exception ex)
        {
            Log.Error("Error while counting the hosts and ports", ex);

            StatusMessage = ex.Message;
            IsStatusMessageDisplayed = true;

            return;
        }

        var portCount = hostCount * portsPerHost;

        if (portCount > GlobalStaticConfiguration.PortScanner_MaxPortCount)
        {
            StatusMessage = string.Format(Strings.ScanTooManyPortsMessage, portCount.ToString("N0"),
                hostCount.ToString("N0"), portsPerHost.ToString("N0"),
                GlobalStaticConfiguration.PortScanner_MaxPortCount.ToString("N0"));
            IsStatusMessageDisplayed = true;

            return;
        }

        if (portCount > GlobalStaticConfiguration.PortScanner_ConfirmPortCount)
        {
            var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive) ??
                         Application.Current.MainWindow;

            if (!await DialogHelper.ShowConfirmationMessageAsync(window, Strings.Scan,
                    string.Format(Strings.ScanManyPortsMessage, portCount.ToString("N0"), hostCount.ToString("N0"),
                        portsPerHost.ToString("N0")),
                    ChildWindowIcon.Warn, Strings.Scan))
                return;

            // The tab may have been closed while the dialog was open (OnClose only stops a running scan)
            if (_closed)
                return;
        }

        IsStatusMessageDisplayed = false;
        StatusMessage = string.Empty;

        IsRunning = true;
        PreparingScan = true;

        _resultsFlushTimer?.Stop();

        lock (_resultsBufferLock)
        {
            _resultsBuffer.Clear();
        }

        Results.Clear();

        // No results are shown until the scan's options are known - follow the current settings
        ScanOptions = CreateScanOptions();

        // Reset before hostname resolution too (not just after), so a cancellation during
        // resolution can't flush the previous scan's stale totals - PortsToScan = 0 also hides
        // the open/closed summary until the new scan's port count is known.
        PortsToScan = 0;
        PortsScanned = 0;
        PortsOpen = 0;
        PortsClosed = 0;
        Volatile.Write(ref _latestPortsScanned, 0);
        Volatile.Write(ref _latestPortsOpen, 0);
        Volatile.Write(ref _latestPortsClosed, 0);

        DragablzTabItem.SetTabHeader(_tabId, Host);

        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        // Resolve hostnames and convert the ports. Start() is fire-and-forget, so every exception has to be
        // handled here - otherwise the scan would never end (IsRunning stays true).
        (List<(IPAddress ipAddress, string hostname)> hosts, List<string> hostnamesNotResolved) hosts;
        int[] ports;

        try
        {
            hosts = await HostRangeHelper.ResolveAsync(HostRangeHelper.CreateListFromInput(Host),
                SettingsManager.Current.Network_ResolveHostnamePreferIPv4, keepAllHostnames: false,
                _cancellationTokenSource.Token);

            ports = await PortRangeHelper.ConvertPortRangeToIntArrayAsync(Ports);
        }
        catch (OperationCanceledException)
        {
            ScanComplete(this, new ScanCompleteArgs(true));
            return;
        }
        catch (Exception ex)
        {
            ScanComplete(this, new ScanCompleteArgs(false, ex));
            return;
        }

        // Show error message if (some) hostnames could not be resolved
        if (hosts.hostnamesNotResolved.Count > 0)
        {
            StatusMessage =
                $"{Strings.TheFollowingHostnamesCouldNotBeResolved} {string.Join(", ", hosts.hostnamesNotResolved)}";
            IsStatusMessageDisplayed = true;
        }

        PortsToScan = ports.Length * hosts.hosts.Count;

        PreparingScan = false;

        // Add host(s) to the history
        ListHelper.Modify(SettingsManager.Current.PortScanner_HostHistory, Host,
            SettingsManager.Current.General_HistoryListEntries);
        ListHelper.Modify(SettingsManager.Current.PortScanner_PortHistory, Ports,
            SettingsManager.Current.General_HistoryListEntries);

        ScanOptions = CreateScanOptions();

        _lastPorts = ports;

        StartScanner(hosts.hosts, ports);
    }

    /// <summary>
    /// Rescans the selected hosts with the ports and options of the last scan. Their rows are replaced by the
    /// results of the rescan (a port may be open / closed now).
    /// </summary>
    private void RescanSelectedHosts()
    {
        if (IsRunning || _lastPorts == null)
            return;

        var ipAddresses = GetSelectedResultsInGridOrder().Select(x => x.IPAddress).Distinct().ToList();

        if (ipAddresses.Count == 0)
            return;

        var rescannedIPAddresses = ipAddresses.ToHashSet();

        _rescanIPAddresses = rescannedIPAddresses;
        _rescanRemovedRows = [.. Results.Where(x => rescannedIPAddresses.Contains(x.IPAddress))];
        _rescanPortsOpen = PortsOpen;
        _rescanPortsClosed = PortsClosed;

        ReplaceResults(rescannedIPAddresses, []);

        // Every host was scanned with all ports: the other hosts keep their counters
        var rescanPortCount = ipAddresses.Count * _lastPorts.Length;

        _rescanPortsToScan = PortsToScan;
        _rescanPortsScanned = PortsScanned;
        _rescanBasePortsOpen = Results.Count(x => x.State == PortState.Open);
        _rescanBasePortsClosed = Math.Max(0, PortsScanned - rescanPortCount - _rescanBasePortsOpen);
        _isRescan = true;

        IsStatusMessageDisplayed = false;
        IsRunning = true;

        _resultsFlushTimer?.Stop();

        lock (_resultsBufferLock)
        {
            _resultsBuffer.Clear();
        }

        PortsToScan = rescanPortCount;
        PortsScanned = 0;
        Volatile.Write(ref _latestPortsScanned, 0);
        Volatile.Write(ref _latestPortsOpen, _rescanBasePortsOpen);
        Volatile.Write(ref _latestPortsClosed, _rescanBasePortsClosed);

        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        StartScanner([.. ipAddresses.Select(x => (x, string.Empty))], _lastPorts);
    }

    /// <summary>
    /// Replaces the rows of the given hosts. Few rows are removed / added one by one, so the groups of the other hosts
    /// keep their state (collapsed) and the scroll position stays. Many rows with one reset instead: every single
    /// change searches the collection and the sorted, grouped view (e.g. a host with 1-65535 and closed ports shown
    /// would freeze the window for minutes).
    /// </summary>
    /// <param name="ipAddresses">Hosts whose rows are replaced.</param>
    /// <param name="rows">New rows of the hosts.</param>
    private void ReplaceResults(HashSet<IPAddress> ipAddresses, IReadOnlyCollection<PortScannerPortInfo> rows)
    {
        var oldRows = Results.Where(x => ipAddresses.Contains(x.IPAddress)).ToList();

        if (oldRows.Count + rows.Count > MaxRowsChangedOneByOne)
        {
            Results.ReplaceAll(Results.Where(x => !ipAddresses.Contains(x.IPAddress)).Concat(rows));

            return;
        }

        foreach (var row in oldRows)
            Results.Remove(row);

        foreach (var row in rows)
            Results.Add(row);
    }

    /// <summary>
    /// Creates the scanner (with <see cref="ScanOptions"/>), subscribes to its events, starts the flush timer and
    /// the scan.
    /// </summary>
    /// <param name="hosts">Hosts to scan.</param>
    /// <param name="ports">Ports to scan.</param>
    private void StartScanner(List<(IPAddress ipAddress, string hostname)> hosts, int[] ports)
    {
        var portScanner = new PortScanner(ScanOptions);

        portScanner.PortScanned += PortScanned;
        portScanner.ScanComplete += ScanComplete;
        portScanner.ProgressChanged += ProgressChanged;

        _resultsFlushTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(150)
        };
        _resultsFlushTimer.Tick += (_, _) =>
        {
            FlushResultsBuffer();
            FlushProgress();
        };
        _resultsFlushTimer.Start();

        portScanner.ScanAsync(hosts, ports, _cancellationTokenSource.Token);
    }

    /// <summary>
    /// Creates the scan options from the current settings.
    /// </summary>
    /// <returns>Options for the <see cref="PortScanner"/>.</returns>
    private static PortScannerOptions CreateScanOptions()
    {
        return new PortScannerOptions
        {
            MaxHostThreads = SettingsManager.Current.PortScanner_MaxHostThreads,
            MaxPortThreads = SettingsManager.Current.PortScanner_MaxPortThreads,
            Timeout = SettingsManager.Current.PortScanner_Timeout,
            Attempts = SettingsManager.Current.PortScanner_Attempts,
            ResolveHostname = SettingsManager.Current.PortScanner_ResolveHostname,
            ShowAllResults = SettingsManager.Current.PortScanner_ShowAllResults
        };
    }

    private void Stop()
    {
        IsCanceling = true;
        _cancellationTokenSource.Cancel();
    }

    /// <summary>
    /// Gets the results in the order shown in the grid: grouped by host, sorted within the group.
    /// </summary>
    /// <returns>Results.</returns>
    private IEnumerable<PortScannerPortInfo> GetResultsInGridOrder()
    {
        return ResultsView.Groups.Cast<CollectionViewGroup>()
            .SelectMany(group => group.Items.Cast<PortScannerPortInfo>());
    }

    /// <summary>
    /// Gets the selected results in the order shown in the grid (<see cref="SelectedResults"/> is in the order they
    /// were selected).
    /// </summary>
    /// <returns>Selected results.</returns>
    private List<PortScannerPortInfo> GetSelectedResultsInGridOrder()
    {
        var selectedResults = new HashSet<PortScannerPortInfo>(SelectedResults.Cast<PortScannerPortInfo>());

        return GetResultsInGridOrder().Where(selectedResults.Contains).ToList();
    }

    private Task Export()
    {
        var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

        var childWindow = new ExportChildWindow();

        var childWindowViewModel = new ExportViewModel(async instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            try
            {
                // Copy the results on the UI thread (they may still change while a scan is running) and
                // export them in the background, so a large export doesn't freeze the window. In the order
                // shown in the grid (Results is in the order the ports were finished).
                var results = instance.ExportAll
                    ? GetResultsInGridOrder().ToList()
                    : GetSelectedResultsInGridOrder();

                await Task.Run(() => ExportManager.Export(instance.FilePath, instance.FileType, results));
            }
            catch (Exception ex)
            {
                Log.Error("Error while exporting data as " + instance.FileType, ex);

                await DialogHelper.ShowMessageAsync(window, Strings.Error,
                   Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine +
                    Environment.NewLine + ex.Message, ChildWindowIcon.Error);
            }

            SettingsManager.Current.PortScanner_ExportFileType = instance.FileType;
            SettingsManager.Current.PortScanner_ExportFilePath = instance.FilePath;
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        }, [
            ExportFileType.Csv, ExportFileType.Xml, ExportFileType.Json
        ], true, SettingsManager.Current.PortScanner_ExportFileType,
        SettingsManager.Current.PortScanner_ExportFilePath);

        childWindow.Title = Strings.Export;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        return window.ShowChildWindowAsync(childWindow);
    }

    #endregion

    #region Events

    /// <summary>
    /// Filters the results once the user stopped typing (see <see cref="Search"/>).
    /// </summary>
    private void SearchDispatcherTimer_Tick(object sender, EventArgs e)
    {
        ApplySearch();
    }

    /// <summary>
    /// Gets the command to filter the results right away (Enter in the search box, see
    /// <see cref="NETworkManager.Utilities.WPF.SearchTextBoxHelper"/>).
    /// </summary>
    public ICommand ApplySearchCommand => new RelayCommand(_ => ApplySearch());

    /// <summary>
    /// Filters the results with the current <see cref="Search"/> right away (e.g. Enter in the search box).
    /// </summary>
    private void ApplySearch()
    {
        _searchDispatcherTimer.Stop();

        ResultsView.Refresh();
    }

    /// <summary>
    /// Handles changes of the settings. While no results are shown, the columns etc. follow the current
    /// settings (see <see cref="ScanOptions"/>). Any setting, so new columns don't need to be added here.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        UpdateScanOptionsIfNoResults();
    }

    /// <summary>
    /// While no results are shown and no scan is running, the columns etc. follow the current settings
    /// (see <see cref="ScanOptions"/>).
    /// </summary>
    private void UpdateScanOptionsIfNoResults()
    {
        if (!IsRunning && Results.Count == 0)
            ScanOptions = CreateScanOptions();
    }

    private void PortScanned(object sender, PortScannerPortScannedArgs e)
    {
        lock (_resultsBufferLock)
        {
            _resultsBuffer.Add(e.Args);
        }
    }

    /// <summary>
    /// Moves buffered scan results into <see cref="Results"/>. Always called on the UI thread -
    /// either from the <see cref="_resultsFlushTimer"/> tick, or from within the Dispatcher.Invoke
    /// call in <see cref="ScanComplete"/>.
    /// </summary>
    private void FlushResultsBuffer()
    {
        List<PortScannerPortInfo> itemsToAdd;

        lock (_resultsBufferLock)
        {
            if (_resultsBuffer.Count == 0)
                return;

            itemsToAdd = [.. _resultsBuffer];
            _resultsBuffer.Clear();
        }

        foreach (var item in itemsToAdd)
            Results.Add(item);
    }

    private void ProgressChanged(object sender, ProgressChangedArgs e)
    {
        if (sender is PortScanner portScanner)
            StoreProgress(portScanner);
    }

    /// <summary>
    /// Stores the current counters of the scanner for <see cref="FlushProgress"/>. Read from the scanner instead of
    /// the event args: events of different ports may arrive in a different order than they were raised, the
    /// scanner always has the current values.
    /// </summary>
    /// <param name="portScanner">Scanner of the running scan.</param>
    private void StoreProgress(PortScanner portScanner)
    {
        Volatile.Write(ref _latestPortsScanned, portScanner.PortsScanned);

        // Rescan: the scanner only counts the rescanned hosts
        var isRescan = Volatile.Read(ref _isRescan);

        Volatile.Write(ref _latestPortsOpen, (isRescan ? _rescanBasePortsOpen : 0) + portScanner.PortsOpen);
        Volatile.Write(ref _latestPortsClosed, (isRescan ? _rescanBasePortsClosed : 0) + portScanner.PortsClosed);
    }

    /// <summary>
    /// Pushes the latest buffered progress value into <see cref="PortsScanned"/>,
    /// <see cref="PortsOpen"/> and <see cref="PortsClosed"/>. Always called on the UI thread -
    /// same calling contexts as <see cref="FlushResultsBuffer"/>.
    /// </summary>
    private void FlushProgress()
    {
        PortsScanned = Volatile.Read(ref _latestPortsScanned);
        PortsOpen = Volatile.Read(ref _latestPortsOpen);
        PortsClosed = Volatile.Read(ref _latestPortsClosed);
    }

    /// <summary>
    /// Handles the ScanComplete event. This is the only place where a scan ends (completed, canceled
    /// or failed), so the state is reset exactly once and a new scan can't be started before it ran.
    /// Also called directly (on the UI thread) if the scan is canceled while the hosts are resolved.
    /// </summary>
    /// <param name="sender">The <see cref="PortScanner"/> instance raising the event, or this view model.</param>
    /// <param name="e">The <see cref="ScanCompleteArgs"/> instance containing the event data.</param>
    private void ScanComplete(object sender, ScanCompleteArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (sender is PortScanner portScanner)
            {
                portScanner.PortScanned -= PortScanned;
                portScanner.ScanComplete -= ScanComplete;
                portScanner.ProgressChanged -= ProgressChanged;
            }

            // Results are buffered - add the remaining ones, with the final counters
            _resultsFlushTimer?.Stop();
            FlushResultsBuffer();

            if (sender is PortScanner finishedScanner)
                StoreProgress(finishedScanner);

            FlushProgress();

            // Rescan: back to the progress of the last scan (open / closed include the rescan)
            if (_isRescan)
            {
                _isRescan = false;

                // Canceled / failed rescan: keep the previous results of the hosts (their rows were removed when the
                // rescan started, only some ports were scanned again)
                if (e.IsCanceled || e.Exception != null)
                {
                    ReplaceResults(_rescanIPAddresses, _rescanRemovedRows);

                    PortsOpen = _rescanPortsOpen;
                    PortsClosed = _rescanPortsClosed;
                }

                _rescanIPAddresses = null;
                _rescanRemovedRows = null;

                PortsToScan = _rescanPortsToScan;
                PortsScanned = _rescanPortsScanned;
            }

            if (e.IsCanceled)
            {
                StatusMessage = Strings.CanceledByUserMessage;
                IsStatusMessageDisplayed = true;
            }
            else if (e.Exception != null)
            {
                Log.Error("Error while scanning", e.Exception);

                StatusMessage = e.Exception.Message;
                IsStatusMessageDisplayed = true;
            }
            else if (Results.Count == 0)
            {
                // Keep a message from the preparation (e.g. hostnames that could not be resolved)
                StatusMessage = IsStatusMessageDisplayed
                    ? StatusMessage + Environment.NewLine + Strings.NoOpenPortsFound
                    : Strings.NoOpenPortsFound;
                IsStatusMessageDisplayed = true;
            }

            PreparingScan = false;
            IsCanceling = false;
            IsRunning = false;

            // E.g. canceled during the preparation, or nothing found
            UpdateScanOptionsIfNoResults();
        });
    }

    #endregion
}