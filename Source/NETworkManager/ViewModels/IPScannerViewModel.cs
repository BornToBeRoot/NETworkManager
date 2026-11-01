using log4net;
using MahApps.Metro.Controls;
using MahApps.Metro.IconPacks;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Controls;
using NETworkManager.Converters;
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace NETworkManager.ViewModels;

/// <summary>
/// ViewModel for the IP Scanner feature.
/// </summary>
public class IPScannerViewModel : ViewModelBase, IProfileManagerMinimal
{
    #region Variables
    private static readonly ILog Log = LogManager.GetLogger(typeof(IPScannerViewModel));

    private CancellationTokenSource _cancellationTokenSource;

    private readonly Guid _tabId;
    private bool _firstLoad = true;
    private bool _closed;

    // Background HostFound / HostUpdated events append here instead of hopping to the UI thread per
    // item - a DispatcherTimer periodically flushes this into the Results collection instead, so a large
    // scan doesn't flood the dispatcher queue with one BeginInvoke per host. One list for both, so an
    // update is always applied after its host was added and updates keep their order.
    // Entry: (host, null) = add the found host, (host, update) = apply the update to the host.
    private readonly List<(IPScannerHostInfo host, IPScannerHostUpdatedArgs update)> _resultsBuffer = [];
    private readonly Lock _resultsBufferLock = new();
    private DispatcherTimer _resultsFlushTimer;

    // Scanner of the running scan - events of another (finished) scanner are ignored
    private IPScanner _ipScanner;

    // Options of the last scan, to rescan hosts with the same options (the columns follow them, see ScanOptions)
    private IPScannerOptions _lastScanOptions;

    // Baseline of the last completed scan of this tab and its input: the next scan of the same input shows what
    // changed (see CompareWithPreviousScan). Only in memory, gone when the tab is closed.
    private Dictionary<IPAddress, IPScannerBaselineHost> _baseline;
    private string _baselineInput;

    // Baseline the shown result was compared with, to update the changes of rescanned hosts (null without comparison)
    private Dictionary<IPAddress, IPScannerBaselineHost> _comparisonBaseline;

    // Input of the running / last scan (not of a rescan of selected hosts)
    private string _scanInput;


    // Repeated scan: the rows of the last scan stay and are updated in place (selection, scroll position and expanded
    // details stay) - row per IP address, new results are copied into it (_repeatResults: result -> row), new hosts
    // are added (_repeatAdded). The values before the scan are restored if it is canceled or fails.
    private Dictionary<IPAddress, IPScannerHostInfo> _repeatRows;
    private readonly Dictionary<IPScannerHostInfo, IPScannerHostInfo> _repeatResults = [];
    private readonly List<IPScannerHostInfo> _repeatAdded = [];
    private Dictionary<IPScannerHostInfo, IPScannerHostInfo> _repeatSnapshots;
    private (int toScan, int scanned, int up, int down) _repeatCounters;

    // Rescan of shown hosts (see RescanSelectedHosts), null if no rescan is running: row per rescanned IP address
    // (every IP address is scanned once, see HostRangeHelper.ResolveAsync). The scanner creates new results - their
    // values are copied into the existing rows (selection and expanded details stay), the updates of a new result are
    // applied to the row it was copied to (_rescanResults: result -> row).
    private Dictionary<IPAddress, IPScannerHostInfo> _rescanRows;
    private readonly Dictionary<IPScannerHostInfo, IPScannerHostInfo> _rescanResults = [];

    // Values of the rescanned rows before the rescan (row -> copy), restored if the rescan is canceled or fails: a
    // found host is copied into its row with placeholders for the running probes, which would stay otherwise
    private Dictionary<IPScannerHostInfo, IPScannerHostInfo> _rescanSnapshots;

    // Counters of the last scan, restored after a rescan (the rescan shows its own progress)
    private int _rescanHostsToScan;
    private int _rescanHostsScanned;

    // Hosts of the last scan counted as up or down (a host is up when it was found, down when it was finished),
    // to recompute the down counter during a rescan
    private int _rescanHostsCounted;

    // Same reasoning as the results buffer above - ProgressChanged fires once per host
    // (unconditionally, unlike HostFound), so it's flushed to the bound property on the same
    // timer instead of updating it directly from the background thread on every event.
    private int _latestHostsScanned;
    private int _latestHostsUp;
    private int _latestHostsDown;

    /// <summary>
    /// Gets or sets the host or IP range to scan.
    /// </summary>
    public string Host
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            // Another input is not repeated (until it was scanned)
            Repeat?.OnInputChanged();

            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the collection view for the host history.
    /// </summary>
    public ICollectionView HostHistoryView { get; }

    /// <summary>
    /// Gets or sets a value indicating whether subnet detection is running.
    /// </summary>
    public bool IsSubnetDetectionRunning
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
    /// Gets the IPv4 subnets of the local network interfaces, loaded when the menu of the subnet detection is opened.
    /// </summary>
    public ObservableCollection<LocalSubnetInfo> LocalSubnets { get; } = [];

    // Incremented per load of LocalSubnets, so the result of an older load (menu opened twice) is ignored
    private int _localSubnetsLoadId;

    /// <summary>
    /// Gets or sets a value indicating whether the menu of the subnet detection is open. Opening it loads the subnets
    /// of the local network interfaces (<see cref="LocalSubnets"/>).
    /// </summary>
    public bool IsLocalSubnetMenuOpen
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();

