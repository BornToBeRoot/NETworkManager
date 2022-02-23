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
using NETworkManager.Localization.Translators;
using NETworkManager.Models;
using NETworkManager.Models.EventSystem;
using System.Threading.Tasks;

namespace NETworkManager.ViewModels
{
    public class IPScannerViewModel : ViewModelBase
    {
        #region Variables
        private readonly IDialogCoordinator _dialogCoordinator;

        private CancellationTokenSource _cancellationTokenSource;

        public readonly int TabId;
        private bool _firstLoad = true;

        private readonly bool _isLoading;

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

        private ObservableCollection<HostInfo> _results = new ObservableCollection<HostInfo>();
        public ObservableCollection<HostInfo> Results
        {
            get => _results;
            set
            {
                if (value != null && value == _results)
                    return;

                _results = value;
            }
        }

        public ICollectionView ResultsView { get; }

        private HostInfo _selectedResult;
        public HostInfo SelectedResult
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

        public bool ResolveHostname => SettingsManager.Current.IPScanner_ResolveHostname;

        public bool ResolveMACAddress => SettingsManager.Current.IPScanner_ResolveMACAddress;

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
            set
            {
                if (value == _statusMessage)
                    return;

                _statusMessage = value;
                OnPropertyChanged();
            }
        }               

        public IEnumerable<CustomCommandInfo> CustomCommands => SettingsManager.Current.IPScanner_CustomCommands;   
        #endregion

        #region Constructor, load settings, shutdown
        public IPScannerViewModel(IDialogCoordinator instance, int tabId, string hostOrIPRange)
        {
            _isLoading = true;

            _dialogCoordinator = instance;

            TabId = tabId;
            Hosts = hostOrIPRange;

            // Host history
            HostsHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.IPScanner_HostsHistory);

            // Result view
            ResultsView = CollectionViewSource.GetDefaultView(Results);
            ResultsView.SortDescriptions.Add(new SortDescription(nameof(HostInfo.PingInfo) + "." + nameof(PingInfo.IPAddressInt32), ListSortDirection.Ascending));

            // Add default custom commands...
            if (SettingsManager.Current.IPScanner_CustomCommands.Count == 0)
                SettingsManager.Current.IPScanner_CustomCommands = new ObservableCollection<CustomCommandInfo>(Utilities.CustomCommand.GetDefaults());

            LoadSettings();

            // Detect if settings have changed...
            SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;

