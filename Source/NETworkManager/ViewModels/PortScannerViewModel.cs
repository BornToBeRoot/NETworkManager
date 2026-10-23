using log4net;
using MahApps.Metro.Controls;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Export;
using NETworkManager.Models.Network;
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
    public ObservableCollection<PortScannerPortInfo> Results
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

        // Result view
        ResultsView = CollectionViewSource.GetDefaultView(Results);
        ResultsView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(PortScannerPortInfo.HostAsString)));

        // Sort by IP address and port - results are added in the order the ports are finished
        ((ListCollectionView)ResultsView).CustomSort = Comparer<PortScannerPortInfo>.Create((x, y) =>
        {
            var result = IPAddressHelper.CompareIPAddresses(x.IPAddress, y.IPAddress);

            return result != 0 ? result : x.Port.CompareTo(y.Port);
        });

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
        var hostCount = HostRangeHelper.GetHostCount(HostRangeHelper.CreateListFromInput(Host));
        var portsPerHost = PortRangeHelper.ConvertPortRangeToIntArray(Ports).Length;
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
                SettingsManager.Current.Network_ResolveHostnamePreferIPv4, _cancellationTokenSource.Token);

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

        var portScanner = new PortScanner(new PortScannerOptions(
            SettingsManager.Current.PortScanner_MaxHostThreads,
            SettingsManager.Current.PortScanner_MaxPortThreads,
            SettingsManager.Current.PortScanner_Timeout,
            SettingsManager.Current.PortScanner_ResolveHostname,
            SettingsManager.Current.PortScanner_ShowAllResults
        ));

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

        portScanner.ScanAsync(hosts.hosts, ports, _cancellationTokenSource.Token);
    }

    private void Stop()
    {
        IsCanceling = true;
        _cancellationTokenSource.Cancel();
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
                ExportManager.Export(instance.FilePath, instance.FileType,
                    instance.ExportAll
                        ? Results
                        : new ObservableCollection<PortScannerPortInfo>(SelectedResults.Cast<PortScannerPortInfo>()
                            .ToArray()));
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
        Volatile.Write(ref _latestPortsScanned, e.Value);

        if (sender is PortScanner portScanner)
        {
            Volatile.Write(ref _latestPortsOpen, portScanner.PortsOpen);
            Volatile.Write(ref _latestPortsClosed, portScanner.PortsClosed);
        }
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

            // Results are buffered - add the remaining ones
            _resultsFlushTimer?.Stop();
            FlushResultsBuffer();
            FlushProgress();

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
        });
    }

    #endregion
}