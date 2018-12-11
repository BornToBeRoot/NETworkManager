using System.Windows.Input;
using System.Windows;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using NETworkManager.Models.Settings;
using System.Collections.Generic;
using NETworkManager.Models.Network;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Windows.Threading;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Data;
using System.Linq;
using NETworkManager.Utilities;
using Dragablz;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Controls;
using NETworkManager.Models.Export;
using NETworkManager.Views;

namespace NETworkManager.ViewModels
{
    public class PortScannerViewModel : ViewModelBase
    {
        #region Variables
        private readonly IDialogCoordinator _dialogCoordinator;

        private CancellationTokenSource _cancellationTokenSource;

        private readonly int _tabId;
        private bool _firstLoad = true;

        private readonly DispatcherTimer _dispatcherTimer = new DispatcherTimer();
        private readonly Stopwatch _stopwatch = new Stopwatch();

        private bool _isLoading;

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

        private string _port;
        public string Port
        {
            get => _port;
            set
            {
                if (value == _port)
                    return;

                _port = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView PortHistoryView { get; }

        private bool _isScanRunning;
        public bool IsScanRunning
        {
            get => _isScanRunning;
            set
            {
                if (value == _isScanRunning)
                    return;

                _isScanRunning = value;

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

        private ObservableCollection<PortInfo> _portScanResults = new ObservableCollection<PortInfo>();
        public ObservableCollection<PortInfo> PortScanResult
        {
            get => _portScanResults;
            set
            {
                if (_portScanResults != null && value == _portScanResults)
                    return;

                _portScanResults = value;
            }
        }

        public ICollectionView PortScanResultsView { get; }

        private PortInfo _selectedPortScanResult;
        public PortInfo SelectedPortScanResult
        {
            get => _selectedPortScanResult;
            set
            {
                if (value == _selectedPortScanResult)
                    return;

                _selectedPortScanResult = value;
                OnPropertyChanged();
            }
        }

        private IList _selectedPortScanResults = new ArrayList();
        public IList SelectedPortScanResults
        {
            get => _selectedPortScanResults;
            set
            {
                if (Equals(value, _selectedPortScanResults))
                    return;

                _selectedPortScanResults = value;
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

        private int _portsOpen;
        public int PortsOpen
        {
            get => _portsOpen;
            set
            {
                if (value == _portsOpen)
                    return;

                _portsOpen = value;
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

        private DateTime? _startTime;
        public DateTime? StartTime
        {
            get => _startTime;
            set
            {
                if (value == _startTime)
                    return;

                _startTime = value;
                OnPropertyChanged();
            }
        }

        private TimeSpan _duration;
        public TimeSpan Duration
        {
            get => _duration;
            set
            {
                if (value == _duration)
                    return;

                _duration = value;
                OnPropertyChanged();
            }
        }

        private DateTime? _endTime;
        public DateTime? EndTime
        {
            get => _endTime;
            set
            {
                if (value == _endTime)
                    return;

                _endTime = value;
                OnPropertyChanged();
            }
        }

        private bool _expandStatistics;
        public bool ExpandStatistics
        {
            get => _expandStatistics;
            set
            {
                if (value == _expandStatistics)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PortScanner_ExpandStatistics = value;

                _expandStatistics = value;
                OnPropertyChanged();
            }
        }

        private bool _displayStatusMessage;
        public bool DisplayStatusMessage
        {
            get => _displayStatusMessage;
            set
            {
                if (value == _displayStatusMessage)
                    return;

                _displayStatusMessage = value;
                OnPropertyChanged();
            }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (value == _statusMessage)
                    return;

                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public bool ShowStatistics => SettingsManager.Current.PortScanner_ShowStatistics;

        #endregion

        #region Constructor, load settings, shutdown
        public PortScannerViewModel(IDialogCoordinator instance, int tabId, string host, string port)
        {
            _isLoading = true;

            _dialogCoordinator = instance;

            _tabId = tabId;
            Host = host;
            Port = port;

            // Set collection view
            HostHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.PortScanner_HostHistory);
            PortHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.PortScanner_PortHistory);

            // Result view
            PortScanResultsView = CollectionViewSource.GetDefaultView(PortScanResult);
            PortScanResultsView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(PortInfo.IPAddress)));

            LoadSettings();

            // Detect if settings have changed...
            SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;

            _isLoading = false;
        }

        private void LoadSettings()
        {
            ExpandStatistics = SettingsManager.Current.PortScanner_ExpandStatistics;
        }

        public void OnLoaded()
        {
            if (!_firstLoad)
                return;

            if (!string.IsNullOrEmpty(Host) && !string.IsNullOrEmpty(Port))
                StartScan();

            _firstLoad = false;
        }

        public void OnClose()
        {
            // Stop scan
            if (IsScanRunning)
                StopScan();
        }
        #endregion

        #region ICommands & Actions
        public ICommand ScanCommand
        {
            get { return new RelayCommand(p => ScanAction()); }
        }

        private void ScanAction()
        {
            if (IsScanRunning)
                StopScan();
            else
                StartScan();
        }

        public ICommand CopySelectedIPAddressCommand
        {
            get { return new RelayCommand(p => CopySelectedIPAddressAction()); }
        }

        private void CopySelectedIPAddressAction()
        {
            CommonMethods.SetClipboard(SelectedPortScanResult.IPAddress.ToString());
        }

        public ICommand CopySelectedHostnameCommand
        {
            get { return new RelayCommand(p => CopySelectedHostnameAction()); }
        }

        private void CopySelectedHostnameAction()
        {
            CommonMethods.SetClipboard(SelectedPortScanResult.Hostname);
        }

        public ICommand CopySelectedPortCommand
        {
            get { return new RelayCommand(p => CopySelectedPortAction()); }
        }

        private void CopySelectedPortAction()
        {
            CommonMethods.SetClipboard(SelectedPortScanResult.Port.ToString());
        }

        public ICommand CopySelectedStatusCommand
        {
            get { return new RelayCommand(p => CopySelectedStatusAction()); }
        }

        private void CopySelectedStatusAction()
        {
            CommonMethods.SetClipboard(Resources.Localization.Strings.ResourceManager.GetString(SelectedPortScanResult.Status.ToString(), LocalizationManager.Culture));
        }

        public ICommand CopySelectedProtocolCommand
        {
            get { return new RelayCommand(p => CopySelectedProtocolAction()); }
        }

        private void CopySelectedProtocolAction()
        {
            CommonMethods.SetClipboard(SelectedPortScanResult.LookupInfo.Protocol.ToString());
        }

        public ICommand CopySelectedServiceCommand
        {
            get { return new RelayCommand(p => CopySelectedServiceAction()); }
        }

        private void CopySelectedServiceAction()
        {
            CommonMethods.SetClipboard(SelectedPortScanResult.LookupInfo.Service);
        }

        public ICommand CopySelectedDescriptionCommand
        {
            get { return new RelayCommand(p => CopySelectedDescriptionAction()); }
        }

        private void CopySelectedDescriptionAction()
        {
            CommonMethods.SetClipboard(SelectedPortScanResult.LookupInfo.Description);
        }

        public ICommand ExportCommand
        {
            get { return new RelayCommand(p => ExportAction()); }
        }

        private async void ExportAction()
        {
            var customDialog = new CustomDialog
            {
                Title = Resources.Localization.Strings.Export
            };

            var exportViewModel = new ExportViewModel(async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                try
                {
                    ExportManager.Export(instance.FilePath, instance.FileType,
                        instance.ExportAll
                            ? PortScanResult
                            : new ObservableCollection<PortInfo>(SelectedPortScanResults.Cast<PortInfo>().ToArray()));
                }
                catch (Exception ex)
                {
                    var settings = AppearanceManager.MetroDialog;
                    settings.AffirmativeButtonText = Resources.Localization.Strings.OK;

                    await _dialogCoordinator.ShowMessageAsync(this, Resources.Localization.Strings.Error, Resources.Localization.Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine + Environment.NewLine + ex.Message, MessageDialogStyle.Affirmative, settings);
                }

                SettingsManager.Current.PortScanner_ExportFileType = instance.FileType;
                SettingsManager.Current.PortScanner_ExportFilePath = instance.FilePath;
            }, instance => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); }, SettingsManager.Current.PortScanner_ExportFileType, SettingsManager.Current.PortScanner_ExportFilePath);

            customDialog.Content = new ExportDialog
            {
                DataContext = exportViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }
        #endregion

        #region Methods
        private async void StartScan()
        {
            _isLoading = true;

            DisplayStatusMessage = false;
            StatusMessage = string.Empty;

            IsScanRunning = true;
            PreparingScan = true;

            // Measure the time
            StartTime = DateTime.Now;
            _stopwatch.Start();
            _dispatcherTimer.Tick += DispatcherTimer_Tick;
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            _dispatcherTimer.Start();
            EndTime = null;

            PortScanResult.Clear();
            PortsOpen = 0;

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

            var hosts = Host.Split(';');

            var hostData = new List<Tuple<IPAddress, string>>();

            var hostname = string.Empty;

            foreach (var host in hosts)
            {
                var host1 = host.Trim();

                IPAddress.TryParse(host1, out var ipAddress);

                try
                {
                    // Resolve DNS
                    // Try to resolve the hostname
                    if (ipAddress == null)
                    {
                        var ipHostEntry = await Dns.GetHostEntryAsync(host1);

                        foreach (var ip in ipHostEntry.AddressList)
                        {
                            switch (ip.AddressFamily)
                            {
                                case AddressFamily.InterNetwork when SettingsManager.Current.PortScanner_ResolveHostnamePreferIPv4:
                                    ipAddress = ip;
                                    break;
                                case AddressFamily.InterNetworkV6 when !SettingsManager.Current.PortScanner_ResolveHostnamePreferIPv4:
                                    ipAddress = ip;
                                    break;
                            }
                        }

                        // Fallback --> If we could not resolve our prefered ip protocol
                        if (ipAddress == null)
                        {
                            foreach (var ip in ipHostEntry.AddressList)
                            {
                                ipAddress = ip;
                                break;
                            }
                        }

                        hostname = host1;
                    }
                    else
                    {
                        try
                        {
                            var ipHostEntry = await Dns.GetHostEntryAsync(ipAddress);

                            hostname = ipHostEntry.HostName;
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }
                catch (SocketException) // This will catch DNS resolve errors
                {
                    if (!string.IsNullOrEmpty(StatusMessage))
                        StatusMessage += Environment.NewLine;

                    StatusMessage += string.Format(Resources.Localization.Strings.CouldNotResolveHostnameFor, host1);
                    DisplayStatusMessage = true;

                    continue;
                }

                hostData.Add(Tuple.Create(ipAddress, hostname));
            }

            if (hostData.Count == 0)
            {
                StatusMessage += Environment.NewLine + Resources.Localization.Strings.NothingToDoCheckYourInput;
                DisplayStatusMessage = true;

                ScanFinished();

                return;
            }

            var ports = await PortRangeHelper.ConvertPortRangeToIntArrayAsync(Port);

            try
            {
                PortsToScan = ports.Length * hostData.Count;
                PortsScanned = 0;

                PreparingScan = false;

                AddHostToHistory(Host);
                AddPortToHistory(Port);

                var portScannerOptions = new PortScannerOptions
                {
                    Threads = SettingsManager.Current.PortScanner_Threads,
                    ShowClosed = SettingsManager.Current.PortScanner_ShowClosed,
                    Timeout = SettingsManager.Current.PortScanner_Timeout
                };

                PortScanner portScanner = new PortScanner();
                portScanner.PortScanned += PortScanner_PortScanned;
                portScanner.ScanComplete += PortScanner_ScanComplete;
                portScanner.ProgressChanged += PortScanner_ProgressChanged;
                portScanner.UserHasCanceled += PortScanner_UserHasCanceled;

                portScanner.ScanAsync(hostData, ports, portScannerOptions, _cancellationTokenSource.Token);
            }

            catch (Exception ex) // This will catch any exception
            {
                StatusMessage = ex.Message;
                DisplayStatusMessage = true;

                ScanFinished();
            }
        }

        private void StopScan()
        {
            CancelScan = true;
            _cancellationTokenSource.Cancel();
        }

        private void ScanFinished()
        {
            // Stop timer and stopwatch
            _stopwatch.Stop();
            _dispatcherTimer.Stop();

            Duration = _stopwatch.Elapsed;
            EndTime = DateTime.Now;

            _stopwatch.Reset();

            CancelScan = false;
            IsScanRunning = false;
        }

        private void AddHostToHistory(string host)
        {
            // Create the new list
            var list = ListHelper.Modify(SettingsManager.Current.PortScanner_HostHistory.ToList(), host, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.PortScanner_HostHistory.Clear();
            OnPropertyChanged(nameof(Host)); // Raise property changed again, after the collection has been cleared

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.PortScanner_HostHistory.Add(x));
        }

        private void AddPortToHistory(string port)
        {
            // Create the new list
            var list = ListHelper.Modify(SettingsManager.Current.PortScanner_PortHistory.ToList(), port, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.PortScanner_PortHistory.Clear();
            OnPropertyChanged(nameof(Port)); // Raise property changed again, after the collection has been cleared

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.PortScanner_PortHistory.Add(x));
        }
        #endregion

        #region Events
        private void PortScanner_UserHasCanceled(object sender, EventArgs e)
        {
            StatusMessage = Resources.Localization.Strings.CanceledByUserMessage;
            DisplayStatusMessage = true;

            ScanFinished();
        }

        private void PortScanner_ProgressChanged(object sender, ProgressChangedArgs e)
        {
            PortsScanned = e.Value;
        }

        private void PortScanner_ScanComplete(object sender, EventArgs e)
        {
            ScanFinished();
        }

        private void PortScanner_PortScanned(object sender, PortScannedArgs e)
        {
            var portInfo = PortInfo.Parse(e);

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate ()
            {
                lock (PortScanResult)
                    PortScanResult.Add(portInfo);
            }));

            if (portInfo.Status == PortInfo.PortStatus.Open)
                PortsOpen++;
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            Duration = _stopwatch.Elapsed;
        }

        private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SettingsInfo.PortScanner_ShowStatistics))
                OnPropertyChanged(nameof(ShowStatistics));
        }
        #endregion
    }
}