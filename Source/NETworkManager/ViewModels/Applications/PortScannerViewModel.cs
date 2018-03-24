using System.Windows.Input;
using System.Windows;
using System;
using System.Collections.ObjectModel;
using NETworkManager.Models.Settings;
using System.Collections.Generic;
using NETworkManager.Models.Network;
using NETworkManager.Helpers;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Windows.Threading;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Data;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels.Dialogs;
using NETworkManager.Views.Dialogs;
using System.Linq;

namespace NETworkManager.ViewModels.Applications
{
    public class PortScannerViewModel : ViewModelBase
    {
        #region Variables
        private IDialogCoordinator dialogCoordinator;
        CancellationTokenSource cancellationTokenSource;

        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        Stopwatch stopwatch = new Stopwatch();

        private bool _isLoading = true;

        private string _host;
        public string Host
        {
            get { return _host; }
            set
            {
                if (value == _host)
                    return;

                _host = value;
                OnPropertyChanged();
            }
        }

        private ICollectionView _hostHistoryView;
        public ICollectionView HostHistoryView
        {
            get { return _hostHistoryView; }
        }

        private string _port;
        public string Port
        {
            get { return _port; }
            set
            {
                if (value == _port)
                    return;

                _port = value;
                OnPropertyChanged();
            }
        }

        private ICollectionView _portHistoryView;
        public ICollectionView PortHistoryView
        {
            get { return _portHistoryView; }
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

                CheckCanScanProfile();

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

        private ObservableCollection<PortInfo> _portScanResult = new ObservableCollection<PortInfo>();
        public ObservableCollection<PortInfo> PortScanResult
        {
            get { return _portScanResult; }
            set
            {
                if (value == _portScanResult)
                    return;

                _portScanResult = value;
            }
        }

        private ICollectionView _portScanResultView;
        public ICollectionView PortScanResultView
        {
            get { return _portScanResultView; }
        }

        private PortInfo _selectedScanResult;
        public PortInfo SelectedScanResult
        {
            get { return _selectedScanResult; }
            set
            {
                if (value == _selectedScanResult)
                    return;

                _selectedScanResult = value;
                OnPropertyChanged();
            }
        }

        private int _portsToScan;
        public int PortsToScan
        {
            get { return _portsToScan; }
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
            get { return _portsScanned; }
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
            get { return _portsOpen; }
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
            get { return _preparingScan; }
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
                    SettingsManager.Current.PortScanner_ExpandStatistics = value;

                _expandStatistics = value;
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

        #region Profiles
        ICollectionView _portScannerProfiles;
        public ICollectionView PortScannerProfiles
        {
            get { return _portScannerProfiles; }
        }

        private PortScannerProfileInfo _selectedProfile = new PortScannerProfileInfo();
        public PortScannerProfileInfo SelectedProfile
        {
            get { return _selectedProfile; }
            set
            {
                if (value == _selectedProfile)
                    return;

                _selectedProfile = value;

                if (value != null)
                {
                    Host = value.Hostname;
                    Port = value.Ports;

                    CheckCanScanProfile();
                }

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
                    SettingsManager.Current.PortScanner_ExpandProfileView = value;

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

                PortScannerProfiles.Refresh();

                OnPropertyChanged();
            }
        }

        private bool _canScanProfile;
        public bool CanScanProfile
        {
            get { return _canScanProfile; }
            set
            {
                if (value == _canScanProfile)
                    return;

                _canScanProfile = value;
                OnPropertyChanged();
            }
        }
        #endregion
        #endregion

        #region Constructor, load settings, shutdown
        public PortScannerViewModel(IDialogCoordinator instance)
        {
            dialogCoordinator = instance;

            // Set collection view
            _hostHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.PortScanner_HostHistory);
            _portHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.PortScanner_PortHistory);

            // Result view
            _portScanResultView = CollectionViewSource.GetDefaultView(PortScanResult);

            // Load profiles
            if (PortScannerProfileManager.Profiles == null)
                PortScannerProfileManager.Load();

            _portScannerProfiles = CollectionViewSource.GetDefaultView(PortScannerProfileManager.Profiles);
            _portScannerProfiles.GroupDescriptions.Add(new PropertyGroupDescription(nameof(PortScannerProfileInfo.Group)));
            _portScannerProfiles.SortDescriptions.Add(new SortDescription(nameof(PortScannerProfileInfo.Group), ListSortDirection.Ascending));
            _portScannerProfiles.SortDescriptions.Add(new SortDescription(nameof(PortScannerProfileInfo.Name), ListSortDirection.Ascending));
            _portScannerProfiles.Filter = o =>
            {
                if (string.IsNullOrEmpty(Search))
                    return true;

                PortScannerProfileInfo info = o as PortScannerProfileInfo;

                string search = Search.Trim();

                // Search by: Name
                return info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1;
            };

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            ExpandStatistics = SettingsManager.Current.PortScanner_ExpandStatistics;
            ExpandProfileView = SettingsManager.Current.PortScanner_ExpandProfileView;
        }