            _isLoading = false;
        }

        public void OnLoaded()
        {
            if (!_firstLoad)
                return;

            if (!string.IsNullOrEmpty(Hosts))
                StartScan();

            _firstLoad = false;
        }

        private void LoadSettings()
        {

        }
        #endregion

        #region ICommands & Actions

        public ICommand ScanCommand => new RelayCommand(p => ScanAction(), Scan_CanExecute);

        private bool Scan_CanExecute(object paramter)
        {
            return Application.Current.MainWindow != null && !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen;
        }

        private void ScanAction()
        {
            Scan();
        }

        public ICommand DetectSubnetCommand => new RelayCommand(p => DetectSubnetAction());

        private void DetectSubnetAction()
        {
            DetectIPRange();
        }

        public ICommand RedirectDataToApplicationCommand => new RelayCommand(RedirectDataToApplicationAction);

        private void RedirectDataToApplicationAction(object name)
        {
            if (!(name is string appName))
                return;

            if (!Enum.TryParse(appName, out ApplicationName applicationName))
                return;

            var host = !string.IsNullOrEmpty(SelectedResult.Hostname) ? SelectedResult.Hostname : SelectedResult.PingInfo.IPAddress.ToString();

            EventSystem.RedirectToApplication(applicationName, host);
        }

        public ICommand PerformDNSLookupIPAddressCommand => new RelayCommand(p => PerformDNSLookupIPAddressAction());

        private void PerformDNSLookupIPAddressAction()
        {
            EventSystem.RedirectToApplication(ApplicationName.DNSLookup, SelectedResult.PingInfo.IPAddress.ToString());
        }

        public ICommand PerformDNSLookupHostnameCommand => new RelayCommand(p => PerformDNSLookupHostnameAction());

        private void PerformDNSLookupHostnameAction()
        {
            EventSystem.RedirectToApplication(ApplicationName.DNSLookup, SelectedResult.Hostname);
        }

        public ICommand CustomCommandCommand => new RelayCommand(CustomCommandAction);

        private void CustomCommandAction(object guid)
        {
            CustomCommand(guid);
        }

        public ICommand AddProfileSelectedHostCommand => new RelayCommand(p => AddProfileSelectedHostAction());

        private async Task AddProfileSelectedHostAction()
        {
            ProfileInfo profileInfo = new ProfileInfo()
            {
                Name = string.IsNullOrEmpty(SelectedResult.Hostname) ? SelectedResult.PingInfo.IPAddress.ToString() : SelectedResult.Hostname.TrimEnd('.'),
                Host = SelectedResult.PingInfo.IPAddress.ToString()
            };

            var customDialog = new CustomDialog
            {
                Title = Localization.Resources.Strings.AddProfile
            };

            var profileViewModel = new ProfileViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                ProfileManager.AddProfile(ProfileDialogManager.ParseProfileInfo(instance));
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, ProfileManager.GetGroupNames(), null, ProfileEditMode.Add, profileInfo);

            customDialog.Content = new ProfileDialog
            {
                DataContext = profileViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand CopySelectedIPAddressCommand => new RelayCommand(p => CopySelectedIPAddressAction());

        private void CopySelectedIPAddressAction()
        {
            ClipboardHelper.SetClipboard(SelectedResult.PingInfo.IPAddress.ToString());
        }

        public ICommand CopySelectedHostnameCommand => new RelayCommand(p => CopySelectedHostnameAction());

        private void CopySelectedHostnameAction()
        {
            ClipboardHelper.SetClipboard(SelectedResult.Hostname);
        }

        public ICommand CopySelectedMACAddressCommand => new RelayCommand(p => CopySelectedMACAddressAction());

        private void CopySelectedMACAddressAction()
        {
            ClipboardHelper.SetClipboard(MACAddressHelper.GetDefaultFormat(SelectedResult.MACAddress.ToString()));
        }

        public ICommand CopySelectedVendorCommand => new RelayCommand(p => CopySelectedVendorAction());

        private void CopySelectedVendorAction()
        {
            ClipboardHelper.SetClipboard(SelectedResult.Vendor);
        }

        public ICommand CopySelectedBytesCommand => new RelayCommand(p => CopySelectedBytesAction());

        private void CopySelectedBytesAction()
        {
            ClipboardHelper.SetClipboard(SelectedResult.PingInfo.Bytes.ToString());
        }

        public ICommand CopySelectedTimeCommand => new RelayCommand(p => CopySelectedTimeAction());

        private void CopySelectedTimeAction()
        {
            ClipboardHelper.SetClipboard(SelectedResult.PingInfo.Time.ToString());
        }

        public ICommand CopySelectedTTLCommand => new RelayCommand(p => CopySelectedTTLAction());

        private void CopySelectedTTLAction()
        {
            ClipboardHelper.SetClipboard(SelectedResult.PingInfo.TTL.ToString());
        }

        public ICommand CopySelectedStatusCommand => new RelayCommand(p => CopySelectedStatusAction());

        private void CopySelectedStatusAction()
        {            
            ClipboardHelper.SetClipboard(IPStatusTranslator.GetInstance().Translate(SelectedResult.PingInfo.Status));
        }

        public ICommand ExportCommand => new RelayCommand(p => ExportAction());

        private void ExportAction()
        {
            Export();
        }
        #endregion

        #region Methods
        private void Scan()
        {
            if (IsScanRunning)
                StopScan();
            else
                StartScan();
        }

        private async Task StartScan()
        {
            IsStatusMessageDisplayed = false;
            IsScanRunning = true;
            PreparingScan = true;
                       
            Results.Clear();

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

            HostsToScan = ipAddresses.Length;
            HostsScanned = 0;

            PreparingScan = false;

            // Add host(s) to the history
            AddHostToHistory(Hosts);

            var ipScanner = new IPScanner
            {
                Threads = SettingsManager.Current.IPScanner_Threads,
                ICMPTimeout = SettingsManager.Current.IPScanner_ICMPTimeout,
                ICMPBuffer = new byte[SettingsManager.Current.IPScanner_ICMPBuffer],
                ICMPAttempts = SettingsManager.Current.IPScanner_ICMPAttempts,
                ResolveHostname = SettingsManager.Current.IPScanner_ResolveHostname,
                UseCustomDNSServer = SettingsManager.Current.IPScanner_UseCustomDNSServer,
                DNSUseTCPOnly = SettingsManager.Current.IPScanner_DNSUseTCPOnly,
                DNSRecursion = SettingsManager.Current.IPScanner_DNSRecursion,
                DNSUseCache = SettingsManager.Current.IPScanner_DNSUseCache,
                DNSTimeout = TimeSpan.FromSeconds(SettingsManager.Current.IPScanner_DNSTimeout),
                DNSRetries = SettingsManager.Current.IPScanner_DNSRetries,
                DNSShowErrorMessage = SettingsManager.Current.IPScanner_DNSShowErrorMessage,
                ResolveMACAddress = SettingsManager.Current.IPScanner_ResolveMACAddress,
                ShowScanResultForAllIPAddresses = SettingsManager.Current.IPScanner_ShowScanResultForAllIPAddresses
            };

            // Set custom dns server...
            if (ipScanner.ResolveHostname && ipScanner.UseCustomDNSServer)
            {
                ipScanner.CustomDNSServer = IPAddress.Parse(SettingsManager.Current.IPScanner_CustomDNSServer);
                ipScanner.CustomDNSPort = SettingsManager.Current.IPScanner_CustomDNSPort;
            }

            ipScanner.HostFound += HostFound;
            ipScanner.ScanComplete += ScanComplete;
            ipScanner.ProgressChanged += ProgressChanged;
            ipScanner.UserHasCanceled += UserHasCanceled;

            ipScanner.ScanAsync(ipAddresses, _cancellationTokenSource.Token);
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

        private async Task DetectIPRange()
        {
            IsSubnetDetectionRunning = true;

            var localIP = await NetworkInterface.DetectLocalIPAddressBasedOnRoutingAsync(IPAddress.Parse("1.1.1.1"));

            // Could not detect local ip address
            if (localIP != null)
            {
                var subnetmaskDetected = false;

                // Get subnetmask, based on ip address
                foreach (var networkInterface in await NetworkInterface.GetNetworkInterfacesAsync())
                {
                    if (networkInterface.IPv4Address.Any(x => x.Item1.Equals(localIP)))
                    {
                        subnetmaskDetected = true;

                        Hosts = $"{localIP}/{Subnetmask.ConvertSubnetmaskToCidr(networkInterface.IPv4Address.First().Item2)}";

                        // Fix: If the user clears the textbox and then clicks again on the button, the textbox remains empty...
                        OnPropertyChanged(nameof(Hosts));

                        break;
                    }
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
                CustomCommandInfo info = (CustomCommandInfo)CustomCommands.FirstOrDefault(x => x.ID == id).Clone();

                if (info == null)
                    return; // ToDo: Log and error message

                // Replace vars
                string hostname = !string.IsNullOrEmpty(SelectedResult.Hostname) ? SelectedResult.Hostname.TrimEnd('.') : "";
                string ipAddress = SelectedResult.PingInfo.IPAddress.ToString();

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
            var list = ListHelper.Modify(SettingsManager.Current.IPScanner_HostsHistory.ToList(), ipRange, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.IPScanner_HostsHistory.Clear();
            OnPropertyChanged(nameof(Hosts)); // Raise property changed again, after the collection has been cleared

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.IPScanner_HostsHistory.Add(x));
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
                    ExportManager.Export(instance.FilePath, instance.FileType, instance.ExportAll ? Results : new ObservableCollection<HostInfo>(SelectedResults.Cast<HostInfo>().ToArray()));
                }
                catch (Exception ex)
                {
                    var settings = AppearanceManager.MetroDialog;
                    settings.AffirmativeButtonText = Localization.Resources.Strings.OK;

                    await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.Error, Localization.Resources.Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine + Environment.NewLine + ex.Message, MessageDialogStyle.Affirmative, settings);
                }

                SettingsManager.Current.IPScanner_ExportFileType = instance.FileType;
                SettingsManager.Current.IPScanner_ExportFilePath = instance.FilePath;
            }, instance => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); }, new ExportManager.ExportFileType[] { ExportManager.ExportFileType.CSV, ExportManager.ExportFileType.XML, ExportManager.ExportFileType.JSON }, true, SettingsManager.Current.IPScanner_ExportFileType, SettingsManager.Current.IPScanner_ExportFilePath);

            customDialog.Content = new ExportDialog
            {
                DataContext = exportViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public void OnClose()
        {
            // Stop scan
            if (IsScanRunning)
                StopScan();
        }

        #endregion

        #region Events
        private void HostFound(object sender, HostFoundArgs e)
        {
            var ipScannerHostInfo = HostInfo.Parse(e);

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
            {
                //lock (Results)
                    Results.Add(ipScannerHostInfo);
            }));
        }

        private void ScanComplete(object sender, EventArgs e)
        {
            ScanFinished();
        }

        private void ProgressChanged(object sender, ProgressChangedArgs e)
        {
            HostsScanned = e.Value;
        }

        private void DnsResolveFailed(AggregateException e)
        {
            StatusMessage = $"{Localization.Resources.Strings.TheFollowingHostnamesCouldNotBeResolved} {string.Join(", ", e.Flatten().InnerExceptions.Select(x => x.Message))}";
            IsStatusMessageDisplayed = true;

            ScanFinished();
        }

        private void UserHasCanceled(object sender, EventArgs e)
        {
            StatusMessage = Localization.Resources.Strings.CanceledByUserMessage;
            IsStatusMessageDisplayed = true;

            ScanFinished();
        }
       
        private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SettingsInfo.IPScanner_ResolveMACAddress):
                    OnPropertyChanged(nameof(ResolveMACAddress));
                    break;
                case nameof(SettingsInfo.IPScanner_ResolveHostname):
                    OnPropertyChanged(nameof(ResolveHostname));
                    break;             
            }
        }
        #endregion
    }
}