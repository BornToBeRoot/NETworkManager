using System.Net;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using System.Windows;
using System;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using NETworkManager.Settings;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using NETworkManager.Model.Network;
using System.Threading;
using NETworkManager.Utilities.Common;

namespace NETworkManager.ViewModels.Applications
{
    class TracerouteViewModel : ViewModelBase
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

                    Properties.Settings.Default.Traceroute_HostnameOrIPAddressHistory = collection;

                    SettingsManager.SettingsChanged = true;
                }

                _hostnameOrIPAddressHistory = value;
                OnPropertyChanged();
            }
        }

        private int _maximumHops;
        public int MaximumHops
        {
            get { return _maximumHops; }
            set
            {
                if (value == _maximumHops)
                    return;

                if (!_isLoading)
                {
                    Properties.Settings.Default.Traceroute_MaximumHops = value;

                    SettingsManager.SettingsChanged = true;
                }

                _maximumHops = value;
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
                    Properties.Settings.Default.Traceroute_Timeout = value;

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
                    Properties.Settings.Default.Traceroute_Buffer = value;

                    SettingsManager.SettingsChanged = true;
                }

                _buffer = value;
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
                    Properties.Settings.Default.Traceroute_ResolveHostnamePreferIPv4 = value;

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

        private bool _isTraceRunning;
        public bool IsTraceRunning
        {
            get { return _isTraceRunning; }
            set
            {
                if (value == _isTraceRunning)
                    return;

                _isTraceRunning = value;
                OnPropertyChanged();
            }
        }

        private bool _cancelTrace;
        public bool CancelTrace
        {
            get { return _cancelTrace; }
            set
            {
                if (value == _cancelTrace)
                    return;

                _cancelTrace = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<HopInfo> _traceResult = new ObservableCollection<HopInfo>();
        public ObservableCollection<HopInfo> TraceResult
        {
            get { return _traceResult; }
            set
            {
                if (value == _traceResult)
                    return;

                _traceResult = value;
            }
        }
        #endregion

        #region Constructor
        public TracerouteViewModel(IDialogCoordinator instance)
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
            if (Properties.Settings.Default.Traceroute_HostnameOrIPAddressHistory != null)
                HostnameOrIPAddressHistory = new List<string>(Properties.Settings.Default.Traceroute_HostnameOrIPAddressHistory.Cast<string>().ToList());

            MaximumHops = Properties.Settings.Default.Traceroute_MaximumHops;
            Timeout = Properties.Settings.Default.Traceroute_Timeout;
            Buffer = Properties.Settings.Default.Traceroute_Buffer;

            if (Properties.Settings.Default.Traceroute_ResolveHostnamePreferIPv4)
                ResolveHostnamePreferIPv4 = true;
            else
                ResolveHostnamePreferIPv6 = true;
        }
        #endregion

        #region ICommands
        public ICommand TraceCommand
        {
            get { return new RelayCommand(p => TraceAction()); }
        }
        #endregion

        #region Methods
        private void TraceAction()
        {
            if (IsTraceRunning)
                StopTrace();
            else
                StartTrace();
        }

        private void StopTrace()
        {
            CancelTrace = true;
            cancellationTokenSource.Cancel();
        }

        private async void StartTrace()
        {
            IsTraceRunning = true;
            TraceResult.Clear();

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

                cancellationTokenSource = new CancellationTokenSource();

                TracerouteOptions tracerouteOptions = new TracerouteOptions
                {
                    Timeout = Timeout,
                    Buffer = Buffer,
                    MaximumHops = MaximumHops,
                    DontFragement = true
                };

                Traceroute traceroute = new Traceroute();

                traceroute.HopReceived += Traceroute_HopReceived;
                traceroute.TraceComplete += Traceroute_TraceComplete;
                traceroute.MaximumHopsReached += Traceroute_MaximumHopsReached;
                traceroute.UserHasCanceled += Traceroute_UserHasCanceled;

                traceroute.TraceAsync(ipAddress, tracerouteOptions, cancellationTokenSource.Token);

                // Add the hostname or ip address to the history
                HostnameOrIPAddressHistory = new List<string>(HistoryListHelper.Modify(HostnameOrIPAddressHistory, HostnameOrIPAddress, 5));
            }
            catch (SocketException) // This will catch DNS resolve errors
            {
                IsTraceRunning = false;
                await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_Header_DnsError"] as string, Application.Current.Resources["String_CouldNotResolveHostnameMessage"] as string, MessageDialogStyle.Affirmative, dialogSettings);
            }
            catch (Exception ex) // This will catch any exception
            {
                IsTraceRunning = false;
                await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_Header_Error"] as string, ex.Message, MessageDialogStyle.Affirmative, dialogSettings);
            }
        }
        #endregion

        #region Events
        private void Traceroute_HopReceived(object sender, HopReceivedArgs e)
        {
            HopInfo tracerouteInfo = HopInfo.Parse(e);

            Application.Current.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                TraceResult.Add(tracerouteInfo);
            }));
        }

        private async void Traceroute_MaximumHopsReached(object sender, MaximumHopsReachedArgs e)
        {
            IsTraceRunning = false;
          
            await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_Header_MaximumHopsReached"] as string, string.Format(Application.Current.Resources["String_MaximumHopsReachedMessage"] as string, e.Hops), MessageDialogStyle.Affirmative, dialogSettings);
        }

        private async void Traceroute_UserHasCanceled(object sender, EventArgs e)
        {
            CancelTrace = false;
            IsTraceRunning = false;

            await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_Header_CanceledByUser"] as string, Application.Current.Resources["String_CanceledByUserMessage"] as string, MessageDialogStyle.Affirmative, dialogSettings);
        }

        private void Traceroute_TraceComplete(object sender, EventArgs e)
        {
            IsTraceRunning = false;
        }
        #endregion

        #region OnShutdown
        public void OnShutdown()
        {
            if (IsTraceRunning)
                StopTrace();
        }
        #endregion
    }
}