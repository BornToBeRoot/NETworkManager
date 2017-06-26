using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
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

namespace NETworkManager.ViewModels.Applications
{
    public class PortScannerViewModel : ViewModelBase
    {
        #region Variables
        private IDialogCoordinator dialogCoordinator;
        MetroDialogSettings dialogSettings = new MetroDialogSettings();

        CancellationTokenSource cancellationTokenSource;

        private bool _isLoading = true;

        private string _hostnameOrIPAddress;
        public string HostnameOrIPAddress
        {
            get { return _hostnameOrIPAddress; }
            set
            {
                if (value == _hostnameOrIPAddress)
                    return;

                _hostnameOrIPAddress = value;
                OnPropertyChanged();
            }
        }

        private List<string> _hostnameOrIPAddressHistory = new List<string>();
        public List<string> HostnameOrIPAddressHistory
        {
            get { return _hostnameOrIPAddressHistory; }
            set
            {
                if (value == _hostnameOrIPAddressHistory)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PortScanner_HostnameOrIPAddressHistory = value;

                _hostnameOrIPAddressHistory = value;
                OnPropertyChanged();
            }
        }

        private string _ports;
        public string Ports
        {
            get { return _ports; }
            set
            {
                if (value == _ports)
                    return;

                _ports = value;
                OnPropertyChanged();
            }
        }

        private List<string> _portsHistory = new List<string>();
        public List<string> PortsHistory
        {
            get { return _portsHistory; }
            set
            {
                if (value == _portsHistory)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PortScanner_PortsHistory = value;

                _portsHistory = value;
                OnPropertyChanged();
            }
        }

        private int _threads;
        public int Threads
        {
            get { return _threads; }
            set
            {
                if (value == _threads)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PortScanner_Threads = value;

                _threads = value;
                OnPropertyChanged();
            }
        }

        private bool _showClosed;
        public bool ShowClosed
        {
            get { return _showClosed; }
            set
            {
                if (value == _showClosed)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PortScanner_ShowClosed = value;

                _showClosed = value;
                OnPropertyChanged();
            }
        }

        private int _timeout;
        public int Timeout
        {
            get { return _timeout; }
            set
            {
                if (value == _timeout)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PortScanner_Timeout = value;

                _timeout = value;
                OnPropertyChanged();
            }
        }

        private bool _resolveHostnamePreferIPv4;
        public bool ResolveHostnamePreferIPv4
        {
            get { return _resolveHostnamePreferIPv4; }
            set
            {
                if (value == _resolveHostnamePreferIPv4)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PortScanner_ResolveHostnamePreferIPv4 = value;

                _resolveHostnamePreferIPv4 = value;
                OnPropertyChanged();
            }
        }

