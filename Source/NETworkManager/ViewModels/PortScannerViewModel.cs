using System.Windows.Input;
using System.Windows;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using NETworkManager.Settings;
using System.Collections.Generic;
using NETworkManager.Models.Network;
using System.Threading;
using System.Net;
using System.Windows.Threading;
using System.ComponentModel;
using System.Windows.Data;
using System.Linq;
using NETworkManager.Utilities;
using Dragablz;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Controls;
using NETworkManager.Models.Export;
using NETworkManager.Views;
using System.Threading.Tasks;

namespace NETworkManager.ViewModels;

public class PortScannerViewModel : ViewModelBase
{
    #region Variables
    private readonly IDialogCoordinator _dialogCoordinator;

    private CancellationTokenSource _cancellationTokenSource;

    private readonly int _tabId;
    private bool _firstLoad = true;

    private string _lastSortDescriptionAscending = string.Empty;

    private string _hosts;
    public string Hosts
    {
        get => _hosts;
        set
        {
            if (value == _hosts)
                return;

            _hosts = value;
            OnPropertyChanged();
        }
    }

    public ICollectionView HostsHistoryView { get; }

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

    private bool _cancelScan;
    public bool CancelScan
    {
        get => _cancelScan;
        set
        {
            if (value == _cancelScan)
                return;

            _cancelScan = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<PortScannerPortInfo> _results = new();
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
    public PortScannerViewModel(IDialogCoordinator instance, int tabId, string host, string port)
    {
        _dialogCoordinator = instance;

        _tabId = tabId;
        Hosts = host;
        Ports = port;

        // Set collection view
        HostsHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.PortScanner_HostsHistory);
        PortsHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.PortScanner_PortsHistory);

        // Result view
        ResultsView = CollectionViewSource.GetDefaultView(Results);
        ResultsView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(PortScannerPortInfo.IPAddress)));
        ResultsView.SortDescriptions.Add(new SortDescription(nameof(PortScannerPortInfo.IPAddressInt32), ListSortDirection.Descending));

        LoadSettings();
    }

    private void LoadSettings()
    {

    }

    public void OnLoaded()
    {
        if (!_firstLoad)
            return;

        if (!string.IsNullOrEmpty(Hosts) && !string.IsNullOrEmpty(Ports))
            StartScan().ConfigureAwait(false);

        _firstLoad = false;
    }

    public void OnClose()
    {
        // Stop scan
        if (IsRunning)
            StopScan();
    }
    #endregion

    #region ICommands & Actions
    public ICommand OpenPortProfileSelectionCommand => new RelayCommand(_ => OpenPortProfileSelectionAction(), OpenPortProfileSelection_CanExecute);

    private bool OpenPortProfileSelection_CanExecute(object parameter) => Application.Current.MainWindow != null && !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen;

    private void OpenPortProfileSelectionAction()
    {
        OpenPortProfileSelection().ConfigureAwait(false);
    }

    public ICommand ScanCommand => new RelayCommand(_ => ScanAction(), Scan_CanExecute);

    private bool Scan_CanExecute(object parameter) => Application.Current.MainWindow != null && !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen;

    private void ScanAction()
    {
        if (IsRunning)
            StopScan();
        else
            StartScan().ConfigureAwait(false);
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
        var customDialog = new CustomDialog
        {
            Title = Localization.Resources.Strings.SelectPortProfile
        };

        var viewModel = new PortProfilesViewModel(async instance =>
        {
            await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

            Ports = string.Join("; ", instance.GetSelectedPortProfiles().Select(x => x.Ports));
        }, async _ =>
        {
            await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
        });

        customDialog.Content = new PortProfilesDialog
        {
            DataContext = viewModel
        };

        await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
    }

    private async Task StartScan()
    {
        IsStatusMessageDisplayed = false;
        StatusMessage = string.Empty;

        IsRunning = true;
        PreparingScan = true;

        Results.Clear();

        // Change the tab title (not nice, but it works)
        var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

        if (window != null)
        {
            foreach (var tabablzControl in VisualTreeHelper.FindVisualChildren<TabablzControl>(window))
            {
                tabablzControl.Items.OfType<DragablzTabItem>().First(x => x.Id == _tabId).Header = Hosts;
            }
        }

        _cancellationTokenSource = new CancellationTokenSource();

        // Resolve hostnames
        List<string> ipRanges;

        try
        {
            ipRanges = await HostRangeHelper.ResolveHostnamesInIPRangesAsync(Hosts.Replace(" ", "").Split(';'), SettingsManager.Current.Network_ResolveHostnamePreferIPv4, _cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            UserHasCanceled(this, EventArgs.Empty);
            return;
        }
        catch (AggregateException exceptions) // DNS error (could not resolve hostname...)
        {
            DnsResolveFailed(exceptions);
            return;
        }

        // Create ip addresses 
        IPAddress[] ipAddresses;

        try
        {
            // Create a list of all ip addresses
            ipAddresses = await HostRangeHelper.CreateIPAddressesFromIPRangesAsync(ipRanges.ToArray(), _cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            UserHasCanceled(this, EventArgs.Empty);
            return;
        }

        var ports = await PortRangeHelper.ConvertPortRangeToIntArrayAsync(Ports);

        PortsToScan = ports.Length * ipAddresses.Length;
        PortsScanned = 0;

        PreparingScan = false;

        // Add host(s) to the history
        AddHostToHistory(Hosts);
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

        portScanner.ScanAsync(ipAddresses, ports, _cancellationTokenSource.Token);
    }

    private void StopScan()
    {
        CancelScan = true;
        _cancellationTokenSource.Cancel();
    }
    
    private async Task Export()
    {
        var customDialog = new CustomDialog
        {
            Title = Localization.Resources.Strings.Export
        };

        var exportViewModel = new ExportViewModel(async instance =>
        {
            await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

            try
            {
                ExportManager.Export(instance.FilePath, instance.FileType, instance.ExportAll ? Results : new ObservableCollection<PortScannerPortInfo>(SelectedResults.Cast<PortScannerPortInfo>().ToArray()));
            }
            catch (Exception ex)
            {
                var settings = AppearanceManager.MetroDialog;
                settings.AffirmativeButtonText = Localization.Resources.Strings.OK;

                await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.Error, Localization.Resources.Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine + Environment.NewLine + ex.Message, MessageDialogStyle.Affirmative, settings);
            }

            SettingsManager.Current.PortScanner_ExportFileType = instance.FileType;
            SettingsManager.Current.PortScanner_ExportFilePath = instance.FilePath;
        }, _ => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); }, new[] { ExportFileType.Csv, ExportFileType.Xml, ExportFileType.Json }, true, SettingsManager.Current.PortScanner_ExportFileType, SettingsManager.Current.PortScanner_ExportFilePath);

        customDialog.Content = new ExportDialog
        {
            DataContext = exportViewModel
        };

        await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
    }

    private void AddHostToHistory(string host)
    {
        // Create the new list
        var list = ListHelper.Modify(SettingsManager.Current.PortScanner_HostsHistory.ToList(), host, SettingsManager.Current.General_HistoryListEntries);

        // Clear the old items
        SettingsManager.Current.PortScanner_HostsHistory.Clear();
        OnPropertyChanged(nameof(Hosts)); // Raise property changed again, after the collection has been cleared

        // Fill with the new items
        list.ForEach(x => SettingsManager.Current.PortScanner_HostsHistory.Add(x));
    }

    private void AddPortToHistory(string port)
    {
        // Create the new list
        var list = ListHelper.Modify(SettingsManager.Current.PortScanner_PortsHistory.ToList(), port, SettingsManager.Current.General_HistoryListEntries);

        // Clear the old items
        SettingsManager.Current.PortScanner_PortsHistory.Clear();
        OnPropertyChanged(nameof(Ports)); // Raise property changed again, after the collection has been cleared

        // Fill with the new items
        list.ForEach(x => SettingsManager.Current.PortScanner_PortsHistory.Add(x));
    }

    public void SortResultByPropertyName(string sortDescription)
    {
        ResultsView.SortDescriptions.Clear();
        ResultsView.SortDescriptions.Add(new SortDescription(nameof(PortScannerPortInfo.IPAddressInt32), ListSortDirection.Descending));

        if (_lastSortDescriptionAscending.Equals(sortDescription))
        {
            ResultsView.SortDescriptions.Add(new SortDescription(sortDescription, ListSortDirection.Descending));
            _lastSortDescriptionAscending = string.Empty;
        }
        else
        {
            ResultsView.SortDescriptions.Add(new SortDescription(sortDescription, ListSortDirection.Ascending));
            _lastSortDescriptionAscending = sortDescription;
        }
    }
    #endregion

    #region Events
    private void UserHasCanceled(object sender, EventArgs e)
    {
        StatusMessage = Localization.Resources.Strings.CanceledByUserMessage;
        IsStatusMessageDisplayed = true;

        CancelScan = false;
        IsRunning = false;
    }

    private void ProgressChanged(object sender, ProgressChangedArgs e)
    {
        PortsScanned = e.Value;
    }

    private void DnsResolveFailed(AggregateException e)
    {
        StatusMessage = $"{Localization.Resources.Strings.TheFollowingHostnamesCouldNotBeResolved} {string.Join(", ", e.Flatten().InnerExceptions.Select(x => x.Message))}";
        IsStatusMessageDisplayed = true;

        CancelScan = false;
        IsRunning = false;
    }

    private void ScanComplete(object sender, EventArgs e)
    {
        if (Results.Count == 0)
        {
            StatusMessage = Localization.Resources.Strings.NoOpenPortsFound;
            IsStatusMessageDisplayed = true;
        }
        
        CancelScan = false;
        IsRunning = false;
    }

    private void PortScanned(object sender, PortScannerPortScannedArgs e)
    {
        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
        {
            Results.Add(e.Args);
        }));
    }
  
    #endregion
}