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
    public PortScannerViewModel(Guid tabId, string host, string port)
    {
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

        // Convert ports to int array
        var ports = await PortRangeHelper.ConvertPortRangeToIntArrayAsync(Ports);

        PortsToScan = ports.Length * hosts.hosts.Count;

        PreparingScan = false;

        // Add host(s) to the history
        AddHostToHistory(Host);
        AddPortToHistory(Ports);

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
        portScanner.UserHasCanceled += UserHasCanceled;

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

    private void AddHostToHistory(string host)
    {
        // Create the new list
        var list = ListHelper.Modify(SettingsManager.Current.PortScanner_HostHistory.ToList(), host,
            SettingsManager.Current.General_HistoryListEntries);

        // Clear the old items
        SettingsManager.Current.PortScanner_HostHistory.Clear();
        OnPropertyChanged(nameof(Host)); // Raise property changed again, after the collection has been cleared

        // Fill with the new items
        list.ForEach(x => SettingsManager.Current.PortScanner_HostHistory.Add(x));
    }

    private void AddPortToHistory(string port)
    {
        // Create the new list
        var list = ListHelper.Modify(SettingsManager.Current.PortScanner_PortHistory.ToList(), port,
            SettingsManager.Current.General_HistoryListEntries);

        // Clear the old items
        SettingsManager.Current.PortScanner_PortHistory.Clear();
        OnPropertyChanged(nameof(Ports)); // Raise property changed again, after the collection has been cleared

        // Fill with the new items
        list.ForEach(x => SettingsManager.Current.PortScanner_PortHistory.Add(x));
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
    /// either from the <see cref="_resultsFlushTimer"/> tick, or from within a Dispatcher.Invoke
    /// call in <see cref="ScanComplete"/> / <see cref="UserHasCanceled"/>.
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

    private void ScanComplete(object sender, EventArgs e)
    {
        // Run in UI thread with lower priority than PortScanned event
        // to ensure all results are added first #3285
        Application.Current.Dispatcher.Invoke(() =>
        {
            _resultsFlushTimer?.Stop();
            FlushResultsBuffer();
            FlushProgress();

            if (Results.Count == 0)
            {
                StatusMessage = Strings.NoOpenPortsFound;
                IsStatusMessageDisplayed = true;
            }

            IsCanceling = false;
            IsRunning = false;
        }, DispatcherPriority.Background);
    }

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