            if (value)
                _ = LoadLocalSubnets();
        }
    }


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
    public ObservableCollection<IPScannerHostInfo> Results
    {
        get;
        set
        {
            if (Equals(value, field))
                return;

            field = value;
        }
    } = [];

    /// <summary>
    /// Delays filtering the results while the user is typing (see <see cref="Search"/>), so a large result is not
    /// filtered on every key press.
    /// </summary>
    private readonly SearchDelay _searchDelay;

    /// <summary>
    /// Gets or sets the search text for filtering the results (IP address, hostname, MAC address, vendor, ports).
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
    /// Gets the collection view for the scan results.
    /// </summary>
    public ICollectionView ResultsView { get; }

    /// <summary>
    /// Gets or sets the currently selected scan result.
    /// </summary>
    public IPScannerHostInfo SelectedResult
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
    /// Gets or sets the total number of hosts to scan.
    /// </summary>
    public int HostsToScan
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
    /// Gets or sets the number of hosts already scanned.
    /// </summary>
    public int HostsScanned
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
    /// Gets or sets the number of hosts found to be reachable so far.
    /// </summary>
    public int HostsUp
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
    /// Gets or sets the number of hosts found to be unreachable so far.
    /// </summary>
    public int HostsDown
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
    /// Gets or sets the number of hosts that are up now and were not up in the previous scan (see <see cref="HasComparison"/>).
    /// </summary>
    public int HostsNew
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
    /// Gets or sets the number of hosts that are not up now and were up in the previous scan (see <see cref="HasComparison"/>).
    /// </summary>
    public int HostsGone
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
    /// Gets or sets the number of hosts that are up with another MAC address than in the previous scan (see <see cref="HasComparison"/>).
    /// </summary>
    public int HostsMACChanged
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
    /// Gets or sets a value indicating whether the shown result is compared with the previous scan of the same input
    /// (change column and counters).
    /// </summary>
    public bool HasComparison
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
    /// Gets or sets the time the last scan was completed, null before (or while a scan is running).
    /// </summary>
    public DateTime? LastScanTime
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
    /// Gets the repeat of the scan ("Automatically scan every", countdown to the next scan).
    /// </summary>
    public ScanRepeater Repeat { get; }

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
    /// Gets the options of the scan whose results are shown. Columns, context menu and row details follow them
    /// instead of the current settings, which may have been changed since. While no results are shown, they
    /// follow the current settings.
    /// </summary>
    public IPScannerResultOptions ScanOptions
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
    /// Gets whether the details of every host are shown (setting <see cref="SettingsInfo.IPScanner_ExpandHostView"/>,
    /// follows the current setting). A row that was expanded / collapsed with its button keeps its state.
    /// </summary>
    public DataGridRowDetailsVisibilityMode HostDetailsVisibilityMode =>
        SettingsManager.Current.IPScanner_ExpandHostView
            ? DataGridRowDetailsVisibilityMode.Visible
            : DataGridRowDetailsVisibilityMode.Collapsed;

    /// <summary>
    /// Gets the available custom commands for the IP Scanner.
    /// </summary>
    public static IEnumerable<CustomCommandInfo> CustomCommands => SettingsManager.Current.IPScanner_CustomCommands;

    #endregion

    #region Constructor, load settings, shutdown

    /// <summary>
    /// Initializes a new instance of the <see cref="IPScannerViewModel"/> class.
    /// </summary>
    /// <param name="tabId">The unique identifier for the tab.</param>
    /// <param name="hostOrIPRange">The initial host or IP range to scan.</param>
    /// <param name="start">Starts it on the first load. If false, the input is only filled in.</param>
    public IPScannerViewModel(Guid tabId, string hostOrIPRange, bool start = true)
    {
        // Repeat of the scan (before the input is set, see its setter): not while a scan or rescan is running
        Repeat = new ScanRepeater(() => Start(isRepeat: true), () => IsRunning,
            SettingsManager.Current.IPScanner_AutoRefreshTime, x => SettingsManager.Current.IPScanner_AutoRefreshTime = x);

        _firstLoad = start;

        ConfigurationManager.Current.IPScannerTabCount++;

        _tabId = tabId;
        Host = hostOrIPRange;

        // Host history
        HostHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.IPScanner_HostHistory);

        // Columns etc. until the first scan
        ScanOptions = CreateResultOptions();

        SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;

        // Result view
        ResultsView = CollectionViewSource.GetDefaultView(Results);

        // Custom comparer to sort by IP address
        ((ListCollectionView)ResultsView).CustomSort = Comparer<IPScannerHostInfo>.Create((x, y) =>
            IPAddressHelper.CompareIPAddresses(x.PingInfo.IPAddress, y.PingInfo.IPAddress));

        ResultsView.Filter = o =>
        {
            if (string.IsNullOrEmpty(Search))
                return true;

            if (o is not IPScannerHostInfo info)
                return false;

            bool Matches(string value) => value?.Contains(Search, StringComparison.OrdinalIgnoreCase) == true;

            return Matches(info.PingInfo.IPAddress.ToString()) ||
                   Matches(info.Hostname) ||
                   Matches(info.MDNSHostname) ||
                   Matches(info.WebTitle) ||
                   Matches(info.WebServer) ||
                   Matches(info.WebPoweredBy) ||
                   Matches(info.WebRealm) ||
                   Matches(info.WebRedirectURL) ||
                   Matches(info.WebCertificate?.Subject) ||
                   Matches(info.WebCertificate?.Issuer) ||
                   Matches(info.SNMPName) ||
                   Matches(info.SNMPDescription) ||
                   Matches(info.SNMPLocation) ||
                   Matches(info.MACAddress) ||
                   Matches(info.Vendor) ||
                   (ScanOptions.OSGuessEnabled &&
                    Matches(IPScannerOSGuessToStringConverter.ToLocalizedString(info.OSGuess))) ||
                   Matches(IPScannerHostRoleToStringConverter.ToLocalizedString(info.Role)) ||
                   Matches(IPScannerHostChangeToStringConverter.ToLocalizedString(info.Change)) ||
                   info.Ports.Any(x => Matches(x.Port.ToString()) || Matches(x.LookupInfo.Service));
        };

        // Hostname, MAC address, vendor and ports arrive after the host was added - filter it again when they
        // change (without live filtering a host found before its hostname arrived would stay hidden). Only enabled
        // while searching, see ApplySearch.
        var liveShaping = (ICollectionViewLiveShaping)ResultsView;
        liveShaping.LiveFilteringProperties.Add(nameof(IPScannerHostInfo.Hostname));
        liveShaping.LiveFilteringProperties.Add(nameof(IPScannerHostInfo.MDNSHostname));
        liveShaping.LiveFilteringProperties.Add(nameof(IPScannerHostInfo.WebTitle));
        liveShaping.LiveFilteringProperties.Add(nameof(IPScannerHostInfo.Change));
        liveShaping.LiveFilteringProperties.Add(nameof(IPScannerHostInfo.WebServer));
        liveShaping.LiveFilteringProperties.Add(nameof(IPScannerHostInfo.WebPoweredBy));
        liveShaping.LiveFilteringProperties.Add(nameof(IPScannerHostInfo.WebRealm));
        liveShaping.LiveFilteringProperties.Add(nameof(IPScannerHostInfo.WebRedirectURL));
        liveShaping.LiveFilteringProperties.Add(nameof(IPScannerHostInfo.WebCertificate));
        liveShaping.LiveFilteringProperties.Add(nameof(IPScannerHostInfo.SNMPName));
        liveShaping.LiveFilteringProperties.Add(nameof(IPScannerHostInfo.SNMPDescription));
        liveShaping.LiveFilteringProperties.Add(nameof(IPScannerHostInfo.SNMPLocation));
        liveShaping.LiveFilteringProperties.Add(nameof(IPScannerHostInfo.MACAddress));
        liveShaping.LiveFilteringProperties.Add(nameof(IPScannerHostInfo.Vendor));
        liveShaping.LiveFilteringProperties.Add(nameof(IPScannerHostInfo.Ports));
        liveShaping.LiveFilteringProperties.Add(nameof(IPScannerHostInfo.OSGuess));

        _searchDelay = new SearchDelay(GlobalStaticConfiguration.SearchDispatcherTimerTimeSpan, ApplySearch);
    }

    /// <summary>
    /// Called when the view is loaded. Starts the scan if it's the first load and a host is specified.
    /// </summary>
    public void OnLoaded()
    {
        if (!_firstLoad)
            return;

        if (!string.IsNullOrEmpty(Host))
            _ = Start();

        _firstLoad = false;
    }

    #endregion

    #region ICommands & Actions

    /// <summary>
    /// Gets the command to start or stop the scan.
    /// </summary>
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
    /// Gets the command to detect the local subnet.
    /// </summary>
    public ICommand DetectSubnetCommand => new RelayCommand(_ => DetectSubnetAction());

    private void DetectSubnetAction()
    {
        _ = DetectSubnet();
    }

    /// <summary>
    /// Gets the command to scan the subnet of a local network interface (<see cref="LocalSubnetInfo"/>).
    /// </summary>
    public ICommand UseLocalSubnetCommand => new RelayCommand(UseLocalSubnetAction);

    private void UseLocalSubnetAction(object parameter)
    {
        if (parameter is not LocalSubnetInfo subnet)
            return;

        SetHostFromSubnetDetection(subnet.Subnet);
    }

    /// <summary>
    /// Gets the command to scan the subnets of all local network interfaces (without the link-local subnet).
    /// </summary>
    public ICommand UseAllLocalSubnetsCommand => new RelayCommand(_ => UseAllLocalSubnetsAction());

    private void UseAllLocalSubnetsAction()
    {
        var subnets = LocalSubnets.Where(x => !x.IsLinkLocal).Select(x => x.Subnet).Distinct().ToList();

        if (subnets.Count > 0)
            SetHostFromSubnetDetection(string.Join("; ", subnets));
    }

    /// <summary>
    /// Gets the command to open the IP Scanner settings (from the scan options menu).
    /// </summary>
    public ICommand OpenSettingsCommand => new RelayCommand(_ => NavigationManager.OpenSettings());

    /// <summary>
    /// Gets the command to redirect the selected host to another application.
    /// </summary>
    public ICommand RedirectDataToApplicationCommand => new RelayCommand(RedirectDataToApplicationAction);

    private void RedirectDataToApplicationAction(object name)
    {
        if (name is not ApplicationName applicationName)
            return;

        NavigationManager.Open(new ApplicationRequest(applicationName) { Target = GetSelectedHost() });
    }

    /// <summary>
    /// Gets the host of the selected result for other tools: the DNS hostname, otherwise the IP address. Not a NetBIOS
    /// name (shown as fallback), it may not be resolvable via DNS in the other tool.
    /// </summary>
    /// <returns>DNS hostname or IP address.</returns>
    private string GetSelectedHost()
    {
        return !string.IsNullOrEmpty(SelectedResult.DNSHostname)
            ? SelectedResult.DNSHostname
            : SelectedResult.PingInfo.IPAddress.ToString();
    }

    /// <summary>
    /// Gets the command to open the selected host in the Web Console (https / http depending on the open ports).
    /// </summary>
    public ICommand OpenWebConsoleCommand => new RelayCommand(_ => OpenWebConsoleAction());

    private void OpenWebConsoleAction()
    {
        // The port and scheme of the web page whose title was read
        if (Uri.TryCreate(SelectedResult.WebURL, UriKind.Absolute, out var webURL))
        {
            NavigationManager.Open(new ApplicationRequest(ApplicationName.WebConsole)
            {
                Target = WebConsoleUrlHelper.CreateUrl(GetSelectedHost(), webURL.Port, webURL.Scheme)
            });

            return;
        }

        // Ports may still be scanned - then http on the default port
        var openPorts = SelectedResult.IsPortScanPending
            ? []
            : SelectedResult.Ports.Where(x => x.State == PortState.Open).Select(x => x.Port);

        NavigationManager.Open(new ApplicationRequest(ApplicationName.WebConsole)
        {
            Target = WebConsoleUrlHelper.CreateUrl(GetSelectedHost(), openPorts)
        });
    }

    /// <summary>
    /// Gets the command to wake up the selected host (sends the magic packet to its MAC address).
    /// </summary>
    public ICommand WakeOnLANCommand => new RelayCommand(_ => WakeOnLANAction());

    private void WakeOnLANAction()
    {
        NavigationManager.Open(new WakeOnLANRequest { Target = SelectedResult.MACAddress });
    }

    /// <summary>
    /// Gets the command to perform a DNS lookup for the selected IP address.
    /// </summary>
    public ICommand PerformDNSLookupIPAddressCommand => new RelayCommand(_ => PerformDNSLookupIPAddressAction());

    private void PerformDNSLookupIPAddressAction()
    {
        NavigationManager.Open(new DNSLookupRequest { Target = SelectedResult.PingInfo.IPAddress.ToString() });
    }

    /// <summary>
    /// Gets the command to perform a DNS lookup for the selected hostname.
    /// </summary>
    public ICommand PerformDNSLookupHostnameCommand => new RelayCommand(_ => PerformDNSLookupHostnameAction());

    private void PerformDNSLookupHostnameAction()
    {
        NavigationManager.Open(new DNSLookupRequest { Target = SelectedResult.DNSHostname });
    }

    /// <summary>
    /// Gets the command to query the registration data of the selected IP address via RDAP.
    /// </summary>
    public ICommand PerformRDAPQueryIPAddressCommand => new RelayCommand(_ => PerformRDAPQueryIPAddressAction());

    private void PerformRDAPQueryIPAddressAction()
    {
        NavigationManager.Open(new RDAPRequest { Target = SelectedResult.PingInfo.IPAddress.ToString() });
    }

    /// <summary>
    /// Gets the command to query the registration data of the selected hostname (its registered domain) via RDAP.
    /// </summary>
    public ICommand PerformRDAPQueryHostnameCommand => new RelayCommand(_ => PerformRDAPQueryHostnameAction());

    private void PerformRDAPQueryHostnameAction()
    {
        NavigationManager.Open(new RDAPRequest { Target = SelectedResult.DNSHostname });
    }

    /// <summary>
    /// Gets the command to execute a custom command for the selected host.
    /// </summary>
    public ICommand CustomCommandCommand => new RelayCommand(CustomCommandAction);

    private void CustomCommandAction(object guid)
    {
        _ = CustomCommand(guid);
    }

    /// <summary>
    /// Gets the command to add the selected host as a profile.
    /// </summary>
    public ICommand AddProfileSelectedHostCommand => new RelayCommand(_ => AddProfileSelectedHostAction());

    private async void AddProfileSelectedHostAction()
    {
        // Only displayed - also trim the dot a single-label name still has (e.g. "gateway.")
        var hostname = DNSClientHelper.SanitizeHostname(SelectedResult.Hostname);

        ProfileInfo profileInfo = new()
        {
            Name = string.IsNullOrEmpty(hostname) ? SelectedResult.PingInfo.IPAddress.ToString() : hostname,
            Host = SelectedResult.PingInfo.IPAddress.ToString(),

            // Additional data
            WakeOnLAN_MACAddress = SelectedResult.MACAddress
        };

        var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

        await ProfileDialogManager.ShowAddProfileDialog(window, this, profileInfo, null,
            ApplicationName.IPScanner);
    }

    /// <summary>
    /// Gets the command to copy the selected ports to the clipboard.
    /// </summary>
    public ICommand CopySelectedPortsCommand => new RelayCommand(_ => CopySelectedPortsAction());

    private void CopySelectedPortsAction()
    {
        StringBuilder stringBuilder = new();

        foreach (var port in SelectedResult.Ports)
            stringBuilder.AppendLine(
                $"{port.Port}/{port.LookupInfo.Protocol},{ResourceTranslator.Translate(ResourceIdentifier.PortState, port.State)},{port.LookupInfo.Service},{port.LookupInfo.Description}");

        ClipboardHelper.SetClipboard(stringBuilder.ToString());
    }

    /// <summary>
    /// Gets the command to copy a value of all selected results (one per line, in the order shown in the grid).
    /// Parameter: <c>IPAddress</c>, <c>Hostname</c> or <c>MACAddress</c>.
    /// </summary>
    public ICommand CopySelectedResultsCommand => new RelayCommand(CopySelectedResultsAction);

    private void CopySelectedResultsAction(object parameter)
    {
        Func<IPScannerHostInfo, string> selector = parameter switch
        {
            "IPAddress" => x => x.PingInfo.IPAddress.ToString(),
            "Hostname" => x => x.Hostname,
            "MACAddress" => x => x.MACAddress,
            _ => null
        };

        if (selector == null)
            return;

        ClipboardHelper.SetClipboard(string.Join(Environment.NewLine,
            GetSelectedResultsInGridOrder().Select(selector).Where(x => !string.IsNullOrEmpty(x)).Distinct()));
    }

    /// <summary>
    /// Gets the command to rescan the selected hosts (with the options of the last scan).
    /// </summary>
    public ICommand RescanSelectedHostsCommand => new RelayCommand(_ => RescanSelectedHosts());

    /// <summary>
    /// Gets the command to export the scan results.
    /// </summary>
    public ICommand ExportCommand => new RelayCommand(_ => ExportAction());

    private void ExportAction()
    {
        _ = Export();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Starts the IP scan.
    /// </summary>
    private async Task Start(bool isRepeat = false)
    {
        // A host is only found if it answers a probe - without ping, port scan, NetBIOS and SNMP every host is
        // unreachable (ARP / NDP alone isn't enough: the neighbor entry is only created by the packets of the probes)
        if (!SettingsManager.Current.IPScanner_ICMPEnabled && !SettingsManager.Current.IPScanner_PortScanEnabled &&
            !SettingsManager.Current.IPScanner_NetBIOSEnabled && !SettingsManager.Current.IPScanner_SNMPEnabled)
        {
            StatusMessage = Strings.EnableAtLeastOneProbeToFindHostsMessage;
            IsStatusMessageDisplayed = true;

            return;
        }

        // Every IP address is kept in memory - ask before a large scan and refuse a too large one
        // (e.g. 10.0.0.0/1) instead of running out of memory
        long hostCount;

        try
        {
            hostCount = HostRangeHelper.GetHostCount(HostRangeHelper.CreateListFromInput(Host));
        }
        catch (Exception ex)
        {
            Log.Error("Error while counting the hosts", ex);

            StatusMessage = ex.Message;
            IsStatusMessageDisplayed = true;

            return;
        }

        if (hostCount > GlobalStaticConfiguration.IPScanner_MaxHostCount)
        {
            StatusMessage = string.Format(Strings.ScanTooManyIPAddressesMessage, hostCount.ToString("N0"),
                GlobalStaticConfiguration.IPScanner_MaxHostCount.ToString("N0"));
            IsStatusMessageDisplayed = true;

            return;
        }

        // Confirmed by the first scan, not asked again when the scan is repeated
        if (!isRepeat && hostCount > GlobalStaticConfiguration.IPScanner_ConfirmHostCount)
        {
            var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive) ??
                         Application.Current.MainWindow;

            if (!await DialogHelper.ShowConfirmationMessageAsync(window, Strings.Scan,
                    string.Format(Strings.ScanManyIPAddressesMessage, hostCount.ToString("N0")),
                    ChildWindowIcon.Warn, Strings.Scan))
                return;

            // The tab may have been closed while the dialog was open (OnClose only stops a running scan)
            if (_closed)
                return;
        }

        IsStatusMessageDisplayed = false;
        IsRunning = true;
        PreparingScan = true;

        _resultsFlushTimer?.Stop();

        lock (_resultsBufferLock)
        {
            _resultsBuffer.Clear();
        }

        if (isRepeat)
        {
            // The rows of the last scan stay (and its changes until the scan is completed), see ApplyRepeatResult
            _repeatRows = Results.DistinctBy(x => x.IPAddress).ToDictionary(x => x.IPAddress);
            _repeatResults.Clear();
            _repeatAdded.Clear();
            _repeatSnapshots = _repeatRows.Values.ToDictionary(x => x, x => x.Clone());
            _repeatCounters = (HostsToScan, HostsScanned, HostsUp, HostsDown);
        }
        else
        {
            Results.Clear();

            // Repeated from the end of this scan (see ScanComplete)
            Repeat.Disarm();

            HasComparison = false;
            _comparisonBaseline = null;
            HostsNew = 0;
            HostsGone = 0;
            HostsMACChanged = 0;
            LastScanTime = null;
        }

        // Compared with the previous scan of the same input when this scan is completed (see ScanComplete)
        _scanInput = Host?.Trim() ?? string.Empty;

        // No results are shown until the scan's options are known - follow the current settings
        ScanOptions = CreateResultOptions();

        // Reset before hostname resolution too (not just after), so a cancellation during
        // resolution can't flush the previous scan's stale totals - HostsToScan = 0 also hides
        // the up/down summary until the new scan's host count is known.
        HostsToScan = 0;
        HostsScanned = 0;
        HostsUp = 0;
        HostsDown = 0;
        Volatile.Write(ref _latestHostsScanned, 0);
        Volatile.Write(ref _latestHostsUp, 0);
        Volatile.Write(ref _latestHostsDown, 0);

        DragablzTabItem.SetTabHeader(_tabId, Host);

        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        // Resolve hostnames and convert the ports. Start() is fire-and-forget, so every exception has to be
        // handled here - otherwise the scan would never end (IsRunning stays true).
        (List<(IPAddress ipAddress, string hostname)> hosts, List<string> hostnamesNotResolved, int excludedCount) hosts;
        int[] ports;
        int[] webPagePorts;

        try
        {
            hosts = await HostRangeHelper.ResolveAsync(HostRangeHelper.CreateListFromInput(Host),
                SettingsManager.Current.Network_ResolveHostnamePreferIPv4, keepAllHostnames: false,
                _cancellationTokenSource.Token);

            ports = await PortRangeHelper.ConvertPortRangeToIntArrayAsync(
                SettingsManager.Current.IPScanner_PortScanPorts);

            webPagePorts = await PortRangeHelper.ConvertPortRangeToIntArrayAsync(
                SettingsManager.Current.IPScanner_WebPagePorts);
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

        // The exclusions removed every host (also if hostnames could not be resolved, keep their message)
        if (hosts.hosts.Count == 0 && hosts.excludedCount > 0)
            AddStatusMessage(Strings.AllIPAddressesAreExcludedMessage);

        HostsToScan = hosts.hosts.Count;

        // No host left (e.g. only hostnames that could not be resolved): don't start the scan, only the message above
        if (HostsToScan == 0)
        {
            ScanComplete(this, new ScanCompleteArgs(false));
            return;
        }

        PreparingScan = false;

        // Add host(s) to the history
        if (!isRepeat)
            ListHelper.Modify(SettingsManager.Current.IPScanner_HostHistory, Host,
            SettingsManager.Current.General_HistoryListEntries);

        var scanOptions = CreateScanOptions(ports, webPagePorts);

        ScanOptions = new IPScannerResultOptions(scanOptions);

        _lastScanOptions = scanOptions;

        // A repeated scan reports every host, so the row of a host that is down now is updated (see ApplyRepeatResult)
        StartScanner(isRepeat ? scanOptions with { ReportAllHosts = true } : scanOptions, hosts.hosts);
    }

    /// <summary>
    /// Rescans the selected hosts with the options of the last scan. The rows are updated in place; a host that is
    /// unreachable now is shown as down.
    /// </summary>
    private void RescanSelectedHosts()
    {
        if (IsRunning || _lastScanOptions == null)
            return;

        var selectedIPAddresses = GetSelectedResultsInGridOrder().Select(x => x.IPAddress).ToHashSet();

        if (selectedIPAddresses.Count == 0)
            return;

        // One row per IP address (DistinctBy only so a broken assumption can't throw)
        _rescanRows = Results.Where(x => selectedIPAddresses.Contains(x.IPAddress))
            .DistinctBy(x => x.IPAddress)
            .ToDictionary(x => x.IPAddress);

        _rescanResults.Clear();

        _rescanSnapshots = _rescanRows.Values.ToDictionary(x => x, x => x.Clone());

        _rescanHostsToScan = HostsToScan;
        _rescanHostsScanned = HostsScanned;
        _rescanHostsCounted = HostsUp + HostsDown;

        IsStatusMessageDisplayed = false;
        IsRunning = true;

        _resultsFlushTimer?.Stop();

        lock (_resultsBufferLock)
        {
            _resultsBuffer.Clear();
        }

        HostsToScan = _rescanRows.Count;
        HostsScanned = 0;
        Volatile.Write(ref _latestHostsScanned, 0);

        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        // Same options (columns, ports), but every host is reported - also if it is unreachable now
        StartScanner(CreateRescanOptions(_lastScanOptions), [.. _rescanRows.Keys.Select(x => (x, string.Empty))]);
    }

    /// <summary>
    /// Creates the scanner, subscribes to its events, starts the flush timer and the scan.
    /// </summary>
    /// <param name="scanOptions">Options of the scan.</param>
    /// <param name="hosts">Hosts to scan.</param>
    private void StartScanner(IPScannerOptions scanOptions, List<(IPAddress ipAddress, string hostname)> hosts)
    {
        var ipScanner = new IPScanner(scanOptions);

        _ipScanner = ipScanner;

        ipScanner.HostFound += HostFound;
        ipScanner.HostUpdated += HostUpdated;
        ipScanner.ScanComplete += ScanComplete;
        ipScanner.ProgressChanged += ProgressChanged;

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

        ipScanner.ScanAsync(hosts, _cancellationTokenSource.Token);
    }

    /// <summary>
    /// Creates the scan options from the current settings.
    /// </summary>
    /// <param name="ports">Ports to scan.</param>
    /// <param name="webPagePorts">Ports whose web page title is read (if open).</param>
    /// <returns>Options for the <see cref="IPScanner"/>.</returns>
    private static IPScannerOptions CreateScanOptions(int[] ports, int[] webPagePorts)
    {
        return new IPScannerOptions
        {
            MaxHostThreads = SettingsManager.Current.IPScanner_MaxHostThreads,
            MaxPortThreads = SettingsManager.Current.IPScanner_MaxPortThreads,
            ICMPEnabled = SettingsManager.Current.IPScanner_ICMPEnabled,
            OSGuessEnabled = SettingsManager.Current.IPScanner_OSGuessEnabled,
            ICMPAttempts = SettingsManager.Current.IPScanner_ICMPAttempts,
            ICMPTimeout = SettingsManager.Current.IPScanner_ICMPTimeout,
            ICMPBufferSize = GlobalStaticConfiguration.LimitICMPBuffer(SettingsManager.Current.IPScanner_ICMPBuffer),
            ResolveHostname = SettingsManager.Current.IPScanner_ResolveHostname,
            PortScanEnabled = SettingsManager.Current.IPScanner_PortScanEnabled,
            PortScanPorts = ports,
            PortScanTimeout = SettingsManager.Current.IPScanner_PortScanTimeout,
            PortScanAttempts = SettingsManager.Current.IPScanner_PortScanAttempts,
            NetBIOSEnabled = SettingsManager.Current.IPScanner_NetBIOSEnabled,
            NetBIOSTimeout = SettingsManager.Current.IPScanner_NetBIOSTimeout,
            ARPEnabled = SettingsManager.Current.IPScanner_ARPEnabled,
            ResolveMACAddress = SettingsManager.Current.IPScanner_ResolveMACAddress,
            MDNSEnabled = SettingsManager.Current.IPScanner_MDNSEnabled,
            MDNSTimeout = SettingsManager.Current.IPScanner_MDNSTimeout,
            WebPageEnabled = SettingsManager.Current.IPScanner_WebPageEnabled,
            WebPagePorts = webPagePorts,
            WebPageTimeout = SettingsManager.Current.IPScanner_WebPageTimeout,
            SNMPEnabled = SettingsManager.Current.IPScanner_SNMPEnabled,
            SNMPVersion = SettingsManager.Current.IPScanner_SNMPVersion,
            SNMPCommunity = SettingsManager.Current.IPScanner_SNMPCommunity,
            SNMPTimeout = SettingsManager.Current.IPScanner_SNMPTimeout,
            ShowAllResults = SettingsManager.Current.IPScanner_ShowAllResults,
            ReportAllHosts = false
        };
    }

    /// <summary>
    /// Creates the options for a rescan: the options of the last scan, every host is reported (also if it is
    /// unreachable now, so its row is updated).
    /// </summary>
    /// <param name="options">Options of the last scan.</param>
    /// <returns>Options for the rescan.</returns>
    private static IPScannerOptions CreateRescanOptions(IPScannerOptions options)
    {
        return options with { ReportAllHosts = true };
    }

    /// <summary>
    /// Creates the result options (columns, context menu and row details) from the current settings.
    /// </summary>
    /// <returns>Options for <see cref="ScanOptions"/>.</returns>
    private static IPScannerResultOptions CreateResultOptions()
    {
        // Same mapping of the settings as for a scan (the ports don't matter for the result options)
        return new IPScannerResultOptions(CreateScanOptions([], []));
    }

    /// <summary>
    /// Shows a message below the input, after the message that is already shown (e.g. hostnames that could not be
    /// resolved). The messages are cleared when a scan starts.
    /// </summary>
    /// <param name="message">Message to show.</param>
    private void AddStatusMessage(string message)
    {
        StatusMessage = IsStatusMessageDisplayed ? StatusMessage + Environment.NewLine + message : message;
        IsStatusMessageDisplayed = true;
    }

    /// <summary>
    /// Stops the IP scan.
    /// </summary>
    private void Stop()
    {
        IsCanceling = true;
        _cancellationTokenSource.Cancel();
    }

    /// <summary>
    /// Loads the subnets of the local network interfaces into <see cref="LocalSubnets"/>.
    /// </summary>
    private async Task LoadLocalSubnets()
    {
        var loadId = ++_localSubnetsLoadId;

        try
        {
            var subnets = NetworkInterface.GetLocalSubnets(await NetworkInterface.GetNetworkInterfacesAsync());

            // Menu opened again in the meantime - the newer load updates the list
            if (loadId != _localSubnetsLoadId)
                return;

            LocalSubnets.Clear();

            foreach (var subnet in subnets)
                LocalSubnets.Add(subnet);
        }
        catch (Exception ex)
        {
            Log.Error("Error while loading the subnets of the local network interfaces", ex);
        }
    }

    /// <summary>
    /// Sets the host to a detected / selected subnet.
    /// </summary>
    /// <param name="subnet">Subnet(s) to scan.</param>
    private void SetHostFromSubnetDetection(string subnet)
    {
        Host = subnet;

        // Fix: If the user clears the TextBox and then clicks again on the button, the TextBox remains empty...
        OnPropertyChanged(nameof(Host));
    }

    /// <summary>
    /// Attempts to detect the local subnet and updates the host information accordingly.
    /// </summary>
    /// <remarks>If the subnet or local IP address cannot be detected, an error message is displayed to the
    /// user. The method updates the Host property with the detected subnet in CIDR notation when successful.</remarks>
    /// <returns>A task that represents the asynchronous subnet detection operation.</returns>
    private async Task DetectSubnet()
    {
        IsSubnetDetectionRunning = true;

        try
        {
            // Try to detect local IP address based on routing to public IP
            var localIP = await NetworkInterface.DetectLocalIPAddressBasedOnRoutingAsync(IPAddress.Parse(GlobalStaticConfiguration.Dashboard_PublicIPv4Address));

            // Fallback: Try to detect local IP address from network interfaces -> Prefer non link-local addresses
            localIP ??= await NetworkInterface.DetectLocalIPAddressFromNetworkInterfaceAsync(System.Net.Sockets.AddressFamily.InterNetwork);

            // If local IP address detected, try to find subnetmask from network interfaces
            if (localIP != null)
            {
                var subnetDetected = false;

                // Get network interfaces, where local IP address is assigned
                var networkInterface = (await NetworkInterface.GetNetworkInterfacesAsync())
                    .FirstOrDefault(x => x.IPv4Address.Any(y => y.Item1.Equals(localIP)));

                // If found, get subnetmask
                if (networkInterface != null)
                {

                    // Find the correct IP address and the associated subnetmask
                    var ipAddressWithSubnet = networkInterface.IPv4Address.First(x => x.Item1.Equals(localIP));

                    Host = $"{ipAddressWithSubnet.Item1}/{Subnetmask.ConvertSubnetmaskToCidr(ipAddressWithSubnet.Item2)}";

                    subnetDetected = true;

                    // Fix: If the user clears the TextBox and then clicks again on the button, the TextBox remains empty...
                    OnPropertyChanged(nameof(Host));
                }

                // Show error message if subnet could not be detected
                if (!subnetDetected)
                {
                    var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

                    await DialogHelper.ShowMessageAsync(window, Strings.Error, Strings.CouldNotDetectSubnetmask, ChildWindowIcon.Error);
                }
            }
            else
            {
                var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

                await DialogHelper.ShowMessageAsync(window, Strings.Error, Strings.CouldNotDetectLocalIPAddressMessage, ChildWindowIcon.Error);
            }
        }
        catch (Exception ex)
        {
            Log.Error("Error while detecting the local subnet", ex);

            var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

            await DialogHelper.ShowMessageAsync(window, Strings.Error, ex.Message, ChildWindowIcon.Error);
        }
        finally
        {
            IsSubnetDetectionRunning = false;
        }
    }

    /// <summary>
    /// Executes a custom command.
    /// </summary>
    /// <param name="guid">The GUID of the custom command to execute.</param>
    private async Task CustomCommand(object guid)
    {
        if (guid is Guid id)
        {
            var info = (CustomCommandInfo)CustomCommands.FirstOrDefault(x => x.ID == id)?.Clone();

            if (info == null)
            {
                Log.Warn($"Custom command with ID \"{id}\" not found (deleted while the context menu was open?)");
                return;
            }

            // Replace vars. The command resolves the hostname via Windows (e.g. "gateway." -> "gateway")
            var hostname = DNSClientHelper.SanitizeHostname(SelectedResult.Hostname) ?? "";

            // The hostname comes from the network (PTR record or NetBIOS name) and could contain characters
            // like "&" that would run additional commands (e.g. "cmd.exe /c ... {{Hostname}}") - only use valid ones
            if (!string.IsNullOrEmpty(hostname) && !RegexHelper.HostnameOrDomainRegex().IsMatch(hostname))
            {
                Log.Warn($"Hostname \"{hostname}\" of {SelectedResult.PingInfo.IPAddress} is not a valid hostname and is not used for the custom command.");
                hostname = "";
            }
            var ipAddress = SelectedResult.PingInfo.IPAddress.ToString();

            info.FilePath = PlaceholderHelper.Resolve(info.FilePath,
                (PlaceholderHelper.Hostname, hostname),
                (PlaceholderHelper.IPAddress, ipAddress));

            if (!string.IsNullOrEmpty(info.Arguments))
            {
                info.Arguments = PlaceholderHelper.Resolve(info.Arguments,
                    (PlaceholderHelper.Hostname, hostname),
                    (PlaceholderHelper.IPAddress, ipAddress));
            }

            try
            {
                Utilities.CustomCommand.Run(info);
            }
            catch (Exception ex)
            {
                Log.Error("Error trying to run custom command", ex);

                var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

                await DialogHelper.ShowMessageAsync(window, Strings.Error, ex.Message, ChildWindowIcon.Error);
            }
        }
    }

    /// <summary>
    /// Gets the selected results in the order shown in the grid (<see cref="SelectedResults"/> is in the order they
    /// were selected).
    /// </summary>
    /// <returns>Selected results.</returns>
    private List<IPScannerHostInfo> GetSelectedResultsInGridOrder()
    {
        var selectedResults = new HashSet<IPScannerHostInfo>(SelectedResults.Cast<IPScannerHostInfo>());

        return ResultsView.Cast<IPScannerHostInfo>().Where(selectedResults.Contains).ToList();
    }

    /// <summary>
    /// Exports the scan results.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
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
                // Copy the results on the UI thread (they are still updated while a scan is running) and
                // export the copies in the background, so a large export doesn't freeze the window. In the
                // order shown in the grid (Results is in the order the hosts were found).
                var results = (instance.ExportAll
                        ? ResultsView.Cast<IPScannerHostInfo>()
                        : GetSelectedResultsInGridOrder())
                    .Select(x => x.Clone()).ToList();

                // Options of the shown scan (values of disabled probes are exported as empty values)
                var options = ScanOptions;

                await Task.Run(() => ExportManager.Export(instance.FilePath, instance.FileType, results, options));
            }
            catch (Exception ex)
            {
                Log.Error("Error while exporting data as " + instance.FileType, ex);

                await DialogHelper.ShowMessageAsync(window, Strings.Error,
                    Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine +
                    Environment.NewLine + ex.Message, ChildWindowIcon.Error);
            }

            SettingsManager.Current.IPScanner_ExportFileType = instance.FileType;
            SettingsManager.Current.IPScanner_ExportFilePath = instance.FilePath;
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        }, [
            ExportFileType.Csv, ExportFileType.Xml, ExportFileType.Json
        ], true, SettingsManager.Current.IPScanner_ExportFileType, SettingsManager.Current.IPScanner_ExportFilePath);

        childWindow.Title = Strings.Export;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        return window.ShowChildWindowAsync(childWindow);
    }

    /// <summary>
    /// Called when the tab is closed. Stops any running scan.
    /// </summary>
    public void OnClose()
    {
        // Prevent multiple calls
        if (_closed)
            return;

        _closed = true;

        // No repeated scan in the background
        Repeat.Close();

        SettingsManager.Current.PropertyChanged -= SettingsManager_PropertyChanged;

        // Stop scan
        if (IsRunning)
            Stop();

        ConfigurationManager.Current.IPScannerTabCount--;
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
        // Live filtering only while searching - without a search every host is shown anyway, but it would still
        // listen to the filtered properties of every row and filter a row again on every update
        var liveShaping = (ICollectionViewLiveShaping)ResultsView;
        var isSearching = !string.IsNullOrEmpty(Search);

        if (liveShaping.IsLiveFiltering != isSearching)
            liveShaping.IsLiveFiltering = isSearching;

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
        if (e.PropertyName == nameof(SettingsInfo.IPScanner_ExpandHostView))
            OnPropertyChanged(nameof(HostDetailsVisibilityMode));

        UpdateScanOptionsIfNoResults();
    }

    /// <summary>
    /// While no results are shown and no scan is running, the columns etc. follow the current settings
    /// (see <see cref="ScanOptions"/>).
    /// </summary>
    private void UpdateScanOptionsIfNoResults()
    {
        if (!IsRunning && Results.Count == 0)
            ScanOptions = CreateResultOptions();
    }

    /// <summary>
    /// Handles the HostFound event. Buffers the host, it's added to the list in <see cref="FlushResultsBuffer"/>.
    /// </summary>
    /// <param name="sender">The <see cref="IPScanner"/> instance raising the event.</param>
    /// <param name="e">The <see cref="IPScannerHostFoundArgs"/> instance containing the event data.</param>
    private void HostFound(object sender, IPScannerHostFoundArgs e)
    {
        if (!IsCurrentScanner(sender, nameof(HostFound)))
            return;

        lock (_resultsBufferLock)
        {
            _resultsBuffer.Add((e.Host, null));
        }
    }

    /// <summary>
    /// Handles the HostUpdated event. Buffers the update, it's applied in <see cref="FlushResultsBuffer"/>.
    /// </summary>
    /// <param name="sender">The <see cref="IPScanner"/> instance raising the event.</param>
    /// <param name="e">The <see cref="IPScannerHostUpdatedArgs"/> instance containing the event data.</param>
    private void HostUpdated(object sender, IPScannerHostUpdatedArgs e)
    {
        if (!IsCurrentScanner(sender, nameof(HostUpdated)))
            return;

        lock (_resultsBufferLock)
        {
            _resultsBuffer.Add((e.Host, e));
        }
    }

    /// <summary>
    /// Checks that an event comes from the scanner of the running scan. The scanner raises nothing after
    /// ScanComplete (all hosts and lookups are finished before), so this only guards against a broken contract.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="eventName">Name of the event (for the log).</param>
    /// <returns>True if the event comes from the scanner of the running scan.</returns>
    private bool IsCurrentScanner(object sender, string eventName)
    {
        if (ReferenceEquals(sender, Volatile.Read(ref _ipScanner)))
            return true;

        Log.Warn($"{eventName} of a finished scan ignored.");
        return false;
    }

    /// <summary>
    /// Adds buffered hosts to <see cref="Results"/> and applies the buffered updates, in the order they were
    /// raised. Always called on the UI thread - either from the <see cref="_resultsFlushTimer"/> tick, or from
    /// within the Dispatcher.Invoke call in <see cref="ScanComplete"/>.
    /// </summary>
    private void FlushResultsBuffer()
    {
        List<(IPScannerHostInfo host, IPScannerHostUpdatedArgs update)> items;

        lock (_resultsBufferLock)
        {
            if (_resultsBuffer.Count == 0)
                return;

            items = [.. _resultsBuffer];
            _resultsBuffer.Clear();
        }

        foreach (var (host, update) in items)
        {
            if (_rescanRows != null)
                ApplyRescanResult(host, update);
            else if (_repeatRows != null)
                ApplyRepeatResult(host, update);
            else if (update == null)
                Results.Add(host);
            else
                update.Apply();
        }
    }

    /// <summary>
    /// Applies a result of a repeated scan (see <see cref="Start"/>): the row of the host is updated in place, a host
    /// without a row is added if it is up (or unreachable hosts are shown). Every host is reported, also if it is down
    /// now - its row is updated, other down hosts are ignored.
    /// </summary>
    /// <param name="host">Result of the scan.</param>
    /// <param name="update">Update of the result, null if the host was found.</param>
    private void ApplyRepeatResult(IPScannerHostInfo host, IPScannerHostUpdatedArgs update)
    {
        if (update == null)
        {
            if (_repeatRows.TryGetValue(host.IPAddress, out var row))
            {
                row.CopyFrom(host);
                _repeatResults[host] = row;
            }
            else if (host.IsReachable || _lastScanOptions.ShowAllResults)
            {
                Results.Add(host);
                _repeatResults[host] = host;
                _repeatAdded.Add(host);
            }

            return;
        }

        if (_repeatResults.TryGetValue(update.Host, out var target))
            update.ApplyTo(target);
    }

    /// <summary>
    /// Applies a buffered result of a rescan to the existing rows: a found host is copied into the row of its IP
    /// address, its updates are applied to this row.
    /// </summary>
    /// <param name="host">Result of the rescan.</param>
    /// <param name="update">Update of the result, null if the host was found.</param>
    private void ApplyRescanResult(IPScannerHostInfo host, IPScannerHostUpdatedArgs update)
    {
        if (update == null)
        {
            if (!_rescanRows.TryGetValue(host.IPAddress, out var row))
                return;

            row.CopyFrom(host);

            _rescanResults[host] = row;

            return;
        }

        if (_rescanResults.TryGetValue(update.Host, out var target))
            update.ApplyTo(target);
    }

    /// <summary>
    /// Handles the ProgressChanged event. Stores the value for <see cref="FlushProgress"/> to
    /// pick up on the next timer tick, instead of updating the bound property directly from a
    /// background thread on every single host.
    /// </summary>
    /// <param name="sender">The <see cref="IPScanner"/> instance raising the event.</param>
    /// <param name="e">The <see cref="ProgressChangedArgs"/> instance containing the event data.</param>
    private void ProgressChanged(object sender, ProgressChangedArgs e)
    {
        if (sender is IPScanner ipScanner)
            StoreProgress(ipScanner);
    }

    /// <summary>
    /// Stores the current counters of the scanner for <see cref="FlushProgress"/>. Read from the scanner instead of
    /// the event args: events of different hosts may arrive in a different order than they were raised (e.g. a
    /// found host and a finished host at the same time), the scanner always has the current values.
    /// </summary>
    /// <param name="ipScanner">Scanner of the running scan.</param>
    private void StoreProgress(IPScanner ipScanner)
    {
        Volatile.Write(ref _latestHostsScanned, ipScanner.HostsScanned);
        Volatile.Write(ref _latestHostsUp, ipScanner.HostsUp);
        Volatile.Write(ref _latestHostsDown, ipScanner.HostsDown);
    }

    /// <summary>
    /// Pushes the latest buffered progress value into <see cref="HostsScanned"/>,
    /// <see cref="HostsUp"/> and <see cref="HostsDown"/>. Always called on the UI thread - same
    /// calling contexts as <see cref="FlushResultsBuffer"/>.
    /// </summary>
    private void FlushProgress()
    {
        HostsScanned = Volatile.Read(ref _latestHostsScanned);

        // Rescan: the counters of the scanner only cover the rescanned hosts - count the shown rows instead (every
        // reachable host is shown), unreachable = all counted hosts of the last scan minus the reachable ones. Not the
        // scanned hosts: a found host is counted as up before it is finished, so after a canceled scan there can be
        // more reachable rows than scanned hosts.
        if (_rescanRows != null)
        {
            HostsUp = Results.Count(x => x.IsReachable);
            HostsDown = Math.Max(0, _rescanHostsCounted - HostsUp);

            return;
        }

        HostsUp = Volatile.Read(ref _latestHostsUp);
        HostsDown = Volatile.Read(ref _latestHostsDown);
    }

    /// <summary>
    /// Handles the ScanComplete event. This is the only place where a scan ends (completed, canceled
    /// or failed), so the state is reset exactly once and a new scan can't be started before it ran.
    /// Also called directly (on the UI thread) if the scan is canceled while the hosts are resolved.
    /// </summary>
    /// <param name="sender">The <see cref="IPScanner"/> instance raising the event, or this view model.</param>
    /// <param name="e">The <see cref="ScanCompleteArgs"/> instance containing the event data.</param>
    private void ScanComplete(object sender, ScanCompleteArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            // A rescan of selected hosts updates rows, it is not a scan of the input (see CompareWithPreviousScan)
            var isRescan = _rescanRows != null;
            var rescannedRows = _rescanRows?.Values.ToList();

            // All hosts and lookups are finished when the scanner raises ScanComplete - add the remaining
            // buffered hosts and apply the remaining updates before the scan ends, with the final counters
            _resultsFlushTimer?.Stop();
            FlushResultsBuffer();

            if (sender is IPScanner finishedScanner)
                StoreProgress(finishedScanner);

            // Canceled / failed rescan: keep the previous results of the rows (before the counters are computed
            // from them)
            if (_rescanRows != null && (e.IsCanceled || e.Exception != null))
            {
                foreach (var (row, snapshot) in _rescanSnapshots)
                    row.CopyFrom(snapshot);
            }

            // Canceled / failed repeated scan: the rows of the last scan with their values (before the counters are
            // computed from them)
            var isRepeat = _repeatRows != null;
            var isRepeatDiscarded = isRepeat && (e.IsCanceled || e.Exception != null);

            if (isRepeatDiscarded)
            {
                foreach (var (row, snapshot) in _repeatSnapshots)
                    row.CopyFrom(snapshot);

                foreach (var added in _repeatAdded)
                    Results.Remove(added);
            }

            if (isRepeat)
            {
                _repeatRows = null;
                _repeatResults.Clear();
                _repeatAdded.Clear();
                _repeatSnapshots = null;
            }

            // Canceled / failed: results of running lookups never arrive - no "..." for them (and Copy works)
            if (e.IsCanceled || e.Exception != null)
            {
                foreach (var host in Results)
                    host.EndPendingLookups();
            }

            FlushProgress();

            // Rescan: back to the progress of the last scan
            if (_rescanRows != null)
            {
                _rescanRows = null;
                _rescanResults.Clear();
                _rescanSnapshots = null;

                HostsToScan = _rescanHostsToScan;
                HostsScanned = _rescanHostsScanned;
            }

            // Canceled / failed repeated scan: the counters of the restored rows (the last scan), not the partial ones
            if (isRepeatDiscarded)
                (HostsToScan, HostsScanned, HostsUp, HostsDown) = _repeatCounters;

            if (sender is IPScanner ipScanner)
            {
                ipScanner.HostFound -= HostFound;
                ipScanner.HostUpdated -= HostUpdated;
                ipScanner.ScanComplete -= ScanComplete;
                ipScanner.ProgressChanged -= ProgressChanged;

                if (ReferenceEquals(ipScanner, _ipScanner))
                    _ipScanner = null;
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
            else if (Results.Count == 0 && HostsToScan > 0)
            {
                // Keep a message from the preparation (e.g. hostnames that could not be resolved)
                AddStatusMessage(Strings.NoReachableHostsFound);
            }

            // A completed rescan: the changes of the rescanned hosts against the same previous scan (e.g. a gone host that is
            // up again)
            if (isRescan && !e.IsCanceled && e.Exception == null && _comparisonBaseline != null)
            {
                var rescannedIPAddresses = rescannedRows.Select(x => x.IPAddress).ToHashSet();

                ApplyComparison(rescannedRows, _comparisonBaseline.Where(x => rescannedIPAddresses.Contains(x.Key))
                    .ToDictionary(x => x.Key, x => x.Value));
            }

            // Only a completed scan of the input is compared and becomes the baseline of the next one. Repeated: a completed
            // scan, and a repeated scan that failed (tried again after the interval, see ScanRepeater.OnScanEnded)
            if (!isRescan)
            {
                var isCompleted = !e.IsCanceled && e.Exception == null && HostsToScan > 0;

                if (isCompleted)
                    CompareWithPreviousScan(isRepeat);

                Repeat.OnScanEnded(isCompleted, e.IsCanceled);
            }

            PreparingScan = false;
            IsCanceling = false;
            IsRunning = false;

            // E.g. canceled during the preparation, or nothing found
            UpdateScanOptionsIfNoResults();

        });
    }

    /// <summary>
    /// Shows a notification / plays a sound (see settings) with the changes of a repeated scan: one notification per
    /// scan, up to three changes one per line, otherwise the counters. Like the host up / down notification of the
    /// Ping Monitor: only new hosts = up, any gone / changed host = down.
    /// </summary>
    private void NotifyChanges()
    {
        var showPopup = SettingsManager.Current.IPScanner_ShowNotificationPopup;
        var playSound = SettingsManager.Current.IPScanner_NotificationSound;

        if (!showPopup && !playSound)
            return;

        var changes = Results.Where(x => x.Change != IPScannerHostChange.None).ToList();

        if (changes.Count == 0)
            return;

        changes.Sort((x, y) => IPAddressHelper.CompareIPAddresses(x.IPAddress, y.IPAddress));

        var isWarning = changes.Any(x => x.Change != IPScannerHostChange.New);

        if (playSound)
            NotificationManager.PlaySound(isWarning
                ? System.Media.SystemSounds.Exclamation
                : System.Media.SystemSounds.Asterisk);

        if (!showPopup)
            return;

        const int maxLines = 3;

        var message = changes.Count <= maxLines
            ? string.Join(Environment.NewLine, changes.Select(x =>
                $"{(string.IsNullOrEmpty(x.Hostname) ? x.IPAddress : $"{x.IPAddress} ({x.Hostname})")}: " +
                IPScannerHostChangeToStringConverter.ToLocalizedString(x.Change)))
            : string.Join(" · ", new[]
                {
                    (Count: HostsNew, Format: Strings.XNew), (Count: HostsGone, Format: Strings.XGone),
                    (Count: HostsMACChanged, Format: Strings.XMACChanged)
                }
                .Where(x => x.Count > 0).Select(x => string.Format(x.Format, x.Count)));

        NotificationManager.Show(isWarning ? PackIconMaterialKind.LanDisconnect : PackIconMaterialKind.LanConnect,
            isWarning ? "Red" : "#badc58", $"{Strings.IPScanner} · {_scanInput}", message,
            SettingsManager.Current.IPScanner_NotificationCloseTime);
    }

    /// <summary>
    /// Marks the changes of the rows against the baseline of the previous scan: new hosts and hosts with another MAC
    /// address, the hosts that are gone (a shown row is marked, otherwise a row is added). Updates the counters.
    /// </summary>
    /// <param name="rows">Rows to compare (all rows of a scan, the rescanned rows of a rescan).</param>
    /// <param name="baseline">Baseline of the previous scan (of these rows).</param>
    private void ApplyComparison(IReadOnlyCollection<IPScannerHostInfo> rows,
        IReadOnlyDictionary<IPAddress, IPScannerBaselineHost> baseline)
    {
        foreach (var host in rows)
        {
            host.Change = IPScannerHostChange.None;
            host.PreviousMACAddress = string.Empty;
        }

        var comparison = IPScannerScanComparer.Compare(baseline, IPScannerScanComparer.CreateBaseline(rows));

        foreach (var host in rows)
        {
            if (comparison.Changes.TryGetValue(host.IPAddress, out var change))
            {
                host.Change = change.Change;
                host.PreviousMACAddress = change.PreviousMACAddress;
            }
        }

        foreach (var (ipAddress, previous) in comparison.Gone)
        {
            // A shown down host (Show unreachable, a rescan) is marked, otherwise a row is added
            var row = rows.FirstOrDefault(x => x.IPAddress.Equals(ipAddress));

            if (row != null)
                row.MarkGone(previous);
            else
                Results.Add(IPScannerHostInfo.CreateGone(ipAddress, previous));
        }

        HostsNew = Results.Count(x => x.Change == IPScannerHostChange.New);
        HostsGone = Results.Count(x => x.Change == IPScannerHostChange.Gone);
        HostsMACChanged = Results.Count(x => x.Change == IPScannerHostChange.MACChanged);
    }

    /// <summary>
    /// Compares the completed scan with the previous completed scan of the same input in this tab: marks new hosts
    /// and hosts with another MAC address, adds the hosts that are gone (shown as down, also without "Show
    /// unreachable"). The scan becomes the baseline of the next one. Called on the UI thread.
    /// </summary>
    /// <param name="isRepeat">The scan was repeated automatically: changes are notified (see settings).</param>
    private void CompareWithPreviousScan(bool isRepeat)
    {
        // The changes of the last comparison (rows of a repeated scan)
        foreach (var host in Results)
        {
            host.Change = IPScannerHostChange.None;
            host.PreviousMACAddress = string.Empty;
        }

        var current = IPScannerScanComparer.CreateBaseline(Results);

        if (_baseline != null && string.Equals(_baselineInput, _scanInput, StringComparison.OrdinalIgnoreCase))
        {
            _comparisonBaseline = _baseline;

            ApplyComparison(Results.ToList(), _baseline);
            HasComparison = true;

            if (isRepeat)
                NotifyChanges();
        }
        else
        {
            _comparisonBaseline = null;
        }

        // A repeated scan updated the rows of hosts that are down now: only shown if they are gone (or unreachable hosts
        // are shown)
        if (!_lastScanOptions.ShowAllResults)
        {
            foreach (var host in Results.Where(x => !x.IsReachable && x.Change != IPScannerHostChange.Gone).ToList())
                Results.Remove(host);
        }

        _baseline = current;
        _baselineInput = _scanInput;
        LastScanTime = DateTime.Now;
    }

    #endregion
}