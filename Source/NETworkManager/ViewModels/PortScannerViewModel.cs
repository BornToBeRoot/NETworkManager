using log4net;
using MahApps.Metro.Controls;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Controls;
using NETworkManager.Localization;
using NETworkManager.Localization.Resources;
using NETworkManager.Models;
using NETworkManager.Models.Export;
using NETworkManager.Models.Network;
using NETworkManager.Profiles;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Utilities.WPF;
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
public class PortScannerViewModel : ViewModelBase, IProfileManagerMinimal
{
    #region Variables
    private static readonly ILog Log = LogManager.GetLogger(typeof(PortScannerViewModel));

    /// <summary>
    /// Maximum effort to remove / add rows one by one when the results of hosts are replaced (rescan, see
    /// <see cref="ReplaceResults"/>) or buffered results are added (see <see cref="FlushResultsBuffer"/>). Above, the
    /// results are replaced with one reset (sorts and groups all rows once, but collapsed groups and the scroll
    /// position are lost).
    /// <para>
    /// The effort is a product (changed rows x all rows), not a row count: WPF inserts / removes every single row in
    /// its group of the sorted view with a linear search. Even one host with 1-65535 (max. 65,535 rows) exceeds it,
    /// e.g. adding 500 rows to 20,000 rows (10.25M) is done with a reset, 100 rows to 60,000 (6M) one by one. With
    /// multiple hosts, the results can have millions of rows (e.g. a /24 with 1-65535 and closed ports shown).
    /// </para>
    /// </summary>
    private const long MaxRowChangesOneByOne = 10_000_000;

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
    private int _latestPortsTimedOut;

    // Ports of the last scan, to rescan hosts with the same ports (and ScanOptions)
    private int[] _lastPorts;

    // Ports of the last scan as entered (e.g. "22; 80; 443"), for a profile created from a result
    private string _lastPortsInput;

    // Message of the tab, shown until the first scan has started or the ports are changed (see constructor)
    private string _message;

    // Rescan of shown hosts (see RescanHosts): their rows are removed and added again by the rescan. The
    // counters are the ones of the other hosts (base) plus the ones of the rescan; the progress of the last scan is
    // restored afterwards.
    private bool _isRescan;
    private int _rescanBasePortsOpen;
    private int _rescanBasePortsClosed;
    private int _rescanBasePortsTimedOut;
    private int _rescanPortsToScan;
    private int _rescanPortsScanned;

    // Rows and counters of the rescanned hosts before the rescan, restored if the rescan is canceled or fails
    private HashSet<IPAddress> _rescanIPAddresses;
    private List<PortScannerPortInfo> _rescanRemovedRows;
    private int _rescanPortsOpen;
    private int _rescanPortsClosed;
    private int _rescanPortsTimedOut;

    // Ports of the last scan without the rescanned hosts, the ports of the rescan are added when it is finished
    private int _rescanBasePortsScanned;

    // Rescan of the selected ports (see RescanSelectedPorts): their rows by IP address and port, updated in place
    // (not removed / added), and their states before the rescan
    private Dictionary<(IPAddress ipAddress, int port), PortScannerPortInfo> _rescanPortRows;
    private Dictionary<PortScannerPortInfo, PortState> _rescanPortPreviousStates;

    // Closed and timed out ports per host of the shown result. They are not shown unless enabled - a rescan needs them
    // to keep the counters of the other hosts (also if the last scan was canceled before all ports of a host were
    // scanned)
    private readonly Dictionary<IPAddress, (int closed, int timedOut)> _portsNotOpenPerHost = [];

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

