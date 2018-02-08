using NETworkManager.Models.Settings;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows.Input;
using System.Windows;
using System;
using System.Threading;
using System.Collections.Generic;
using NETworkManager.Models.Network;
using NETworkManager.Helpers;
using System.Windows.Threading;
using System.Diagnostics;
using System.ComponentModel;
using MahApps.Metro.Controls.Dialogs;
using System.Windows.Data;
using NETworkManager.ViewModels.Dialogs;
using NETworkManager.Views.Dialogs;
using System.Linq;

namespace NETworkManager.ViewModels.Applications
{
    public class IPScannerViewModel : ViewModelBase
    {
        #region Variables
        private IDialogCoordinator dialogCoordinator;
        CancellationTokenSource cancellationTokenSource;

        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        Stopwatch stopwatch = new Stopwatch();

        private bool _isLoading = true;

        private string _ipRange;
        public string IPRange
        {
            get { return _ipRange; }
            set
            {
                if (value == _ipRange)
                    return;

                _ipRange = value;
                OnPropertyChanged();
            }
        }

        private ICollectionView _ipRangeHistoryView;
        public ICollectionView IPRangeHistoryView
        {
            get { return _ipRangeHistoryView; }
        }

        private bool _isScanRunning;
        public bool IsScanRunning
        {
            get { return _isScanRunning; }
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
            get { return _cancelScan; }
            set
            {
                if (value == _cancelScan)
                    return;

                _cancelScan = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<IPScannerHostInfo> _ipScanResult = new ObservableCollection<IPScannerHostInfo>();
        public ObservableCollection<IPScannerHostInfo> IPScanResult
        {
            get { return _ipScanResult; }
            set
            {
                if (value == _ipScanResult)
                    return;

                _ipScanResult = value;
            }
        }

        private ICollectionView _ipScanResultView;
        public ICollectionView IPScanResultView
        {
            get { return _ipScanResultView; }
        }

        private IPScannerHostInfo _selectedIPScanResult;
        public IPScannerHostInfo SelectedIPScanResult
        {
            get { return _selectedIPScanResult; }
            set
            {
                if (value == _selectedIPScanResult)
                    return;

                _selectedIPScanResult = value;
                OnPropertyChanged();
            }
        }

        public bool ResolveHostname
        {
            get { return SettingsManager.Current.IPScanner_ResolveHostname; }
        }

        public bool ResolveMACAddress
        {
            get { return SettingsManager.Current.IPScanner_ResolveMACAddress; }
        }

        private int _ipAddressesToScan;
        public int IPAddressesToScan
        {
            get { return _ipAddressesToScan; }
            set
            {
                if (value == _ipAddressesToScan)
                    return;

                _ipAddressesToScan = value;
                OnPropertyChanged();
            }
        }

        private int _ipAddressesScanned;
        public int IPAddressesScanned
        {
            get { return _ipAddressesScanned; }
            set
            {
                if (value == _ipAddressesScanned)
                    return;

                _ipAddressesScanned = value;
                OnPropertyChanged();
            }
        }

        private int _hostsFound;
        public int HostsFound
        {
            get { return _hostsFound; }
            set
            {
                if (value == _hostsFound)
                    return;

                _hostsFound = value;
                OnPropertyChanged();
            }
        }

        private bool _preparingScan;
        public bool PreparingScan
        {
            get { return _preparingScan; }
            set
            {
                if (value == _preparingScan)
                    return;

                _preparingScan = value;
                OnPropertyChanged();
            }
        }

        private bool _displayStatusMessage;
        public bool DisplayStatusMessage
        {
            get { return _displayStatusMessage; }
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
            get { return _statusMessage; }
            set
            {
                if (value == _statusMessage)
                    return;

                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        private DateTime? _startTime;
        public DateTime? StartTime
        {
            get { return _startTime; }
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
            get { return _duration; }
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
            get { return _endTime; }
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
            get { return _expandStatistics; }
            set
            {
                if (value == _expandStatistics)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.IPScanner_ExpandStatistics = value;

                _expandStatistics = value;
                OnPropertyChanged();
            }
        }

        #region Profiles
        ICollectionView _ipScannerProfiles;
        public ICollectionView IPScannerProfiles
        {
            get { return _ipScannerProfiles; }
        }

        private IPScannerProfileInfo _selectedProfile = new IPScannerProfileInfo();
        public IPScannerProfileInfo SelectedProfile
        {
            get { return _selectedProfile; }
            set
            {
                if (value == _selectedProfile)
                    return;

                if (value != null)
                    IPRange = value.IPRange;

                _selectedProfile = value;
                OnPropertyChanged();
            }
        }

        private bool _expandProfileView;
        public bool ExpandProfileView
        {
            get { return _expandProfileView; }
            set
            {
                if (value == _expandProfileView)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.IPScanner_ExpandProfileView = value;

                _expandProfileView = value;
                OnPropertyChanged();
            }
        }

        private string _search;
        public string Search
        {
            get { return _search; }
            set
            {
                if (value == _search)
                    return;

                _search = value;

                IPScannerProfiles.Refresh();

                OnPropertyChanged();
            }
        }
        #endregion
        #endregion

        #region Constructor, load settings, shutdown
        public IPScannerViewModel(IDialogCoordinator instance)
        {
            dialogCoordinator = instance;

            // Set collection view
            _ipRangeHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.IPScanner_IPRangeHistory);

            // Result view
            _ipScanResultView = CollectionViewSource.GetDefaultView(IPScanResult);
            _ipScanResultView.SortDescriptions.Add(new SortDescription(nameof(PingInfo.IPAddressInt32), ListSortDirection.Ascending));

            // Load profiles
            if (IPScannerProfileManager.Profiles == null)
                IPScannerProfileManager.Load();

            _ipScannerProfiles = CollectionViewSource.GetDefaultView(IPScannerProfileManager.Profiles);
            _ipScannerProfiles.GroupDescriptions.Add(new PropertyGroupDescription(nameof(IPScannerProfileInfo.Group)));
            _ipScannerProfiles.SortDescriptions.Add(new SortDescription(nameof(IPScannerProfileInfo.Group), ListSortDirection.Ascending));
            _ipScannerProfiles.SortDescriptions.Add(new SortDescription(nameof(IPScannerProfileInfo.Name), ListSortDirection.Ascending));
            _ipScannerProfiles.Filter = o =>
            {
                if (string.IsNullOrEmpty(Search))
                    return true;

                IPScannerProfileInfo info = o as IPScannerProfileInfo;

                string search = Search.Trim();

                // Search by: Name
                return info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1;
            };

            LoadSettings();

            // Detect if settings have changed...
            SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;

            _isLoading = false;
        }

        private void LoadSettings()
        {
            ExpandStatistics = SettingsManager.Current.IPScanner_ExpandStatistics;
            ExpandProfileView = SettingsManager.Current.IPScanner_ExpandProfileView;
        }

        public void OnShutdown()
        {
            // Stop scan
            if (IsScanRunning)
                StopScan();
        }

        private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SettingsInfo.IPScanner_ResolveMACAddress))
                OnPropertyChanged(nameof(ResolveMACAddress));

            if (e.PropertyName == nameof(SettingsInfo.IPScanner_ResolveHostname))
                OnPropertyChanged(nameof(ResolveHostname));
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

        public ICommand ScanProfileCommand
        {
            get { return new RelayCommand(p => ScanProfileAction()); }
        }

        private void ScanProfileAction()
        {
            IPRange = SelectedProfile.IPRange;

            StartScan();
        }

        public ICommand CopySelectedIPAddressCommand
        {
            get { return new RelayCommand(p => CopySelectedIPAddressAction()); }
        }

        private void CopySelectedIPAddressAction()
        {
            Clipboard.SetText(SelectedIPScanResult.PingInfo.IPAddress.ToString());
        }

        public ICommand CopySelectedHostnameCommand
        {
            get { return new RelayCommand(p => CopySelectedHostnameAction()); }
        }

        private void CopySelectedHostnameAction()
        {
            Clipboard.SetText(SelectedIPScanResult.Hostname);
        }

        public ICommand CopySelectedMACAddressCommand
        {
            get { return new RelayCommand(p => CopySelectedMACAddressAction()); }
        }

        private void CopySelectedMACAddressAction()
        {
            Clipboard.SetText(MACAddressHelper.GetDefaultFormat(SelectedIPScanResult.MACAddress.ToString()));
        }

        public ICommand CopySelectedVendorCommand
        {
            get { return new RelayCommand(p => CopySelectedVendorAction()); }
        }

        private void CopySelectedVendorAction()
        {
            Clipboard.SetText(SelectedIPScanResult.Vendor);
        }

        public ICommand CopySelectedBytesCommand
        {
            get { return new RelayCommand(p => CopySelectedBytesAction()); }
        }

        private void CopySelectedBytesAction()
        {
            Clipboard.SetText(SelectedIPScanResult.PingInfo.Bytes.ToString());
        }

        public ICommand CopySelectedTimeCommand
        {
            get { return new RelayCommand(p => CopySelectedTimeAction()); }
        }

        private void CopySelectedTimeAction()
        {
            Clipboard.SetText(SelectedIPScanResult.PingInfo.Time.ToString());
        }

        public ICommand CopySelectedTTLCommand
        {
            get { return new RelayCommand(p => CopySelectedTTLAction()); }
        }

        private void CopySelectedTTLAction()
        {
            Clipboard.SetText(SelectedIPScanResult.PingInfo.TTL.ToString());
        }

        public ICommand CopySelectedStatusCommand
        {
            get { return new RelayCommand(p => CopySelectedStatusAction()); }
        }

        private void CopySelectedStatusAction()
        {          
            Clipboard.SetText(Application.Current.Resources["String_IPStatus_" + SelectedIPScanResult.PingInfo.Status.ToString()] as string);
        }        

        public ICommand AddProfileCommand
        {
            get { return new RelayCommand(p => AddProfileAction()); }
        }

        private async void AddProfileAction()
        {
            CustomDialog customDialog = new CustomDialog()
            {
                Title = Application.Current.Resources["String_Header_AddProfile"] as string
            };

            IPScannerProfileViewModel ipScannerProfileViewModel = new IPScannerProfileViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                IPScannerProfileInfo ipScannerProfileInfo = new IPScannerProfileInfo
                {
                    Name = instance.Name,
                    IPRange = instance.IPRange,
                    Group = instance.Group
                };

                IPScannerProfileManager.AddProfile(ipScannerProfileInfo);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, IPScannerProfileManager.GetProfileGroups(), new IPScannerProfileInfo() { IPRange = IPRange });

            customDialog.Content = new IPScannerProfileDialog
            {
                DataContext = ipScannerProfileViewModel
            };

            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand EditProfileCommand
        {
            get { return new RelayCommand(p => EditProfileAction()); }
        }

        private async void EditProfileAction()
        {
            CustomDialog customDialog = new CustomDialog()
            {
                Title = Application.Current.Resources["String_Header_EditProfile"] as string
            };

            IPScannerProfileViewModel ipScannerProfileViewModel = new IPScannerProfileViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                IPScannerProfileManager.RemoveProfile(SelectedProfile);

                IPScannerProfileInfo ipScannerProfileInfo = new IPScannerProfileInfo
                {
                    Name = instance.Name,
                    IPRange = instance.IPRange,
                    Group = instance.Group
                };

                IPScannerProfileManager.AddProfile(ipScannerProfileInfo);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, IPScannerProfileManager.GetProfileGroups(), SelectedProfile);

            customDialog.Content = new IPScannerProfileDialog
            {
                DataContext = ipScannerProfileViewModel
            };

            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand CopyAsProfileCommand
        {
            get { return new RelayCommand(p => CopyAsProfileAction()); }
        }

        private async void CopyAsProfileAction()
        {
            CustomDialog customDialog = new CustomDialog()
            {
                Title = Application.Current.Resources["String_Header_CopyProfile"] as string
            };

            IPScannerProfileViewModel ipScannerProfileViewModel = new IPScannerProfileViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                IPScannerProfileInfo ipScannerProfileInfo = new IPScannerProfileInfo
                {
                    Name = instance.Name,
                    IPRange = instance.IPRange,
                    Group = instance.Group
                };

                IPScannerProfileManager.AddProfile(ipScannerProfileInfo);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, IPScannerProfileManager.GetProfileGroups(), SelectedProfile);

            customDialog.Content = new IPScannerProfileDialog
            {
                DataContext = ipScannerProfileViewModel
            };

            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }
                
