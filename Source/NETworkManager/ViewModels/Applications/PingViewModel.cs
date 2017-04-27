using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Model.Common;
using NETworkManager.Model.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities.Common;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace NETworkManager.ViewModels.Applications
{
    class PingViewModel : ViewModelBase
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
                {
                    StringCollection collection = new StringCollection();
                    collection.AddRange(value.ToArray());

                    Properties.Settings.Default.Ping_HostnameOrIPAddressHistory = collection;

                    SettingsManager.SettingsChanged = true;
                }

                _hostnameOrIPAddressHistory = value;
                OnPropertyChanged();
            }
        }

        private int _attempts;
        public int Attempts
        {
            get { return _attempts; }
            set
            {
                if (value == _attempts)
                    return;

                if (!_isLoading)
                {
                    Properties.Settings.Default.Ping_Attempts = value;

                    SettingsManager.SettingsChanged = true;
                }

                _attempts = value;
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
                {
                    Properties.Settings.Default.Ping_Timeout = value;

                    SettingsManager.SettingsChanged = true;
                }

                _timeout = value;
                OnPropertyChanged();
            }
        }

        private int _buffer;
        public int Buffer
        {
            get { return _buffer; }
            set
            {
                if (value == _buffer)
                    return;

                if (!_isLoading)
                {
                    Properties.Settings.Default.Ping_Buffer = value;

                    SettingsManager.SettingsChanged = true;
                }

                _buffer = value;
                OnPropertyChanged();
            }
        }

        private int _ttl;
        public int TTL
        {
            get { return _ttl; }
            set
            {
                if (value == _ttl)
                    return;

                if (!_isLoading)
                {
                    Properties.Settings.Default.Ping_TTL = value;

                    SettingsManager.SettingsChanged = true;
                }

                _ttl = value;
                OnPropertyChanged();
            }
        }

        private bool _dontFragment;
        public bool DontFragment
        {
            get { return _dontFragment; }
            set
            {
                if (value == _dontFragment)
                    return;

                if (!_isLoading)
                {
                    Properties.Settings.Default.Ping_DontFragment = value;

                    SettingsManager.SettingsChanged = true;
                }

                _dontFragment = value;
                OnPropertyChanged();
            }
        }

        private int _waitTime;
        public int WaitTime
        {
            get { return _waitTime; }
            set
            {
                if (value == _waitTime)
                    return;

                if (!_isLoading)
                {
                    Properties.Settings.Default.Ping_WaitTime = value;

                    SettingsManager.SettingsChanged = true;
                }

                _waitTime = value;
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
                {
                    Properties.Settings.Default.Ping_ResolveHostnamePreferIPv4 = value;

                    SettingsManager.SettingsChanged = true;
                }

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

        private bool _isPingRunning;
        public bool IsPingRunning
        {
            get { return _isPingRunning; }
            set
            {
                if (value == _isPingRunning)
                    return;

                _isPingRunning = value;
                OnPropertyChanged();
            }
        }

        private bool _cancelPing;
        public bool CancelPing
        {
            get { return _cancelPing; }
            set
            {
                if (value == _cancelPing)
                    return;

                _cancelPing = value;
                OnPropertyChanged();
            }
        }

        private AsyncObservableCollection<PingInfo> _pingResult = new AsyncObservableCollection<PingInfo>();
        public AsyncObservableCollection<PingInfo> PingResult
        {
            get { return _pingResult; }
            set
            {
                if (value == _pingResult)
                    return;

                _pingResult = value;
            }
        }

        private int _pingsTransmitted;
        public int PingsTransmitted
        {
            get { return _pingsTransmitted; }
            set
            {
                if (value == _pingsTransmitted)
                    return;

                _pingsTransmitted = value;
                OnPropertyChanged();
            }
        }

        private int _pingsReceived;
        public int PingsReceived
        {
            get { return _pingsReceived; }
            set
            {
                if (value == _pingsReceived)
                    return;

                _pingsReceived = value;
                OnPropertyChanged();
            }
        }

        private int _pingsLost;
        public int PingsLost
        {
            get { return _pingsLost; }
            set
            {
                if (value == _pingsLost)
                    return;

                _pingsLost = value;
                OnPropertyChanged();
            }
        }

        private long _minimumTime;
        public long MinimumTime
        {
            get { return _minimumTime; }
            set
            {
                if (value == _minimumTime)
                    return;

                _minimumTime = value;
                OnPropertyChanged();
            }
        }

        private long _maximumTime;
        public long MaximumTime
        {
            get { return _maximumTime; }
            set
            {
                if (value == _maximumTime)
                    return;

                _maximumTime = value;
                OnPropertyChanged();
            }
        }

        private int _averageTime;
        public int AverageTime
        {
            get { return _averageTime; }
            set
            {
                if (value == _averageTime)
                    return;

                _averageTime = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Contructor
        public PingViewModel(IDialogCoordinator instance)
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

        #region Load settings
        private void LoadSettings()
        {
            if (Properties.Settings.Default.Ping_HostnameOrIPAddressHistory != null)
                HostnameOrIPAddressHistory = new List<string>(Properties.Settings.Default.Ping_HostnameOrIPAddressHistory.Cast<string>().ToList());

            Attempts = Properties.Settings.Default.Ping_Attempts;
            Timeout = Properties.Settings.Default.Ping_Timeout;
            Buffer = Properties.Settings.Default.Ping_Buffer;
            TTL = Properties.Settings.Default.Ping_TTL;
            DontFragment = Properties.Settings.Default.Ping_DontFragment;
            WaitTime = Properties.Settings.Default.Ping_WaitTime;

            if (Properties.Settings.Default.Ping_ResolveHostnamePreferIPv4)
                ResolveHostnamePreferIPv4 = true;
            else
                ResolveHostnamePreferIPv6 = true;
        }
        #endregion

        #region ICommands
        public ICommand PingCommand
        {
            get { return new RelayCommand(p => PingAction()); }
        }
        #endregion

        #region Methods
        private void PingAction()
        {
            if (IsPingRunning)
                StopPing();
            else
                StartPing();
        }

        private async void StartPing()
        {
            IsPingRunning = true;

            PingResult.Clear();
            PingsTransmitted = 0;
            PingsReceived = 0;
            PingsLost = 0;
            AverageTime = 0;
            MinimumTime = 0;
            MaximumTime = 0;

            // Try to parse the string into an IP-Address
            IPAddress ipAddress;
            IPAddress.TryParse(HostnameOrIPAddress, out ipAddress);

            try
            {
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

                    // Fallback --> If we could not resolve our prefered ip protocol for the hostname
                    if (ipAddress == null)
                    {
                        foreach (IPAddress ip in ipHostEntrys.AddressList)
                        {
                            ipAddress = ip;
                            continue;
                        }
                    }
                }
            }
            catch (SocketException) // This will catch DNS resolve errors
            {
                await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_Header_DnsError"] as string, Application.Current.Resources["String_CouldNotResolveHostnameMessage"] as string, MessageDialogStyle.Affirmative, dialogSettings);

                return;
            }

            // Add the hostname or ip address to the history
            HostnameOrIPAddressHistory = new List<string>(HistoryListHelper.Modify(HostnameOrIPAddressHistory, HostnameOrIPAddress, 5));

            cancellationTokenSource = new CancellationTokenSource();

            PingOptions pingOptions = new PingOptions()
            {
                Attempts = Attempts,
                Timeout = Timeout,
                Buffer = new byte[Buffer],
                TTL = TTL,
                DontFragment = DontFragment,
                WaitTime = WaitTime
            };

            Ping ping = new Ping();

            ping.PingReceived += Ping_PingReceived;
            ping.PingCompleted += Ping_PingCompleted;
            ping.UserHasCanceled += Ping_UserHasCanceled;

            ping.SendAsync(ipAddress, pingOptions, cancellationTokenSource.Token);
        }

        private void StopPing()
        {
            CancelPing = true;
            cancellationTokenSource.Cancel();
        }

        public void OnShutdown()
        {
            if (IsPingRunning)
                PingAction();
        }
        #endregion

        #region Events
        private void Ping_UserHasCanceled(object sender, EventArgs e)
        {
            CancelPing = false;
            IsPingRunning = false;
        }

        private void Ping_PingCompleted(object sender, EventArgs e)
        {
            IsPingRunning = false;
        }

        private void Ping_PingReceived(object sender, PingArgs e)
        {
            PingInfo pingInfo = PingInfo.Parse(e);

            // Add the result to the collection
            PingResult.Add(pingInfo);
            
            // Calculate statistics
            PingsTransmitted++;

            if (pingInfo.Status == System.Net.NetworkInformation.IPStatus.Success)
            {
                PingsReceived++;

                if (PingsReceived == 1)
                {
                    MinimumTime = pingInfo.Time;
                    MaximumTime = pingInfo.Time;
                }
                else
                {
                    if (MinimumTime > pingInfo.Time)
                        MinimumTime = pingInfo.Time;

                    if (MaximumTime < pingInfo.Time)
                        MaximumTime = pingInfo.Time;
                }

                // I don't know if this can slow my application if the collection is to large
                AverageTime = (int)PingResult.Average(s => s.Time);
            }
            else
            {
                PingsLost++;
            }
        }
        #endregion
    }
}