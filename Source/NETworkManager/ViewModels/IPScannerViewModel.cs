using NETworkManager.Settings;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows.Input;
using System.Windows;
using System;
using System.Collections;
using System.Threading;
using System.Collections.Generic;
using NETworkManager.Models.Network;
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
using System.Text.RegularExpressions;
using NETworkManager.Profiles;
using NETworkManager.Localization;
using NETworkManager.Models;
using NETworkManager.Models.EventSystem;
using System.Threading.Tasks;
using System.Text;

namespace NETworkManager.ViewModels;

public class IPScannerViewModel : ViewModelBase, IProfileManagerMinimal
{
    #region Variables
    private readonly IDialogCoordinator _dialogCoordinator;

    private CancellationTokenSource _cancellationTokenSource;

    private readonly Guid _tabId;
    private bool _firstLoad = true;

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

    private bool _isSubnetDetectionRunning;
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

    private ObservableCollection<IPScannerHostInfo> _results = [];
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

    public ICollectionView ResultsView { get; }

    private IPScannerHostInfo _selectedResult;
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

    public static IEnumerable<CustomCommandInfo> CustomCommands => SettingsManager.Current.IPScanner_CustomCommands;
    #endregion

    #region Constructor, load settings, shutdown
    public IPScannerViewModel(IDialogCoordinator instance, Guid tabId, string hostOrIPRange)
    {
        _dialogCoordinator = instance;

        _tabId = tabId;
        Host = hostOrIPRange;

        // Host history
        HostHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.IPScanner_HostHistory);

