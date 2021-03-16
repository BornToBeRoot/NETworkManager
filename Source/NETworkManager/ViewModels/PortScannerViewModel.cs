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
using NETworkManager.Localization.Translators;
using System.Threading.Tasks;

namespace NETworkManager.ViewModels
{
    public class PortScannerViewModel : ViewModelBase
    {
        #region Variables
        private readonly IDialogCoordinator _dialogCoordinator;

        private CancellationTokenSource _cancellationTokenSource;

        public readonly int TabId;
        private bool _firstLoad = true;

        private string _lastSortDescriptionAscending = string.Empty;

        private bool _isLoading;

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

        public ICollectionView ResultsView { get; }

        private PortInfo _selectedResult;
        public PortInfo SelectedResult
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

        public bool ResolveHostname => SettingsManager.Current.PortScanner_ResolveHostname;

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
            set
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
            _isLoading = true;

            _dialogCoordinator = instance;

            TabId = tabId;
            Hosts = host;
            Ports = port;

            // Set collection view
            HostsHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.PortScanner_HostsHistory);
            PortsHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.PortScanner_PortsHistory);

            // Add default port profiles...
            if (SettingsManager.Current.PortScanner_PortProfiles.Count == 0)
                SettingsManager.Current.PortScanner_PortProfiles = new ObservableCollection<PortProfileInfo>(PortProfile.DefaultList());

            // Result view
            ResultsView = CollectionViewSource.GetDefaultView(PortScanResult);
            ResultsView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(PortInfo.IPAddress)));
            ResultsView.SortDescriptions.Add(new SortDescription(nameof(PortInfo.IPAddressInt32), ListSortDirection.Descending));

            LoadSettings();

            // Detect if settings have changed...
            SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;

            _isLoading = false;
        }

        private void LoadSettings()
        {

        }

        public void OnLoaded()
        {
            if (!_firstLoad)
                return;

            if (!string.IsNullOrEmpty(Hosts) && !string.IsNullOrEmpty(Ports))
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
        public ICommand OpenPortProfileSelectionCommand => new RelayCommand(p => OpenPortProfileSelectionAction(), OpenPortProfileSelection_CanExecute);

        private bool OpenPortProfileSelection_CanExecute(object parameter) => Application.Current.MainWindow != null && !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen;

        private void OpenPortProfileSelectionAction()
        {
            OpenPortProfileSelection();
        }

        public ICommand ScanCommand => new RelayCommand(p => ScanAction(), Scan_CanExecute);

        private bool Scan_CanExecute(object paramter) => Application.Current.MainWindow != null && !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen;

        private void ScanAction()
        {
            if (IsScanRunning)
                StopScan();
            else
                StartScan();
        }

        public ICommand CopySelectedIPAddressCommand => new RelayCommand(p => CopySelectedIPAddressAction());

        private void CopySelectedIPAddressAction()
        {
            ClipboardHelper.SetClipboard(SelectedResult.IPAddress.ToString());
        }

        public ICommand CopySelectedHostnameCommand => new RelayCommand(p => CopySelectedHostnameAction());

        private void CopySelectedHostnameAction()
        {
            ClipboardHelper.SetClipboard(SelectedResult.Hostname);
        }

        public ICommand CopySelectedPortCommand => new RelayCommand(p => CopySelectedPortAction());

        private void CopySelectedPortAction()
        {
            ClipboardHelper.SetClipboard(SelectedResult.Port.ToString());
        }

        public ICommand CopySelectedStatusCommand => new RelayCommand(p => CopySelectedStatusAction());

        private void CopySelectedStatusAction()
        {
            ClipboardHelper.SetClipboard(PortStateTranslator.GetInstance().Translate(SelectedResult.State));
        }

        public ICommand CopySelectedProtocolCommand => new RelayCommand(p => CopySelectedProtocolAction());

        private void CopySelectedProtocolAction()
        {
            ClipboardHelper.SetClipboard(SelectedResult.LookupInfo.Protocol.ToString());
        }

        public ICommand CopySelectedServiceCommand => new RelayCommand(p => CopySelectedServiceAction());

        private void CopySelectedServiceAction()
        {
            ClipboardHelper.SetClipboard(SelectedResult.LookupInfo.Service);
        }

        public ICommand CopySelectedDescriptionCommand => new RelayCommand(p => CopySelectedDescriptionAction());

        private void CopySelectedDescriptionAction()
        {
            ClipboardHelper.SetClipboard(SelectedResult.LookupInfo.Description);
        }

        public ICommand ExportCommand => new RelayCommand(p => ExportAction());

        private void ExportAction()
        {
            Export();
        }
        #endregion

        #region Methods
        private async Task OpenPortProfileSelection()
        {
            var customDialog = new CustomDialog
            {
                Title = Localization.Resources.Strings.SelectPortProfile
            };

            var viewModel = new PortProfilesViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                Ports = instance.SelectedPortProfile.Ports;
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            });

            customDialog.Content = new PortProfilesDialog
            {
                DataContext = viewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        private async Task StartScan()
        {
            _isLoading = true;

            IsStatusMessageDisplayed = false;
            StatusMessage = string.Empty;

            IsScanRunning = true;
            PreparingScan = true;

            PortScanResult.Clear();

            // Change the tab title (not nice, but it works)
            var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

            if (window != null)
            {
                foreach (var tabablzControl in VisualTreeHelper.FindVisualChildren<TabablzControl>(window))
                {
                    tabablzControl.Items.OfType<DragablzTabItem>().First(x => x.Id == TabId).Header = Hosts;
                }
            }

            _cancellationTokenSource = new CancellationTokenSource();

            // Resolve hostnames
            List<string> ipRanges;

            try
            {
                ipRanges = await HostRangeHelper.ResolveHostnamesInIPRangesAsync(Hosts.Replace(" ", "").Split(';'), _cancellationTokenSource.Token);
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

            var portScanner = new PortScanner
            {
                ResolveHostname = SettingsManager.Current.PortScanner_ResolveHostname,
                HostThreads = SettingsManager.Current.PortScanner_HostThreads,
                PortThreads = SettingsManager.Current.PortScanner_PortThreads,
                ShowClosed = SettingsManager.Current.PortScanner_ShowClosed,
                Timeout = SettingsManager.Current.PortScanner_Timeout
            };

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

        private void ScanFinished()
        {
            CancelScan = false;
            IsScanRunning = false;
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
                    ExportManager.Export(instance.FilePath, instance.FileType, instance.ExportAll ? PortScanResult : new ObservableCollection<PortInfo>(SelectedResults.Cast<PortInfo>().ToArray()));
                }
                catch (Exception ex)
                {
                    var settings = AppearanceManager.MetroDialog;
                    settings.AffirmativeButtonText = Localization.Resources.Strings.OK;

                    await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.Error, Localization.Resources.Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine + Environment.NewLine + ex.Message, MessageDialogStyle.Affirmative, settings);
                }

                SettingsManager.Current.PortScanner_ExportFileType = instance.FileType;
                SettingsManager.Current.PortScanner_ExportFilePath = instance.FilePath;
            }, instance => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); }, new ExportManager.ExportFileType[] { ExportManager.ExportFileType.CSV, ExportManager.ExportFileType.XML, ExportManager.ExportFileType.JSON }, true, SettingsManager.Current.PortScanner_ExportFileType, SettingsManager.Current.PortScanner_ExportFilePath);

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
            ResultsView.SortDescriptions.Add(new SortDescription(nameof(PortInfo.IPAddressInt32), ListSortDirection.Descending));

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

            ScanFinished();
        }

        private void ProgressChanged(object sender, ProgressChangedArgs e)
        {
            PortsScanned = e.Value;
        }

        private void DnsResolveFailed(AggregateException e)
        {
            StatusMessage = $"{Localization.Resources.Strings.TheFollowingHostnamesCouldNotBeResolved} {string.Join(", ", e.Flatten().InnerExceptions.Select(x => x.Message))}";
            IsStatusMessageDisplayed = true;

            ScanFinished();
        }

        private void ScanComplete(object sender, EventArgs e)
        {
            ScanFinished();
        }

        private void PortScanned(object sender, PortScannedArgs e)
        {
            var portInfo = PortInfo.Parse(e);

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate ()
            {
                //lock (PortScanResult)
                    PortScanResult.Add(portInfo);
            }));
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {

        }

        private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SettingsInfo.PortScanner_ResolveHostname):
                    OnPropertyChanged(nameof(ResolveHostname));
                    break;
            }
        }
        #endregion
    }
}