        public ICommand DeleteProfileCommand
        {
            get { return new RelayCommand(p => DeleteProfileAction()); }
        }

        private async void DeleteProfileAction()
        {

            CustomDialog customDialog = new CustomDialog()
            {
                Title = Application.Current.Resources["String_Header_DeleteProfile"] as string
            };

            ConfirmRemoveViewModel confirmRemoveViewModel = new ConfirmRemoveViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                IPScannerProfileManager.RemoveProfile(SelectedProfile);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, Application.Current.Resources["String_DeleteProfileMessage"] as string);

            customDialog.Content = new ConfirmRemoveDialog
            {
                DataContext = confirmRemoveViewModel
            };

            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }        
        #endregion

        #region Methods
        private async void StartScan()
        {
            DisplayStatusMessage = false;
            IsScanRunning = true;
            PreparingScan = true;

            // Measure the time
            StartTime = DateTime.Now;
            stopwatch.Start();
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            dispatcherTimer.Start();
            EndTime = null;

            IPScanResult.Clear();
            HostsFound = 0;

            cancellationTokenSource = new CancellationTokenSource();

            IPAddress[] ipAddresses;

            try
            {
                // Create a list of all ip addresses
                ipAddresses = await IPScanRangeHelper.ConvertIPRangeToIPAddressesAsync(IPRange, cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                IpScanner_UserHasCanceled(this, EventArgs.Empty);
                return;
            }

            IPAddressesToScan = ipAddresses.Length;
            IPAddressesScanned = 0;

            PreparingScan = false;

            // Add the range to the history
            AddIPRangeToHistory(IPRange);

            IPScannerOptions ipScannerOptions = new IPScannerOptions
            {
                Threads = SettingsManager.Current.IPScanner_Threads,
                Timeout = SettingsManager.Current.IPScanner_Timeout,
                Buffer = new byte[SettingsManager.Current.IPScanner_Buffer],
                Attempts = SettingsManager.Current.IPScanner_Attempts,
                ResolveHostname = SettingsManager.Current.IPScanner_ResolveHostname,
                ResolveMACAddress = SettingsManager.Current.IPScanner_ResolveMACAddress
            };

            IPScanner ipScanner = new IPScanner();

            ipScanner.HostFound += IpScanner_HostFound;
            ipScanner.ScanComplete += IpScanner_ScanComplete;
            ipScanner.ProgressChanged += IpScanner_ProgressChanged;
            ipScanner.UserHasCanceled += IpScanner_UserHasCanceled;

            ipScanner.ScanAsync(ipAddresses, ipScannerOptions, cancellationTokenSource.Token);
        }

        private void StopScan()
        {
            CancelScan = true;
            cancellationTokenSource.Cancel();
        }

        private void ScanFinished()
        {
            // Stop timer and stopwatch
            stopwatch.Stop();
            dispatcherTimer.Stop();

            Duration = stopwatch.Elapsed;
            EndTime = DateTime.Now;

            stopwatch.Reset();

            CancelScan = false;
            IsScanRunning = false;
        }

        private void AddIPRangeToHistory(string ipRange)
        {
            // Create the new list
            List<string> list = ListHelper.Modify(SettingsManager.Current.IPScanner_IPRangeHistory.ToList(), ipRange, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.IPScanner_IPRangeHistory.Clear();
            OnPropertyChanged(nameof(IPRange)); // Raise property changed again, after the collection has been cleared

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.IPScanner_IPRangeHistory.Add(x));
        }
        #endregion

        #region Events
        private void IpScanner_HostFound(object sender, IPScannerHostFoundArgs e)
        {
            IPScannerHostInfo ipScannerHostInfo = IPScannerHostInfo.Parse(e);

            Application.Current.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                IPScanResult.Add(ipScannerHostInfo);
            }));

            HostsFound++;
        }

        private void IpScanner_ScanComplete(object sender, EventArgs e)
        {
            ScanFinished();
        }

        private void IpScanner_ProgressChanged(object sender, ProgressChangedArgs e)
        {
            IPAddressesScanned = e.Value;
        }

        private void IpScanner_UserHasCanceled(object sender, EventArgs e)
        {
            StatusMessage = Application.Current.Resources["String_CanceledByUser"] as string;
            DisplayStatusMessage = true;

            ScanFinished();
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            Duration = stopwatch.Elapsed;
        }
        #endregion
    }
}
