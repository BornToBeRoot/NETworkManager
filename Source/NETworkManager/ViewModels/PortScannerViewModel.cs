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
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Export;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;

namespace NETworkManager.ViewModels;

public class PortScannerViewModel : ViewModelBase
{
    #region Variables

    private readonly IDialogCoordinator _dialogCoordinator;

    private CancellationTokenSource _cancellationTokenSource;

    private readonly Guid _tabId;
    private bool _firstLoad = true;
    private bool _closed;

    private string _host;

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

    public ICollectionView HostHistoryView { get; }

    private string _ports;

    public string Ports
    {
        get => _ports;
        set
        {
            if (value == _ports)
                return;

            _ports = value;
            OnPropertyChanged();
        }
    }

    public ICollectionView PortsHistoryView { get; }

    private bool _isRunning;

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

    private ObservableCollection<PortScannerPortInfo> _results = [];

    public ObservableCollection<PortScannerPortInfo> Results
    {
        get => _results;
        set
        {
            if (_results != null && value == _results)
                return;

            _results = value;
        }
    }

    public ICollectionView ResultsView { get; }

    private PortScannerPortInfo _selectedResult;

    public PortScannerPortInfo SelectedResult
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

    private int _portsToScan;

    public int PortsToScan
    {
        get => _portsToScan;
        set
        {
            if (value == _portsToScan)
                return;

            _portsToScan = value;
            OnPropertyChanged();
        }
    }

    private int _portsScanned;

    public int PortsScanned
    {
        get => _portsScanned;
        set
        {
            if (value == _portsScanned)
                return;

            _portsScanned = value;
            OnPropertyChanged();
        }
    }

    private bool _preparingScan;

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

    #endregion

    #region Constructor, load settings, shutdown

    public PortScannerViewModel(IDialogCoordinator instance, Guid tabId, string host, string port)
    {
        _dialogCoordinator = instance;

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
            Start().ConfigureAwait(false);

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
               !((MetroWindow)Application.Current.MainWindow)
                   .IsAnyDialogOpen;
    }

    private void OpenPortProfileSelectionAction()
    {
        OpenPortProfileSelection().ConfigureAwait(false);
    }

    public ICommand ScanCommand => new RelayCommand(_ => ScanAction(), Scan_CanExecute);

    private bool Scan_CanExecute(object parameter)
    {
        return Application.Current.MainWindow != null &&
               !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen;
    }

    private void ScanAction()
    {
        if (IsRunning)
            Stop();
        else
            Start().ConfigureAwait(false);
    }

    public ICommand ExportCommand => new RelayCommand(_ => ExportAction());

    private void ExportAction()
    {
        Export().ConfigureAwait(false);
    }

    #endregion

    #region Methods

    private async Task OpenPortProfileSelection()
    {
        var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

        var customDialog = new CustomDialog
        {
            Title = Strings.SelectPortProfile
        };

        var viewModel = new PortProfilesViewModel(async instance =>
        {
            await _dialogCoordinator.HideMetroDialogAsync(window, customDialog);

            Ports = string.Join("; ", instance.GetSelectedPortProfiles().Select(x => x.Ports));
        }, async _ => { await _dialogCoordinator.HideMetroDialogAsync(window, customDialog); });

        customDialog.Content = new PortProfilesDialog
        {
            DataContext = viewModel
        };

        await _dialogCoordinator.ShowMetroDialogAsync(window, customDialog);
    }

    private async Task Start()
    {
        IsStatusMessageDisplayed = false;
        StatusMessage = string.Empty;

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

        // Convert ports to int array
        var ports = await PortRangeHelper.ConvertPortRangeToIntArrayAsync(Ports);

        PortsToScan = ports.Length * hosts.hosts.Count;
        PortsScanned = 0;

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

        portScanner.ScanAsync(hosts.hosts, ports, _cancellationTokenSource.Token);
    }

    private void Stop()
    {
        IsCanceling = true;
        _cancellationTokenSource.Cancel();
    }

    private async Task Export()
    {
        var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

        var customDialog = new CustomDialog
        {
            Title = Strings.Export
        };

        var exportViewModel = new ExportViewModel(async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(window, customDialog);

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
                    var settings = AppearanceManager.MetroDialog;
                    settings.AffirmativeButtonText = Strings.OK;

                    await _dialogCoordinator.ShowMessageAsync(window, Strings.Error,
                        Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine +
                        Environment.NewLine + ex.Message, MessageDialogStyle.Affirmative, settings);
                }

                SettingsManager.Current.PortScanner_ExportFileType = instance.FileType;
                SettingsManager.Current.PortScanner_ExportFilePath = instance.FilePath;
            }, _ => { _dialogCoordinator.HideMetroDialogAsync(window, customDialog); },
            [ExportFileType.Csv, ExportFileType.Xml, ExportFileType.Json],
            true,
            SettingsManager.Current.PortScanner_ExportFileType, SettingsManager.Current.PortScanner_ExportFilePath);

        customDialog.Content = new ExportDialog
        {
            DataContext = exportViewModel
        };

        await _dialogCoordinator.ShowMetroDialogAsync(window, customDialog);
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
        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            new Action(delegate { Results.Add(e.Args); }));
    }

    private void ProgressChanged(object sender, ProgressChangedArgs e)
    {
        PortsScanned = e.Value;
    }

    private void ScanComplete(object sender, EventArgs e)
    {
        if (Results.Count == 0)
        {
            StatusMessage = Strings.NoOpenPortsFound;
            IsStatusMessageDisplayed = true;
        }

        IsCanceling = false;
        IsRunning = false;
    }

    private void UserHasCanceled(object sender, EventArgs e)
    {
        StatusMessage = Strings.CanceledByUserMessage;
        IsStatusMessageDisplayed = true;

        IsCanceling = false;
        IsRunning = false;
    }

    #endregion
}