        private bool _resolveHostnamePreferIPv6;
        public bool ResolveHostnamePreferIPv6
        {
            get { return _resolveHostnamePreferIPv6; }
            set
            {
                if (value == _resolveHostnamePreferIPv6)
                    return;

                _resolveHostnamePreferIPv6 = value;
                OnPropertyChanged();
            }
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

        private int _progressBarMaximum;
        public int ProgressBarMaximum
        {
            get { return _progressBarMaximum; }
            set
            {
                if (value == _progressBarMaximum)
                    return;

                _progressBarMaximum = value;
                OnPropertyChanged();
            }
        }

        private int _progressBarValue;
        public int ProgressBarValue
        {
            get { return _progressBarValue; }
            set
            {
                if (value == _progressBarValue)
                    return;

                _progressBarValue = value;
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
        #endregion

        #region Constructor, Load settings
        public PortScannerViewModel(IDialogCoordinator instance)
        {
            dialogCoordinator = instance;

            dialogSettings.CustomResourceDictionary = new ResourceDictionary
            {
                Source = new Uri("NETworkManager;component/Resources/Styles/MetroDialogStyles.xaml", UriKind.RelativeOrAbsolute)
            };

            LoadSettings();

            _isLoading = false;
        }
        #endregion

        #region Settings
        private void LoadSettings()
        {
            if (SettingsManager.Current.PortScanner_HostnameOrIPAddressHistory != null)
                HostnameOrIPAddressHistory = new List<string>(SettingsManager.Current.PortScanner_HostnameOrIPAddressHistory);

            if (SettingsManager.Current.PortScanner_PortsHistory != null)
                PortsHistory = new List<string>(SettingsManager.Current.PortScanner_PortsHistory);

            Threads = SettingsManager.Current.PortScanner_Threads;
            ShowClosed = SettingsManager.Current.PortScanner_ShowClosed;
            Timeout = SettingsManager.Current.PortScanner_Timeout;

            if (SettingsManager.Current.PortScanner_ResolveHostnamePreferIPv4)
                ResolveHostnamePreferIPv4 = true;
            else
                ResolveHostnamePreferIPv6 = true;
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
        #endregion

        #region Methods
        private async void StartScan()
        {
            IsScanRunning = true;
            PreparingScan = true;

            PortScanResult.Clear();

            cancellationTokenSource = new CancellationTokenSource();

            // Try to parse the string into an IP-Address
            IPAddress.TryParse(HostnameOrIPAddress, out IPAddress ipAddress);

            try
            {
                // Resolve DNS
                // Try to resolve the hostname
                if (ipAddress == null)
                {
                    IPHostEntry ipHostEntrys = await Dns.GetHostEntryAsync(HostnameOrIPAddress);

                    foreach (IPAddress ip in ipHostEntrys.AddressList)
                    {
                        if (ip.AddressFamily == AddressFamily.InterNetwork && ResolveHostnamePreferIPv4)
                        {
                            ipAddress = ip;
                            continue;
                        }
                        else if (ip.AddressFamily == AddressFamily.InterNetworkV6 && !ResolveHostnamePreferIPv4)
                        {
                            ipAddress = ip;
                            continue;
                        }
                    }

                    // Fallback --> If we could not resolve our prefered ip protocol
                    if (ipAddress == null)
                    {
                        foreach (IPAddress ip in ipHostEntrys.AddressList)
                        {
                            ipAddress = ip;
                            continue;
                        }
                    }
                }

                int[] ports = await PortRangeHelper.ConvertPortRangeToIntArrayAsync(Ports);

                ProgressBarMaximum = ports.Length;
                ProgressBarValue = 0;

                PreparingScan = false;

                HostnameOrIPAddressHistory = new List<string>(HistoryListHelper.Modify(HostnameOrIPAddressHistory, HostnameOrIPAddress, SettingsManager.Current.Application_HistoryListEntries));
                PortsHistory = new List<string>(HistoryListHelper.Modify(PortsHistory, Ports, SettingsManager.Current.Application_HistoryListEntries));

                PortScannerOptions portScannerOptions = new PortScannerOptions
                {
                    Threads = Threads,
                    ShowClosed = ShowClosed,
                    Timeout = Timeout
                };

                PortScanner portScanner = new PortScanner();
                portScanner.PortScanned += PortScanner_PortScanned;
                portScanner.ScanComplete += PortScanner_ScanComplete;
                portScanner.ProgressChanged += PortScanner_ProgressChanged;
                portScanner.UserHasCanceled += PortScanner_UserHasCanceled;

                portScanner.ScanAsync(ipAddress, ports, portScannerOptions, cancellationTokenSource.Token);
            }
            catch (SocketException) // This will catch DNS resolve errors
            {
                IsScanRunning = false;
                await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_Header_DnsError"] as string, Application.Current.Resources["String_CouldNotResolveHostnameMessage"] as string, MessageDialogStyle.Affirmative, dialogSettings);
            }
            catch (Exception ex) // This will catch any exception
            {
                IsScanRunning = false;
                await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_Header_Error"] as string, ex.Message, MessageDialogStyle.Affirmative, dialogSettings);
            }
        }

        #region Events
        private async void PortScanner_UserHasCanceled(object sender, EventArgs e)
        {
            CancelScan = false;
            IsScanRunning = false;

            await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_Header_CanceledByUser"] as string, Application.Current.Resources["String_CanceledByUserMessage"] as string, MessageDialogStyle.Affirmative, dialogSettings);
        }

        private void PortScanner_ProgressChanged(object sender, ProgressChangedArgs e)
        {
            ProgressBarValue = e.Value;
        }

        private void PortScanner_ScanComplete(object sender, EventArgs e)
        {
            IsScanRunning = false;
        }

        private void PortScanner_PortScanned(object sender, PortScannedArgs e)
        {
            PortInfo portInfo = PortInfo.Parse(e);

            Application.Current.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                PortScanResult.Add(portInfo);
            }));
        }
        #endregion

        private void StopScan()
        {
            CancelScan = true;
            cancellationTokenSource.Cancel();
        }
        #endregion

        #region OnShutdown
        public void OnShutdown()
        {
            if (IsScanRunning)
                StopScan();
        }
        #endregion
    }
}