        public void OnShutdown()
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

        public ICommand ScanProfileCommand
        {
            get { return new RelayCommand(p => ScanProfileAction()); }
        }

        private void ScanProfileAction()
        {
            Host = SelectedProfile.Hostname;
            Port = SelectedProfile.Ports;

            StartScan();
        }

        public ICommand CopySelectedIPAddressCommand
        {
            get { return new RelayCommand(p => CopySelectedIPAddressAction()); }
        }

        private void CopySelectedIPAddressAction()
        {
            Clipboard.SetText(SelectedScanResult.Host.Item1.ToString());
        }

        public ICommand CopySelectedHostnameCommand
        {
            get { return new RelayCommand(p => CopySelectedHostnameAction()); }
        }

        private void CopySelectedHostnameAction()
        {
            Clipboard.SetText(SelectedScanResult.Host.Item2);
        }

        public ICommand CopySelectedPortCommand
        {
            get { return new RelayCommand(p => CopySelectedPortAction()); }
        }

        private void CopySelectedPortAction()
        {
            Clipboard.SetText(SelectedScanResult.Port.ToString());
        }

        public ICommand CopySelectedStatusCommand
        {
            get { return new RelayCommand(p => CopySelectedStatusAction()); }
        }

        private void CopySelectedStatusAction()
        {
            Clipboard.SetText(Application.Current.Resources["String_PortStatus_" + SelectedScanResult.Status.ToString()] as string);
        }

        public ICommand CopySelectedProtocolCommand
        {
            get { return new RelayCommand(p => CopySelectedProtocolAction()); }
        }

        private void CopySelectedProtocolAction()
        {
            Clipboard.SetText(SelectedScanResult.LookupInfo.Protocol.ToString());
        }

        public ICommand CopySelectedServiceCommand
        {
            get { return new RelayCommand(p => CopySelectedServiceAction()); }
        }

        private void CopySelectedServiceAction()
        {
            Clipboard.SetText(SelectedScanResult.LookupInfo.Service);
        }

        public ICommand CopySelectedDescriptionCommand
        {
            get { return new RelayCommand(p => CopySelectedDescriptionAction()); }
        }

        private void CopySelectedDescriptionAction()
        {
            Clipboard.SetText(SelectedScanResult.LookupInfo.Description);
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

            PortScannerProfileViewModel portScannerProfileViewModel = new PortScannerProfileViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                PortScannerProfileInfo portScannerProfileInfo = new PortScannerProfileInfo
                {
                    Name = instance.Name,
                    Hostname = instance.Hostname,
                    Ports = instance.Ports,
                    Group = instance.Group
                };

                PortScannerProfileManager.AddProfile(portScannerProfileInfo);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, PortScannerProfileManager.GetProfileGroups(), new PortScannerProfileInfo() { Hostname = Host, Ports = Port });

            customDialog.Content = new PortScannerProfileDialog
            {
                DataContext = portScannerProfileViewModel
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

            PortScannerProfileViewModel portScannerProfileViewModel = new PortScannerProfileViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                PortScannerProfileManager.RemoveProfile(SelectedProfile);

                PortScannerProfileInfo portScannerProfileInfo = new PortScannerProfileInfo
                {
                    Name = instance.Name,
                    Hostname = instance.Hostname,
                    Ports = instance.Ports,
                    Group = instance.Group
                };

                PortScannerProfileManager.AddProfile(portScannerProfileInfo);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, PortScannerProfileManager.GetProfileGroups(), SelectedProfile);

            customDialog.Content = new PortScannerProfileDialog
            {
                DataContext = portScannerProfileViewModel
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

            PortScannerProfileViewModel portScannerProfileViewModel = new PortScannerProfileViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                PortScannerProfileInfo portScannerProfileInfo = new PortScannerProfileInfo
                {
                    Name = instance.Name,
                    Hostname = instance.Hostname,
                    Ports = instance.Ports,
                    Group = instance.Group
                };

                PortScannerProfileManager.AddProfile(portScannerProfileInfo);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, PortScannerProfileManager.GetProfileGroups(), SelectedProfile);

            customDialog.Content = new PortScannerProfileDialog
            {
                DataContext = portScannerProfileViewModel
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

                PortScannerProfileManager.RemoveProfile(SelectedProfile);
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

        public ICommand EditGroupCommand
        {
            get { return new RelayCommand(p => EditGroupAction(p)); }
        }

        private async void EditGroupAction(object group)
        {
            CustomDialog customDialog = new CustomDialog()
            {
                Title = Application.Current.Resources["String_Header_EditGroup"] as string
            };

            EditGroupViewModel editGroupViewModel = new EditGroupViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                PortScannerProfileManager.RenameGroup(instance.OldGroup, instance.Group);

                _portScannerProfiles.Refresh();
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, group.ToString());

            customDialog.Content = new EditGroupDialog
            {
                DataContext = editGroupViewModel
            };

            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }
        #endregion