            // The message of the tab is about the ports passed in (e.g. a port profile of the profile was not
            // found), it no longer applies when they are changed. Hidden only if it is still shown - not an error of
            // a scan that didn't start (e.g. invalid hosts)
            if (_message != null)
            {
                if (StatusMessage == _message)
                {
                    IsStatusMessageDisplayed = false;
                    StatusMessage = string.Empty;
                }

                _message = null;
            }
        }
    }

    private readonly InputSuggestions _portsSuggestions = InputSuggestions.ForPortScannerPorts();

    /// <summary>
    /// Gets the port profiles and the ports history for the ports input (grouped).
    /// </summary>
    public ICollectionView PortsSuggestions => _portsSuggestions.View;

    /// <summary>
    /// Gets the names of the port profiles for the Tab completion of the ports input.
    /// </summary>
    public IEnumerable<string> PortProfileNames => _portsSuggestions.ProfileNames;

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
    /// Gets the number of open ports per host (IP address) of the shown result, for the group headers. Counted when a
    /// result is added (see <see cref="AddResult"/>) - counting the rows of a group on every added row would take
    /// rows x rows with closed ports shown. Changed on the UI thread only, before the row is added.
    /// </summary>
    public Dictionary<IPAddress, int> OpenPortsPerHost { get; } = [];

    /// <summary>
    /// Gets a number that changes when <see cref="OpenPortsPerHost"/> changes without a row being added or removed
    /// (a port rescanned in place), so the group headers are updated.
    /// </summary>
    public int OpenPortsPerHostVersion
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }

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
    /// Gets or sets the number of ports found to be closed (the host refused the connection) so far.
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
    /// Gets or sets the number of ports without an answer (timed out) so far.
    /// </summary>
    public int PortsTimedOut
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
    private readonly SearchDelay _searchDelay;

    /// <summary>
    /// Gets or sets the search text for filtering the results (IP address, hostname, port, protocol, service, status).
    /// The results are filtered after a short delay (see <see cref="_searchDelay"/>).
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
    /// <param name="message">
    ///     Message shown below the input, also with the first scan (e.g. a port profile of the profile was not found).
    /// </param>
    public PortScannerViewModel(Guid tabId, string host, string port, bool start = true, string message = null)
    {
        _firstLoad = start;

        ConfigurationManager.Current.PortScannerTabCount++;

        _tabId = tabId;
        Host = host;
        Ports = port;

        // After the ports, changing them removes the message (see Ports)
        if (!string.IsNullOrEmpty(message))
        {
            _message = message;

            StatusMessage = message;
            IsStatusMessageDisplayed = true;
        }

        // Set collection view
        HostHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.PortScanner_HostHistory);

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

        _searchDelay = new SearchDelay(GlobalStaticConfiguration.SearchDispatcherTimerTimeSpan, ApplySearch);

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
    /// Gets the command to rescan the selected ports (rows), updated in place.
    /// </summary>
    public ICommand RescanSelectedPortsCommand => new RelayCommand(_ => RescanSelectedPorts());

    /// <summary>
    /// Gets the command to rescan the host of a group (with the ports and options of the last scan). The parameter is
    /// the group (<see cref="CollectionViewGroup"/>) of the host.
    /// </summary>
    public ICommand RescanHostCommand => new RelayCommand(RescanHostAction);

    private void RescanHostAction(object parameter)
    {
        if (parameter is CollectionViewGroup && GetHostRow(parameter) is { } row)
            RescanHosts([row.IPAddress]);
    }

    /// <summary>
    /// Gets the row of the host a command is for: the first row of a group (parameter of the context menu of a group
    /// header, one host per group), otherwise the selected result (context menu of a row).
    /// </summary>
    /// <param name="parameter">Command parameter, a <see cref="CollectionViewGroup"/> for a group header.</param>
    /// <returns>Row with the IP address and hostname of the host.</returns>
    private PortScannerPortInfo GetHostRow(object parameter)
    {
        return parameter is CollectionViewGroup { Items.Count: > 0 } group &&
               group.Items[0] is PortScannerPortInfo row
            ? row
            : SelectedResult;
    }

    /// <summary>
    /// Gets the command to open a host in another application. Parameter: the application (host of the selected
    /// result), or the application and the group of a group header (see <see cref="GetHostRow"/>).
    /// </summary>
    public ICommand RedirectDataToApplicationCommand => new RelayCommand(RedirectDataToApplicationAction);

    private void RedirectDataToApplicationAction(object parameter)
    {
        var (name, hostParameter) = parameter is object[] { Length: 2 } values
            ? (values[0], values[1])
            : (parameter, null);

        if (name is not ApplicationName applicationName)
            return;

        NavigationManager.Open(new ApplicationRequest(applicationName) { Target = GetHost(GetHostRow(hostParameter)) });
    }

    /// <summary>
    /// Gets the host of a result for other tools: the hostname (always a DNS hostname from the PTR record), otherwise
    /// the IP address.
    /// </summary>
    /// <param name="row">Result of the host.</param>
    /// <returns>Hostname or IP address.</returns>
    private static string GetHost(PortScannerPortInfo row)
    {
        return !string.IsNullOrEmpty(row.Hostname) ? row.Hostname : row.IPAddress.ToString();
    }

    /// <summary>
    /// Gets the command to perform a DNS lookup for the IP address of a host (see <see cref="GetHostRow"/>).
    /// </summary>
    public ICommand PerformDNSLookupIPAddressCommand =>
        new RelayCommand(parameter => PerformDNSLookupIPAddressAction(GetHostRow(parameter)));

    private static void PerformDNSLookupIPAddressAction(PortScannerPortInfo row)
    {
        NavigationManager.Open(new DNSLookupRequest { Target = row.IPAddress.ToString() });
    }

    /// <summary>
    /// Gets the command to perform a DNS lookup for the hostname of a host (see <see cref="GetHostRow"/>).
    /// </summary>
    public ICommand PerformDNSLookupHostnameCommand =>
        new RelayCommand(parameter => PerformDNSLookupHostnameAction(GetHostRow(parameter)));

    private static void PerformDNSLookupHostnameAction(PortScannerPortInfo row)
    {
        NavigationManager.Open(new DNSLookupRequest { Target = row.Hostname });
    }

    /// <summary>
    /// Gets the command to query the registration data of the IP address of a host via RDAP (see
    /// <see cref="GetHostRow"/>).
    /// </summary>
    public ICommand PerformRDAPQueryIPAddressCommand =>
        new RelayCommand(parameter => PerformRDAPQueryIPAddressAction(GetHostRow(parameter)));

    private static void PerformRDAPQueryIPAddressAction(PortScannerPortInfo row)
    {
        NavigationManager.Open(new RDAPRequest { Target = row.IPAddress.ToString() });
    }

    /// <summary>
    /// Gets the command to query the registration data of the hostname (its registered domain) of a host via RDAP
    /// (see <see cref="GetHostRow"/>).
    /// </summary>
    public ICommand PerformRDAPQueryHostnameCommand =>
        new RelayCommand(parameter => PerformRDAPQueryHostnameAction(GetHostRow(parameter)));

    private static void PerformRDAPQueryHostnameAction(PortScannerPortInfo row)
    {
        NavigationManager.Open(new RDAPRequest { Target = row.Hostname });
    }

    /// <summary>
    /// Gets the command to copy the open ports of the host of a group header (e.g. "22; 80; 443", sorted). Parameter:
    /// the <see cref="CollectionViewGroup"/>. Not available if the host has no open port.
    /// </summary>
    public ICommand CopyOpenPortsCommand => new RelayCommand(CopyOpenPortsAction, CopyOpenPorts_CanExecute);

    private static IEnumerable<int> GetOpenPorts(object parameter)
    {
        return parameter is CollectionViewGroup group
            ? group.Items.OfType<PortScannerPortInfo>().Where(x => x.State == PortState.Open).Select(x => x.Port)
                .Distinct().Order()
            : [];
    }

    private static bool CopyOpenPorts_CanExecute(object parameter)
    {
        return GetOpenPorts(parameter).Any();
    }

    private static void CopyOpenPortsAction(object parameter)
    {
        ClipboardHelper.SetClipboard(string.Join("; ", GetOpenPorts(parameter)));
    }

    /// <summary>
    /// Gets the command to add a host as a profile (with the ports of the shown scan), see <see cref="GetHostRow"/>.
    /// </summary>
    public ICommand AddProfileSelectedHostCommand =>
        new RelayCommand(parameter => AddProfileSelectedHostAction(GetHostRow(parameter)));

    private async void AddProfileSelectedHostAction(PortScannerPortInfo row)
    {
        // Only displayed - also trim the dot a single-label name still has (e.g. "gateway.")
        var hostname = DNSClientHelper.SanitizeHostname(row.Hostname);

        ProfileInfo profileInfo = new()
        {
            Name = string.IsNullOrEmpty(hostname) ? row.IPAddress.ToString() : hostname,
            Host = row.IPAddress.ToString(),

            // Additional data
            PortScanner_Ports = _lastPortsInput
        };

        var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

        await ProfileDialogManager.ShowAddProfileDialog(window, this, profileInfo, null,
            ApplicationName.PortScanner);
    }

    /// <summary>
    /// Gets the command to open the selected open web port in the Web Console.
    /// </summary>
    public ICommand OpenWebConsoleCommand => new RelayCommand(_ => OpenWebConsoleAction());

    private void OpenWebConsoleAction()
    {
        if (!IsWebConsoleAvailable)
            return;

        NavigationManager.Open(new ApplicationRequest(ApplicationName.WebConsole)
        {
            Target = WebConsoleUrlHelper.CreateUrl(GetHost(SelectedResult), SelectedResult.Port)
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

    /// <summary>
    /// Shows a message below the input, after the message that is already shown (e.g. the message of the tab or
    /// hostnames that could not be resolved). The messages are cleared when a scan starts.
    /// </summary>
    /// <param name="message">Message to show.</param>
    private void AddStatusMessage(string message)
    {
        StatusMessage = IsStatusMessageDisplayed ? StatusMessage + Environment.NewLine + message : message;
        IsStatusMessageDisplayed = true;
    }

    private async Task Start()
    {
        // Port profiles in the input (e.g. "Webserver; 8080") are replaced by their ports. The input is validated, but
        // a port profile may have been renamed or deleted since.
        if (!PortProfile.TryResolvePorts(Ports, SettingsManager.Current.PortScanner_PortProfiles,
                out var resolvedPorts))
        {
            StatusMessage = Strings.EnterValidPortsOrPortProfiles;
            IsStatusMessageDisplayed = true;

            return;
        }

        // Every port is kept in memory - ask before a large scan and refuse a too large one
        // (e.g. 10.0.0.0/8 with 1-65535) instead of running out of memory
        long hostCount;
        int portsPerHost;

        try
        {
            hostCount = HostRangeHelper.GetHostCount(HostRangeHelper.CreateListFromInput(Host));
            portsPerHost = PortRangeHelper.ConvertPortRangeToIntArray(resolvedPorts).Length;
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

        // Message of the tab (see constructor): still shown with the first scan
        if (_message != null)
        {
            StatusMessage = _message;
            IsStatusMessageDisplayed = true;

            _message = null;
        }

        IsRunning = true;
        PreparingScan = true;

        _resultsFlushTimer?.Stop();

        lock (_resultsBufferLock)
        {
            _resultsBuffer.Clear();
        }

        OpenPortsPerHost.Clear();
        Results.Clear();
        _portsNotOpenPerHost.Clear();

        // No results are shown until the scan's options are known - follow the current settings
        ScanOptions = CreateScanOptions();

        // Reset before hostname resolution too (not just after), so a cancellation during
        // resolution can't flush the previous scan's stale totals - PortsToScan = 0 also hides
        // the open/closed summary until the new scan's port count is known.
        PortsToScan = 0;
        PortsScanned = 0;
        PortsOpen = 0;
        PortsClosed = 0;
        PortsTimedOut = 0;
        Volatile.Write(ref _latestPortsScanned, 0);
        Volatile.Write(ref _latestPortsOpen, 0);
        Volatile.Write(ref _latestPortsClosed, 0);
        Volatile.Write(ref _latestPortsTimedOut, 0);

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

            ports = await PortRangeHelper.ConvertPortRangeToIntArrayAsync(resolvedPorts);
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
            AddStatusMessage(
                $"{Strings.TheFollowingHostnamesCouldNotBeResolved} {string.Join(", ", hosts.hostnamesNotResolved)}");

        PortsToScan = ports.Length * hosts.hosts.Count;

        // No host left (e.g. only hostnames that could not be resolved): don't start the scan, only the message above
        if (PortsToScan == 0)
        {
            ScanComplete(this, new ScanCompleteArgs(false));
            return;
        }

        PreparingScan = false;

        // Add host(s) to the history
        ListHelper.Modify(SettingsManager.Current.PortScanner_HostHistory, Host,
            SettingsManager.Current.General_HistoryListEntries);
        // A single port profile name is already in the list, only other inputs are added to the history
        if (PortProfile.FindByName(SettingsManager.Current.PortScanner_PortProfiles, Ports) == null)
            ListHelper.Modify(SettingsManager.Current.PortScanner_PortHistory, Ports,
                SettingsManager.Current.General_HistoryListEntries);

        ScanOptions = CreateScanOptions();

        _lastPorts = ports;
        _lastPortsInput = Ports;

        StartScanner(ScanOptions, (portScanner, ct) => portScanner.ScanAsync(hosts.hosts, ports, ct));
    }

    /// <summary>
    /// Rescans hosts with the ports and options of the last scan. Their rows are replaced by the results of the
    /// rescan (a port may be open / closed now).
    /// </summary>
    /// <param name="ipAddresses">Hosts to rescan.</param>
    private void RescanHosts(List<IPAddress> ipAddresses)
    {
        if (IsRunning || _lastPorts == null || ipAddresses.Count == 0)
            return;

        var rescannedIPAddresses = ipAddresses.ToHashSet();

        _rescanIPAddresses = rescannedIPAddresses;
        _rescanRemovedRows = [.. Results.Where(x => rescannedIPAddresses.Contains(x.IPAddress))];
        _rescanPortsOpen = PortsOpen;
        _rescanPortsClosed = PortsClosed;
        _rescanPortsTimedOut = PortsTimedOut;

        ReplaceResults(rescannedIPAddresses, []);

        var rescanPortCount = ipAddresses.Count * _lastPorts.Length;

        // The other hosts keep their counters: without the open (all shown), closed and timed out ports of the
        // rescanned hosts
        var removedPortsOpen = _rescanRemovedRows.Count(x => x.State == PortState.Open);
        var removedPortsClosed = ipAddresses.Sum(x => _portsNotOpenPerHost.GetValueOrDefault(x).closed);
        var removedPortsTimedOut = ipAddresses.Sum(x => _portsNotOpenPerHost.GetValueOrDefault(x).timedOut);

        _rescanPortsToScan = PortsToScan;
        _rescanPortsScanned = PortsScanned;
        _rescanBasePortsOpen = Math.Max(0, PortsOpen - removedPortsOpen);
        _rescanBasePortsClosed = Math.Max(0, PortsClosed - removedPortsClosed);
        _rescanBasePortsTimedOut = Math.Max(0, PortsTimedOut - removedPortsTimedOut);
        _rescanBasePortsScanned = Math.Max(0,
            PortsScanned - removedPortsOpen - removedPortsClosed - removedPortsTimedOut);
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
        Volatile.Write(ref _latestPortsTimedOut, _rescanBasePortsTimedOut);

        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        StartScanner(ScanOptions,
            (portScanner, ct) => portScanner.ScanAsync([.. ipAddresses.Select(x => (x, string.Empty))], _lastPorts, ct));
    }

    /// <summary>
    /// Rescans the selected ports (rows). The rows are updated in place - selection, collapsed groups and the scroll
    /// position stay. Every port is reported (also if closed ports are not shown), a port that is closed now keeps its
    /// row with the new state (like a host that is down now in the IP Scanner). Without a ping before the scan and
    /// without resolving the hostname (the rows keep theirs).
    /// </summary>
    private void RescanSelectedPorts()
    {
        if (IsRunning || _lastPorts == null)
            return;

        var rows = GetSelectedResultsInGridOrder();

        if (rows.Count == 0)
            return;

        _rescanPortRows = rows.DistinctBy(x => (x.IPAddress, x.Port)).ToDictionary(x => (x.IPAddress, x.Port));
        _rescanPortPreviousStates = rows.ToDictionary(x => x, x => x.State);

        // The other ports keep their counters, the rescanned ones are added by the scanner
        _rescanPortsToScan = PortsToScan;
        _rescanPortsScanned = PortsScanned;
        _rescanBasePortsOpen = Math.Max(0, PortsOpen - rows.Count(x => x.State == PortState.Open));
        _rescanBasePortsClosed = Math.Max(0, PortsClosed - rows.Count(x => x.State == PortState.Closed));
        _rescanBasePortsTimedOut = Math.Max(0, PortsTimedOut - rows.Count(x => x.State == PortState.TimedOut));
        _isRescan = true;

        IsStatusMessageDisplayed = false;
        IsRunning = true;

        _resultsFlushTimer?.Stop();

        lock (_resultsBufferLock)
        {
            _resultsBuffer.Clear();
        }

        PortsToScan = rows.Count;
        PortsScanned = 0;
        Volatile.Write(ref _latestPortsScanned, 0);
        Volatile.Write(ref _latestPortsOpen, _rescanBasePortsOpen);
        Volatile.Write(ref _latestPortsClosed, _rescanBasePortsClosed);
        Volatile.Write(ref _latestPortsTimedOut, _rescanBasePortsTimedOut);

        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        StartScanner(ScanOptions with { ShowAllResults = true, PingHostsBeforeScan = false, ResolveHostname = false },
            (portScanner, ct) => portScanner.ScanPortsAsync([.. rows.Select(x => (x.IPAddress, x.Port))], ct));
    }

    /// <summary>
    /// Changes the number of closed or timed out ports of a host (see <see cref="_portsNotOpenPerHost"/>), e.g. for a
    /// port rescanned in place.
    /// </summary>
    /// <param name="ipAddress">IP address of the host.</param>
    /// <param name="state">State of the port, only closed and timed out are counted.</param>
    /// <param name="change">+1 or -1.</param>
    private void UpdatePortsNotOpenPerHost(IPAddress ipAddress, PortState state, int change)
    {
        if (state is not (PortState.Closed or PortState.TimedOut))
            return;

        var (closed, timedOut) = _portsNotOpenPerHost.GetValueOrDefault(ipAddress);

        if (state == PortState.Closed)
            closed = Math.Max(0, closed + change);
        else
            timedOut = Math.Max(0, timedOut + change);

        _portsNotOpenPerHost[ipAddress] = (closed, timedOut);
    }

    /// <summary>
    /// Applies a result of a port rescan to the row of the port (see <see cref="RescanSelectedPorts"/>).
    /// </summary>
    /// <param name="result">Result of the rescan.</param>
    private void ApplyPortRescanResult(PortScannerPortInfo result)
    {
        if (!_rescanPortRows.TryGetValue((result.IPAddress, result.Port), out var row) || row.State == result.State)
            return;

        if (row.State == PortState.Open || result.State == PortState.Open)
        {
            OpenPortsPerHost[row.IPAddress] = Math.Max(0,
                OpenPortsPerHost.GetValueOrDefault(row.IPAddress) + (result.State == PortState.Open ? 1 : -1));

            OpenPortsPerHostVersion++;
        }

        row.State = result.State;
    }

    /// <summary>
    /// Replaces the rows of the given hosts. Few rows are removed / added one by one, so the groups of the other hosts
    /// keep their state (collapsed) and the scroll position stays. Many rows (or rows of a large result) with one reset
    /// instead: every single change searches the whole collection and the sorted, grouped view (e.g. a host with
    /// 1-65535 and closed ports shown would freeze the window for minutes).
    /// </summary>
    /// <param name="ipAddresses">Hosts whose rows are replaced.</param>
    /// <param name="rows">New rows of the hosts.</param>
    private void ReplaceResults(HashSet<IPAddress> ipAddresses, IReadOnlyCollection<PortScannerPortInfo> rows)
    {
        var oldRows = Results.Where(x => ipAddresses.Contains(x.IPAddress)).ToList();

        foreach (var ipAddress in ipAddresses)
            OpenPortsPerHost.Remove(ipAddress);

        if ((long)(oldRows.Count + rows.Count) * Results.Count > MaxRowChangesOneByOne)
        {
            foreach (var row in rows)
                CountOpenPort(row);

            Results.ReplaceAll(Results.Where(x => !ipAddresses.Contains(x.IPAddress)).Concat(rows));

            return;
        }

        try
        {
            foreach (var row in oldRows)
                Results.Remove(row);

            foreach (var row in rows)
                AddResult(row);
        }
        catch (ArgumentException ex) when (IsGroupVirtualizationSizeException(ex))
        {
            Log.Warn("Replacing the results of hosts failed in WPF (collapsed groups), the results are reset.", ex);

            // Rows of the hosts (old, not yet removed or already added) replaced at once, counted again
            foreach (var ipAddress in ipAddresses)
                OpenPortsPerHost.Remove(ipAddress);

            foreach (var row in rows)
                CountOpenPort(row);

            Results.ReplaceAll(Results.Where(x => !ipAddresses.Contains(x.IPAddress)).Concat(rows));
        }
    }

    /// <summary>
    /// Adds results one by one (see <see cref="AddResult"/>). If WPF fails to add a row (see
    /// <see cref="IsGroupVirtualizationSizeException"/>), the remaining results are added with one reset.
    /// </summary>
    /// <param name="results">Results to add.</param>
    private void AddResults(IReadOnlyList<PortScannerPortInfo> results)
    {
        for (var i = 0; i < results.Count; i++)
        {
            try
            {
                AddResult(results[i]);
            }
            catch (ArgumentException ex) when (IsGroupVirtualizationSizeException(ex))
            {
                Log.Warn("Adding a result failed in WPF (collapsed groups), the results are reset.", ex);

                // The row is already in the results (the exception comes from the notification of the view), the
                // reset rebuilds the view and its containers
                var remaining = results.Skip(i + 1).ToList();

                foreach (var result in remaining)
                    CountOpenPort(result);

                Results.ReplaceAll(Results.Concat(remaining));

                return;
            }
        }
    }

    /// <summary>
    /// Checks if an exception is the WPF bug of the virtualization with grouping: <c>VirtualizingStackPanel</c>
    /// computes a negative average row size while rows are added / removed and groups are collapsed (e.g. during a
    /// scan) and throws "Width and Height must be non-negative" (<see cref="Size"/>). Also on <c>main</c> (grouping,
    /// <c>IsVirtualizingWhenGrouping</c>, collapsible group headers).
    /// </summary>
    /// <param name="exception">Exception thrown while a row was added or removed.</param>
    /// <returns>True if it is this WPF exception.</returns>
    private static bool IsGroupVirtualizationSizeException(ArgumentException exception)
    {
        return exception.TargetSite?.DeclaringType == typeof(Size);
    }

    /// <summary>
    /// Adds a result, counted in <see cref="OpenPortsPerHost"/> before it is added (the group header is updated when
    /// the row is added).
    /// </summary>
    /// <param name="result">Result to add.</param>
    private void AddResult(PortScannerPortInfo result)
    {
        CountOpenPort(result);

        Results.Add(result);
    }

    /// <summary>
    /// Counts the result in <see cref="OpenPortsPerHost"/> if the port is open.
    /// </summary>
    /// <param name="result">Result to count.</param>
    private void CountOpenPort(PortScannerPortInfo result)
    {
        if (result.State == PortState.Open)
            OpenPortsPerHost[result.IPAddress] = OpenPortsPerHost.GetValueOrDefault(result.IPAddress) + 1;
    }

    /// <summary>
    /// Creates the scanner (with <see cref="ScanOptions"/>), subscribes to its events, starts the flush timer and
    /// the scan.
    /// </summary>
    /// <param name="options">Options of the scanner.</param>
    /// <param name="scan">Starts the scan (e.g. <see cref="PortScanner.ScanAsync"/>) with the token of the scan.</param>
    private void StartScanner(PortScannerOptions options, Action<PortScanner, CancellationToken> scan)
    {
        var portScanner = new PortScanner(options);

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

        scan(portScanner, _cancellationTokenSource.Token);
    }

    /// <summary>
    /// Creates the scan options from the current settings.
    /// </summary>
    /// <returns>Options for the <see cref="PortScanner"/>.</returns>
    private static PortScannerOptions CreateScanOptions()
    {
        return new PortScannerOptions
        {
            AutoAdjustThreads = SettingsManager.Current.PortScanner_AutoAdjustThreads,
            MaxHostThreads = SettingsManager.Current.PortScanner_MaxHostThreads,
            MaxPortThreads = SettingsManager.Current.PortScanner_MaxPortThreads,
            Timeout = SettingsManager.Current.PortScanner_Timeout,
            Attempts = SettingsManager.Current.PortScanner_Attempts,
            PingHostsBeforeScan = SettingsManager.Current.PortScanner_PingHostsBeforeScan,
            ICMPAttempts = SettingsManager.Current.PortScanner_ICMPAttempts,
            ICMPTimeout = SettingsManager.Current.PortScanner_ICMPTimeout,
            // Limits of the input - a value edited in the settings file would throw (negative) or make every ping fail
            // (.NET's Ping throws for more than 65500 bytes)
            ICMPBuffer = new byte[Math.Clamp(SettingsManager.Current.PortScanner_ICMPBuffer, 1, 65500)],
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
    /// Gets the command to filter the results right away (Enter in the search box, see
    /// <see cref="NETworkManager.Utilities.WPF.SearchTextBoxHelper"/>).
    /// </summary>
    public ICommand ApplySearchCommand => _searchDelay.ApplyCommand;

    /// <summary>
    /// Filters the results with the current <see cref="Search"/> right away (e.g. Enter in the search box).
    /// </summary>
    private void ApplySearch()
    {
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

        if (_rescanPortRows != null)
        {
            foreach (var item in itemsToAdd)
                ApplyPortRescanResult(item);

            return;
        }

        // Many rows (e.g. 1-65535 with closed ports shown): with one reset instead of one by one - WPF inserts every
        // added row into its group of the sorted view with a linear search of the group (rows x rows, the window hung
        // for minutes). A reset sorts and groups all rows once.
        if ((long)itemsToAdd.Count * (Results.Count + itemsToAdd.Count) > MaxRowChangesOneByOne)
        {
            foreach (var item in itemsToAdd)
                CountOpenPort(item);

            Results.ReplaceAll(Results.Concat(itemsToAdd));

            return;
        }

        AddResults(itemsToAdd);
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
        Volatile.Write(ref _latestPortsTimedOut,
            (isRescan ? _rescanBasePortsTimedOut : 0) + portScanner.PortsTimedOut);
    }

    /// <summary>
    /// Pushes the latest buffered progress value into <see cref="PortsScanned"/>,
    /// <see cref="PortsOpen"/>, <see cref="PortsClosed"/> and <see cref="PortsTimedOut"/>. Always called on the UI thread -
    /// same calling contexts as <see cref="FlushResultsBuffer"/>.
    /// </summary>
    private void FlushProgress()
    {
        PortsScanned = Volatile.Read(ref _latestPortsScanned);
        PortsOpen = Volatile.Read(ref _latestPortsOpen);
        PortsClosed = Volatile.Read(ref _latestPortsClosed);
        PortsTimedOut = Volatile.Read(ref _latestPortsTimedOut);
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

            // Canceled / failed rescan: keep the previous results of the hosts (their rows were removed when the
            // rescan started, only some ports were scanned again)
            var isPortRescan = _rescanPortRows != null;
            var isRescanDiscarded = _isRescan && !isPortRescan && (e.IsCanceled || e.Exception != null);

            // Closed and timed out ports per host of the shown result (of a rescan: replace the ones of the rescanned
            // hosts)
            if (isPortRescan)
            {
                // Rows updated in place: their previous state out, the current one in (not rescanned ports after a
                // cancel keep theirs)
                foreach (var (row, previousState) in _rescanPortPreviousStates)
                {
                    UpdatePortsNotOpenPerHost(row.IPAddress, previousState, -1);
                    UpdatePortsNotOpenPerHost(row.IPAddress, row.State, 1);
                }
            }
            else if (sender is PortScanner finishedPortScanner && !isRescanDiscarded)
            {
                if (_isRescan)
                {
                    foreach (var ipAddress in _rescanIPAddresses)
                        _portsNotOpenPerHost.Remove(ipAddress);
                }

                foreach (var (ipAddress, count) in finishedPortScanner.GetClosedAndTimedOutPortsPerHost())
                    _portsNotOpenPerHost[ipAddress] = count;
            }

            // Rescan: back to the progress of the last scan (open / closed / timed out include the rescan)
            if (_isRescan)
            {
                _isRescan = false;

                if (isPortRescan)
                {
                    // Counters from the rows - also correct if the rescan was canceled (ports not rescanned keep
                    // their state); the ports were already scanned by the last scan
                    var rows = _rescanPortPreviousStates.Keys;

                    PortsOpen = _rescanBasePortsOpen + rows.Count(x => x.State == PortState.Open);
                    PortsClosed = _rescanBasePortsClosed + rows.Count(x => x.State == PortState.Closed);
                    PortsTimedOut = _rescanBasePortsTimedOut + rows.Count(x => x.State == PortState.TimedOut);
                    PortsScanned = _rescanPortsScanned;

                    _rescanPortRows = null;
                    _rescanPortPreviousStates = null;
                }
                else if (isRescanDiscarded)
                {
                    ReplaceResults(_rescanIPAddresses, _rescanRemovedRows);

                    PortsOpen = _rescanPortsOpen;
                    PortsClosed = _rescanPortsClosed;
                    PortsTimedOut = _rescanPortsTimedOut;
                    PortsScanned = _rescanPortsScanned;
                }
                else
                {
                    // Ports of the other hosts + ports of the rescan
                    PortsScanned = _rescanBasePortsScanned + PortsScanned;
                }

                _rescanIPAddresses = null;
                _rescanRemovedRows = null;

                PortsToScan = _rescanPortsToScan;
            }

            // Keep a message from the preparation (e.g. the message of the tab or hostnames that could not be resolved)
            if (e.IsCanceled)
            {
                AddStatusMessage(Strings.CanceledByUserMessage);
            }
            else if (e.Exception != null)
            {
                Log.Error("Error while scanning", e.Exception);

                AddStatusMessage(e.Exception.Message);
            }
            else if (Results.Count == 0 && PortsToScan > 0)
            {
                AddStatusMessage(Strings.NoOpenPortsFound);
            }

            // Hosts that were not scanned because they didn't answer the ping before the scan (option)
            if (!e.IsCanceled && e.Exception == null && sender is PortScanner { HostsSkipped: > 0 } pingScanner)
                AddStatusMessage(string.Format(Strings.XHostsDidNotAnswerThePingAndWereNotScanned,
                    pingScanner.HostsSkipped));

            PreparingScan = false;
            IsCanceling = false;
            IsRunning = false;

            // E.g. canceled during the preparation, or nothing found
            UpdateScanOptionsIfNoResults();
        });
    }

    #endregion
}