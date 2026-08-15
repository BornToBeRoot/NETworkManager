using log4net;
using MahApps.Metro.Controls;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Controls;
using NETworkManager.Localization;
using NETworkManager.Localization.Resources;
using NETworkManager.Models;
using NETworkManager.Models.EventSystem;
using NETworkManager.Models.Export;
using NETworkManager.Models.Network;
using NETworkManager.Profiles;
using NETworkManager.Settings;
using NETworkManager.Utilities;
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

    // Background HostScanned events append here instead of hopping to the UI thread per item -
    // a DispatcherTimer periodically flushes this into the Results collection instead, so a large
    // scan doesn't flood the dispatcher queue with one BeginInvoke per host.
    private readonly List<IPScannerHostInfo> _resultsBuffer = [];
    private readonly Lock _resultsBufferLock = new();
    private DispatcherTimer _resultsFlushTimer;

    // Same reasoning as the results buffer above - ProgressChanged fires once per host
    // (unconditionally, unlike HostScanned), so it's flushed to the bound property on the same
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
    public IPScannerViewModel(Guid tabId, string hostOrIPRange)
    {
        ConfigurationManager.Current.IPScannerTabCount++;

        _tabId = tabId;
        Host = hostOrIPRange;

        // Host history
        HostHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.IPScanner_HostHistory);

        // Result view
        ResultsView = CollectionViewSource.GetDefaultView(Results);

        // Custom comparer to sort by IP address
        ((ListCollectionView)ResultsView).CustomSort = Comparer<IPScannerHostInfo>.Create((x, y) =>
            IPAddressHelper.CompareIPAddresses(x.PingInfo.IPAddress, y.PingInfo.IPAddress));
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
    /// Gets the command to redirect the selected host to another application.
    /// </summary>
    public ICommand RedirectDataToApplicationCommand => new RelayCommand(RedirectDataToApplicationAction);

    private void RedirectDataToApplicationAction(object name)
    {
        if (name is not ApplicationName applicationName)
            return;

        var host = !string.IsNullOrEmpty(SelectedResult.Hostname)
            ? SelectedResult.Hostname
            : SelectedResult.PingInfo.IPAddress.ToString();

        EventSystem.RedirectToApplication(applicationName, host);
    }

    /// <summary>
    /// Gets the command to perform a DNS lookup for the selected IP address.
    /// </summary>
    public ICommand PerformDNSLookupIPAddressCommand => new RelayCommand(_ => PerformDNSLookupIPAddressAction());

    private void PerformDNSLookupIPAddressAction()
    {
        EventSystem.RedirectToApplication(ApplicationName.DNSLookup, SelectedResult.PingInfo.IPAddress.ToString());
    }

    /// <summary>
    /// Gets the command to perform a DNS lookup for the selected hostname.
    /// </summary>
    public ICommand PerformDNSLookupHostnameCommand => new RelayCommand(_ => PerformDNSLookupHostnameAction());

    private void PerformDNSLookupHostnameAction()
    {
        EventSystem.RedirectToApplication(ApplicationName.DNSLookup, SelectedResult.Hostname);
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
        ProfileInfo profileInfo = new()
        {
            Name = string.IsNullOrEmpty(SelectedResult.Hostname)
                ? SelectedResult.PingInfo.IPAddress.ToString()
                : SelectedResult.Hostname.TrimEnd('.'),
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
    private async Task Start()
    {
        IsStatusMessageDisplayed = false;
        IsRunning = true;
        PreparingScan = true;

        _resultsFlushTimer?.Stop();

        lock (_resultsBufferLock)
        {
            _resultsBuffer.Clear();
        }

        Results.Clear();

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

        // Resolve hostnames
        (List<(IPAddress ipAddress, string hostname)> hosts, List<string> hostnamesNotResolved) hosts;

        try
        {
            hosts = await HostRangeHelper.ResolveAsync(HostRangeHelper.CreateListFromInput(Host),
                SettingsManager.Current.Network_ResolveHostnamePreferIPv4, _cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            UserHasCanceled(this, EventArgs.Empty);
            return;
        }

        // Show error message if (some) hostnames could not be resolved
        if (hosts.hostnamesNotResolved.Count > 0)
        {
            StatusMessage =
                $"{Strings.TheFollowingHostnamesCouldNotBeResolved} {string.Join(", ", hosts.hostnamesNotResolved)}";
            IsStatusMessageDisplayed = true;
        }

        HostsToScan = hosts.hosts.Count;

        PreparingScan = false;

        // Add host(s) to the history
        AddHostToHistory(Host);

        var ipScanner = new IPScanner(new IPScannerOptions(
            SettingsManager.Current.IPScanner_MaxHostThreads,
            SettingsManager.Current.IPScanner_MaxPortThreads,
            SettingsManager.Current.IPScanner_ICMPAttempts,
            SettingsManager.Current.IPScanner_ICMPTimeout,
            new byte[SettingsManager.Current.IPScanner_ICMPBuffer],
            SettingsManager.Current.IPScanner_ResolveHostname,
            SettingsManager.Current.IPScanner_PortScanEnabled,
            await PortRangeHelper.ConvertPortRangeToIntArrayAsync(SettingsManager.Current.IPScanner_PortScanPorts),
            SettingsManager.Current.IPScanner_PortScanTimeout,
            SettingsManager.Current.IPScanner_NetBIOSEnabled,
            SettingsManager.Current.IPScanner_NetBIOSTimeout,
            SettingsManager.Current.IPScanner_ResolveMACAddress,
            SettingsManager.Current.IPScanner_ShowAllResults
        ));

        ipScanner.HostScanned += HostScanned;
        ipScanner.ScanComplete += ScanComplete;
        ipScanner.ProgressChanged += ProgressChanged;
        ipScanner.UserHasCanceled += UserHasCanceled;

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

        ipScanner.ScanAsync(hosts.hosts, _cancellationTokenSource.Token);
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
    /// Attempts to detect the local subnet and updates the host information accordingly.
    /// </summary>
    /// <remarks>If the subnet or local IP address cannot be detected, an error message is displayed to the
    /// user. The method updates the Host property with the detected subnet in CIDR notation when successful.</remarks>
    /// <returns>A task that represents the asynchronous subnet detection operation.</returns>
    private async Task DetectSubnet()
    {
        IsSubnetDetectionRunning = true;

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

        IsSubnetDetectionRunning = false;
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
                return; // ToDo: Log and error message

            // Replace vars
            var hostname = !string.IsNullOrEmpty(SelectedResult.Hostname) ? SelectedResult.Hostname.TrimEnd('.') : "";
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
    /// Adds the scanned host/range to the history.
    /// </summary>
    /// <param name="ipRange">The host or IP range to add.</param>
    private void AddHostToHistory(string ipRange)
    {
        // Create the new list
        var list = ListHelper.Modify([.. SettingsManager.Current.IPScanner_HostHistory], ipRange,
            SettingsManager.Current.General_HistoryListEntries);

        // Clear the old items
        SettingsManager.Current.IPScanner_HostHistory.Clear();
        OnPropertyChanged(nameof(Host)); // Raise property changed again, after the collection has been cleared

        // Fill with the new items
        list.ForEach(SettingsManager.Current.IPScanner_HostHistory.Add);
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
                ExportManager.Export(instance.FilePath, instance.FileType,
                    instance.ExportAll
                        ? Results
                        : new ObservableCollection<IPScannerHostInfo>(SelectedResults.Cast<IPScannerHostInfo>()
                            .ToArray()));
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

        // Stop scan
        if (IsRunning)
            Stop();

        ConfigurationManager.Current.IPScannerTabCount--;
    }

    #endregion

    #region Events

    /// <summary>
    /// Handles the HostScanned event. Adds the result to the list.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="IPScannerHostScannedArgs"/> instance containing the event data.</param>
    private void HostScanned(object sender, IPScannerHostScannedArgs e)
    {
        lock (_resultsBufferLock)
        {
            _resultsBuffer.Add(e.Args);
        }
    }

    /// <summary>
    /// Moves buffered scan results into <see cref="Results"/>. Always called on the UI thread -
    /// either from the <see cref="_resultsFlushTimer"/> tick, or from within a Dispatcher.Invoke
    /// call in <see cref="ScanComplete"/> / <see cref="UserHasCanceled"/>.
    /// </summary>
    private void FlushResultsBuffer()
    {
        List<IPScannerHostInfo> itemsToAdd;

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

    /// <summary>
    /// Handles the ProgressChanged event. Stores the value for <see cref="FlushProgress"/> to
    /// pick up on the next timer tick, instead of updating the bound property directly from a
    /// background thread on every single host.
    /// </summary>
    /// <param name="sender">The <see cref="IPScanner"/> instance raising the event.</param>
    /// <param name="e">The <see cref="ProgressChangedArgs"/> instance containing the event data.</param>
    private void ProgressChanged(object sender, ProgressChangedArgs e)
    {
        Volatile.Write(ref _latestHostsScanned, e.Value);

        if (sender is IPScanner ipScanner)
        {
            Volatile.Write(ref _latestHostsUp, ipScanner.HostsUp);
            Volatile.Write(ref _latestHostsDown, ipScanner.HostsDown);
        }
    }

    /// <summary>
    /// Pushes the latest buffered progress value into <see cref="HostsScanned"/>,
    /// <see cref="HostsUp"/> and <see cref="HostsDown"/>. Always called on the UI thread - same
    /// calling contexts as <see cref="FlushResultsBuffer"/>.
    /// </summary>
    private void FlushProgress()
    {
        HostsScanned = Volatile.Read(ref _latestHostsScanned);
        HostsUp = Volatile.Read(ref _latestHostsUp);
        HostsDown = Volatile.Read(ref _latestHostsDown);
    }

    /// <summary>
    /// Handles the ScanComplete event.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ScanComplete(object sender, EventArgs e)
    {
        // Run in UI thread with lower priority than HostScanned event
        // to ensure all results are added first #3285
        Application.Current.Dispatcher.Invoke(() =>
        {
            _resultsFlushTimer?.Stop();
            FlushResultsBuffer();
            FlushProgress();

            if (Results.Count == 0)
            {
                StatusMessage = Strings.NoReachableHostsFound;
                IsStatusMessageDisplayed = true;
            }

            IsCanceling = false;
            IsRunning = false;
        }, DispatcherPriority.Background);
    }

    /// <summary>
    /// Handles the UserHasCanceled event.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void UserHasCanceled(object sender, EventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            _resultsFlushTimer?.Stop();
            FlushResultsBuffer();
            FlushProgress();

            StatusMessage = Strings.CanceledByUserMessage;
            IsStatusMessageDisplayed = true;

            IsCanceling = false;
            IsRunning = false;
        });
    }

    #endregion
}