        #region Methods
        private async void StartScan()
        {
            DisplayStatusMessage = false;
            StatusMessage = string.Empty;

            IsScanRunning = true;
            PreparingScan = true;

            // Measure the time
            StartTime = DateTime.Now;
            stopwatch.Start();
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            dispatcherTimer.Start();
            EndTime = null;

            PortScanResult.Clear();
            PortsOpen = 0;

            cancellationTokenSource = new CancellationTokenSource();

            string[] hosts = Host.Split(';');

            List<Tuple<IPAddress, string>> hostData = new List<Tuple<IPAddress, string>>();

            for (int i = 0; i < hosts.Length; i++)
            {
                string host = hosts[i].Trim();
                string hostname = string.Empty;
                IPAddress.TryParse(host, out IPAddress ipAddress);

                try
                {
                    // Resolve DNS
                    // Try to resolve the hostname
                    if (ipAddress == null)
                    {
                        IPHostEntry ipHostEntry = await Dns.GetHostEntryAsync(host);

                        foreach (IPAddress ip in ipHostEntry.AddressList)
                        {
                            if (ip.AddressFamily == AddressFamily.InterNetwork && SettingsManager.Current.PortScanner_ResolveHostnamePreferIPv4)
                            {
                                ipAddress = ip;
                                continue;
                            }
                            else if (ip.AddressFamily == AddressFamily.InterNetworkV6 && !SettingsManager.Current.PortScanner_ResolveHostnamePreferIPv4)
                            {
                                ipAddress = ip;
                                continue;
                            }
                        }

                        // Fallback --> If we could not resolve our prefered ip protocol
                        if (ipAddress == null)
                        {
                            foreach (IPAddress ip in ipHostEntry.AddressList)
                            {
                                ipAddress = ip;
                                continue;
                            }
                        }

                        hostname = host;
                    }
                    else
                    {
                        try
                        {
                            IPHostEntry ipHostEntry = await Dns.GetHostEntryAsync(ipAddress);

                            hostname = ipHostEntry.HostName;
                        }
                        catch { }
                    }
                }
                catch (SocketException) // This will catch DNS resolve errors
                {
                    if (!string.IsNullOrEmpty(StatusMessage))
                        StatusMessage += Environment.NewLine;

                    StatusMessage += string.Format(Application.Current.Resources["String_CouldNotResolveHostnameFor"] as string, host);
                    DisplayStatusMessage = true;

                    continue;
                }

                hostData.Add(Tuple.Create(ipAddress, hostname));
            }

            if (hostData.Count == 0)
            {
                StatusMessage += Environment.NewLine + Application.Current.Resources["String_NothingToDoCheckYourInput"] as string;
                DisplayStatusMessage = true;

                ScanFinished();

                return;
            }

            int[] ports = await PortRangeHelper.ConvertPortRangeToIntArrayAsync(Port);

            try
            {
                PortsToScan = ports.Length * hostData.Count;
                PortsScanned = 0;

                PreparingScan = false;

                AddHostToHistory(Host);
                AddPortToHistory(Port);

                PortScannerOptions portScannerOptions = new PortScannerOptions
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

                portScanner.ScanAsync(hostData, ports, portScannerOptions, cancellationTokenSource.Token);
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

        private void CheckCanScanProfile()
        {
            CanScanProfile = !IsScanRunning && !string.IsNullOrEmpty(SelectedProfile.Hostname) && !string.IsNullOrEmpty(SelectedProfile.Ports);
        }

        private void AddHostToHistory(string host)
        {
            // Create the new list
            List<string> list = ListHelper.Modify(SettingsManager.Current.PortScanner_HostHistory.ToList(), host, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.PortScanner_HostHistory.Clear();
            OnPropertyChanged(nameof(Host)); // Raise property changed again, after the collection has been cleared

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.PortScanner_HostHistory.Add(x));
        }

        private void AddPortToHistory(string port)
        {
            // Create the new list
            List<string> list = ListHelper.Modify(SettingsManager.Current.PortScanner_PortHistory.ToList(), port, SettingsManager.Current.General_HistoryListEntries);

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
            StatusMessage = Application.Current.Resources["String_CanceledByUser"] as string;
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
            PortInfo portInfo = PortInfo.Parse(e);

            Application.Current.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                PortScanResult.Add(portInfo);
            }));

            if (portInfo.Status == PortInfo.PortStatus.Open)
                PortsOpen++;
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            Duration = stopwatch.Elapsed;
        }
        #endregion               
    }
}