        // Result view
        IPAddressComparer comparer = new();
        Results = new ObservableCollection<IPScannerHostInfo>(Results.OrderBy(x => x.PingInfo.IPAddress, comparer));
        ResultsView = CollectionViewSource.GetDefaultView(Results);
    }

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

    public ICommand ScanCommand => new RelayCommand(_ => ScanAction(), Scan_CanExecute);

    private bool Scan_CanExecute(object parameter)
    {
        return Application.Current.MainWindow != null && !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen;
    }

    private void ScanAction()
    {
        if (IsRunning)
            Stop();
        else
            Start().ConfigureAwait(false);
    }

    public ICommand DetectSubnetCommand => new RelayCommand(_ => DetectSubnetAction());

    private void DetectSubnetAction()
    {
        DetectIPRange().ConfigureAwait(false);
    }

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

    public ICommand PerformDNSLookupIPAddressCommand => new RelayCommand(_ => PerformDNSLookupIPAddressAction());

    private void PerformDNSLookupIPAddressAction()
    {
        EventSystem.RedirectToApplication(ApplicationName.DNSLookup, SelectedResult.PingInfo.IPAddress.ToString());
    }

    public ICommand PerformDNSLookupHostnameCommand => new RelayCommand(_ => PerformDNSLookupHostnameAction());

    private void PerformDNSLookupHostnameAction()
    {
        EventSystem.RedirectToApplication(ApplicationName.DNSLookup, SelectedResult.Hostname);
    }

    public ICommand CustomCommandCommand => new RelayCommand(CustomCommandAction);

    private void CustomCommandAction(object guid)
    {
        CustomCommand(guid).ConfigureAwait(false);
    }

    public ICommand AddProfileSelectedHostCommand => new RelayCommand(_ => AddProfileSelectedHostAction());

    private async void AddProfileSelectedHostAction()
    {
        ProfileInfo profileInfo = new()
        {
            Name = string.IsNullOrEmpty(SelectedResult.Hostname) ? SelectedResult.PingInfo.IPAddress.ToString() : SelectedResult.Hostname.TrimEnd('.'),
            Host = SelectedResult.PingInfo.IPAddress.ToString(),

            // Additional data
            WakeOnLAN_MACAddress = SelectedResult.MACAddressString
        };

        await ProfileDialogManager.ShowAddProfileDialog(this, _dialogCoordinator, profileInfo);
    }

    public ICommand CopySelectedPortsCommand => new RelayCommand(_ => CopySelectedPortsAction());

    private void CopySelectedPortsAction()
    {
        StringBuilder stringBuilder = new();

        foreach (var port in SelectedResult.Ports)
        {
            stringBuilder.AppendLine($"{port.Port}/{port.LookupInfo.Protocol},{ResourceTranslator.Translate(ResourceIdentifier.PortState, port.State)},{port.LookupInfo.Service},{port.LookupInfo.Description}");
        }

        ClipboardHelper.SetClipboard(stringBuilder.ToString());
    }

    public ICommand ExportCommand => new RelayCommand(_ => ExportAction());

    private void ExportAction()
    {
        Export().ConfigureAwait(false);
    }
    #endregion

    #region Methods
    private async Task Start()
    {
        IsStatusMessageDisplayed = false;
        IsRunning = true;
        PreparingScan = true;

        Results.Clear();

        // Change the tab title (not nice, but it works)
        var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

        if (window != null)
        {
            foreach (var tabablzControl in VisualTreeHelper.FindVisualChildren<TabablzControl>(window))
            {
                tabablzControl.Items.OfType<DragablzTabItem>().First(x => x.Id == _tabId).Header = Host;
            }
        }

        _cancellationTokenSource = new CancellationTokenSource();

        // Resolve hostnames
        (List<(IPAddress ipAddress, string hostname)> hosts, List<string> hostnamesNotResolved) hosts;

        try
        {
            hosts = await HostRangeHelper.ResolveAsync(HostRangeHelper.CreateListFromInput(Host), SettingsManager.Current.Network_ResolveHostnamePreferIPv4, _cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            UserHasCanceled(this, EventArgs.Empty);
            return;
        }

        // Show error message if (some) hostnames could not be resolved
        if (hosts.hostnamesNotResolved.Count > 0)
        {
            StatusMessage = $"{Localization.Resources.Strings.TheFollowingHostnamesCouldNotBeResolved} {string.Join(", ", hosts.hostnamesNotResolved)}";
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
            SettingsManager.Current.IPScanner_PortScanEnabled,
            await PortRangeHelper.ConvertPortRangeToIntArrayAsync(SettingsManager.Current.IPScanner_PortScanPorts),
            SettingsManager.Current.IPScanner_PortScanTimeout,
            SettingsManager.Current.IPScanner_ResolveHostname,
            SettingsManager.Current.IPScanner_DNSShowErrorMessage,
            SettingsManager.Current.IPScanner_ResolveMACAddress,
            SettingsManager.Current.IPScanner_ShowAllResults
        ));

        ipScanner.HostScanned += HostScanned;
        ipScanner.ScanComplete += ScanComplete;
        ipScanner.ProgressChanged += ProgressChanged;
        ipScanner.UserHasCanceled += UserHasCanceled;

        ipScanner.ScanAsync(hosts.hosts, _cancellationTokenSource.Token);
    }

    private void Stop()
    {
        IsCanceling = true;
        _cancellationTokenSource.Cancel();
    }

    private async Task DetectIPRange()
    {
        IsSubnetDetectionRunning = true;

        var localIP = await NetworkInterface.DetectLocalIPAddressBasedOnRoutingAsync(IPAddress.Parse("1.1.1.1"));

        // Could not detect local ip address
        if (localIP != null)
        {
            var subnetmaskDetected = false;

            // Get subnetmask, based on ip address
            foreach (var networkInterface in (await NetworkInterface.GetNetworkInterfacesAsync()).Where(networkInterface => networkInterface.IPv4Address.Any(x => x.Item1.Equals(localIP))))
            {
                subnetmaskDetected = true;

                Host = $"{localIP}/{Subnetmask.ConvertSubnetmaskToCidr(networkInterface.IPv4Address.First().Item2)}";

                // Fix: If the user clears the TextBox and then clicks again on the button, the TextBox remains empty...
                OnPropertyChanged(nameof(Host));

                break;
            }

            if (!subnetmaskDetected)
                await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.Error, Localization.Resources.Strings.CouldNotDetectSubnetmask, MessageDialogStyle.Affirmative, AppearanceManager.MetroDialog);
        }
        else
        {
            await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.Error, Localization.Resources.Strings.CouldNotDetectLocalIPAddressMessage, MessageDialogStyle.Affirmative, AppearanceManager.MetroDialog);
        }

        IsSubnetDetectionRunning = false;
    }

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
                info.Arguments = Regex.Replace(info.Arguments, "\\$\\$hostname\\$\\$", hostname, RegexOptions.IgnoreCase);
                info.Arguments = Regex.Replace(info.Arguments, "\\$\\$ipaddress\\$\\$", ipAddress, RegexOptions.IgnoreCase);
            }

            try
            {
                Utilities.CustomCommand.Run(info);
            }
            catch (Exception ex)
            {
                await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.ResourceManager.GetString("Error", LocalizationManager.GetInstance().Culture), ex.Message, MessageDialogStyle.Affirmative, AppearanceManager.MetroDialog);
            }
        }
    }

    private void AddHostToHistory(string ipRange)
    {
        // Create the new list
        var list = ListHelper.Modify(SettingsManager.Current.IPScanner_HostHistory.ToList(), ipRange, SettingsManager.Current.General_HistoryListEntries);

        // Clear the old items
        SettingsManager.Current.IPScanner_HostHistory.Clear();
        OnPropertyChanged(nameof(Host)); // Raise property changed again, after the collection has been cleared

        // Fill with the new items
        list.ForEach(x => SettingsManager.Current.IPScanner_HostHistory.Add(x));
    }

    private Task Export()
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
                ExportManager.Export(instance.FilePath, instance.FileType, instance.ExportAll ? Results : new ObservableCollection<IPScannerHostInfo>(SelectedResults.Cast<IPScannerHostInfo>().ToArray()));
            }
            catch (Exception ex)
            {
                var settings = AppearanceManager.MetroDialog;
                settings.AffirmativeButtonText = Localization.Resources.Strings.OK;

                await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.Error, Localization.Resources.Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine + Environment.NewLine + ex.Message, MessageDialogStyle.Affirmative, settings);
            }

            SettingsManager.Current.IPScanner_ExportFileType = instance.FileType;
            SettingsManager.Current.IPScanner_ExportFilePath = instance.FilePath;
        }, _ =>
        {
            _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
        }, new[]
        {
            ExportFileType.Csv, ExportFileType.Xml, ExportFileType.Json
        }, true, SettingsManager.Current.IPScanner_ExportFileType, SettingsManager.Current.IPScanner_ExportFilePath);

        customDialog.Content = new ExportDialog
        {
            DataContext = exportViewModel
        };

        return _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
    }

    public void OnClose()
    {
        // Stop scan
        if (IsRunning)
            Stop();
    }

    #endregion

    #region Events
    private void HostScanned(object sender, IPScannerHostScannedArgs e)
    {
        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
        {
            Results.Add(e.Args);
        }));
    }

    private void ProgressChanged(object sender, ProgressChangedArgs e)
    {
        HostsScanned = e.Value;
    }

    private void ScanComplete(object sender, EventArgs e)
    {
        if (Results.Count == 0)
        {
            StatusMessage = Localization.Resources.Strings.NoReachableHostsFound;
            IsStatusMessageDisplayed = true;
        }

        IsCanceling = false;
        IsRunning = false;
    }

    private void UserHasCanceled(object sender, EventArgs e)
    {
        StatusMessage = Localization.Resources.Strings.CanceledByUserMessage;
        IsStatusMessageDisplayed = true;

        IsCanceling = false;
        IsRunning = false;
    }
    #endregion
}
