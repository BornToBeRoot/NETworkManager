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
using System.Text.RegularExpressions;
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

    private string _host;

    /// <summary>
    /// Gets or sets the host or IP range to scan.
    /// </summary>
    public string Host
    {
        get => _host;
        set
        {
            if (value == _host)
                return;

            _host = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the collection view for the host history.
    /// </summary>
    public ICollectionView HostHistoryView { get; }

    private bool _isSubnetDetectionRunning;

    /// <summary>
    /// Gets or sets a value indicating whether subnet detection is running.
    /// </summary>
    public bool IsSubnetDetectionRunning
    {
        get => _isSubnetDetectionRunning;
        set
        {
            if (value == _isSubnetDetectionRunning)
                return;

            _isSubnetDetectionRunning = value;
            OnPropertyChanged();
        }
    }


    private bool _isRunning;

    /// <summary>
    /// Gets or sets a value indicating whether the scan is currently running.
    /// </summary>
    public bool IsRunning
    {
        get => _isRunning;
        set
        {
            if (value == _isRunning)
                return;

            _isRunning = value;
            OnPropertyChanged();
        }
    }

    private bool _isCanceling;

    /// <summary>
    /// Gets or sets a value indicating whether the scan is being canceled.
    /// </summary>
    public bool IsCanceling
    {
        get => _isCanceling;
        set
        {
            if (value == _isCanceling)
                return;

            _isCanceling = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<IPScannerHostInfo> _results = [];

    /// <summary>
    /// Gets or sets the collection of scan results.
    /// </summary>
    public ObservableCollection<IPScannerHostInfo> Results
    {
        get => _results;
        set
        {
            if (Equals(value, _results))
                return;

            _results = value;
        }
    }

    /// <summary>
    /// Gets the collection view for the scan results.
    /// </summary>
    public ICollectionView ResultsView { get; }

    private IPScannerHostInfo _selectedResult;

    /// <summary>
    /// Gets or sets the currently selected scan result.
    /// </summary>
    public IPScannerHostInfo SelectedResult
    {
        get => _selectedResult;
        set
        {
            if (value == _selectedResult)
                return;

            _selectedResult = value;
            OnPropertyChanged();
        }
    }

    private IList _selectedResults = new ArrayList();

    /// <summary>
    /// Gets or sets the list of currently selected scan results (for multi-selection).
    /// </summary>
    public IList SelectedResults
    {
        get => _selectedResults;
        set
        {
            if (Equals(value, _selectedResults))
                return;

            _selectedResults = value;
            OnPropertyChanged();
        }
    }

    private int _hostsToScan;

    /// <summary>
    /// Gets or sets the total number of hosts to scan.
    /// </summary>
    public int HostsToScan
    {
        get => _hostsToScan;
        set
        {
            if (value == _hostsToScan)
                return;

            _hostsToScan = value;
            OnPropertyChanged();
        }
    }

    private int _hostsScanned;

    /// <summary>
    /// Gets or sets the number of hosts already scanned.
    /// </summary>
    public int HostsScanned
    {
        get => _hostsScanned;
        set
        {
            if (value == _hostsScanned)
                return;

            _hostsScanned = value;
            OnPropertyChanged();
        }
    }

    private bool _preparingScan;

    /// <summary>
    /// Gets or sets a value indicating whether the scan is being prepared.
    /// </summary>
    public bool PreparingScan
    {
        get => _preparingScan;
        set
        {
            if (value == _preparingScan)
                return;

            _preparingScan = value;
            OnPropertyChanged();
        }
    }

    private bool _isStatusMessageDisplayed;

    /// <summary>
    /// Gets or sets a value indicating whether the status message is displayed.
    /// </summary>
    public bool IsStatusMessageDisplayed
    {
        get => _isStatusMessageDisplayed;
        set
        {
            if (value == _isStatusMessageDisplayed)
                return;

            _isStatusMessageDisplayed = value;
            OnPropertyChanged();
        }
    }

    private string _statusMessage;

    /// <summary>
    /// Gets the status message to display.
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (value == _statusMessage)
                return;

            _statusMessage = value;
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
            Start().ConfigureAwait(false);

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
            Start().ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the command to detect the local subnet.
    /// </summary>
    public ICommand DetectSubnetCommand => new RelayCommand(_ => DetectSubnetAction());

    private void DetectSubnetAction()
    {
        DetectSubnet().ConfigureAwait(false);
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
        CustomCommand(guid).ConfigureAwait(false);
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
        Export().ConfigureAwait(false);
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

        Results.Clear();

        DragablzTabItem.SetTabHeader(_tabId, Host);

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
        HostsScanned = 0;

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

            info.FilePath = Regex.Replace(info.FilePath, "\\$\\$hostname\\$\\$", hostname, RegexOptions.IgnoreCase);
            info.FilePath = Regex.Replace(info.FilePath, "\\$\\$ipaddress\\$\\$", ipAddress, RegexOptions.IgnoreCase);

            if (!string.IsNullOrEmpty(info.Arguments))
            {
                info.Arguments = Regex.Replace(info.Arguments, "\\$\\$hostname\\$\\$", hostname,
                    RegexOptions.IgnoreCase);
                info.Arguments = Regex.Replace(info.Arguments, "\\$\\$ipaddress\\$\\$", ipAddress,
                    RegexOptions.IgnoreCase);
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
        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            new Action(delegate
            {
                Results.Add(e.Args);
            }));
    }

    /// <summary>
    /// Handles the ProgressChanged event. Updates the progress.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="ProgressChangedArgs"/> instance containing the event data.</param>
    private void ProgressChanged(object sender, ProgressChangedArgs e)
    {
        HostsScanned = e.Value;
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
        StatusMessage = Strings.CanceledByUserMessage;
        IsStatusMessageDisplayed = true;

        IsCanceling = false;
        IsRunning = false;
    }

    #endregion
}