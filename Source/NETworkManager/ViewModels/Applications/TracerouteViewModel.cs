using System.Net;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using System.Windows;
using System;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using NETworkManager.Models.Settings;
using System.Collections.Generic;
using NETworkManager.Models.Network;
using System.Threading;
using NETworkManager.Helpers;


namespace NETworkManager.ViewModels.Applications
{
    public class TracerouteViewModel : ViewModelBase
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
                    SettingsManager.Current.Traceroute_HostnameOrIPAddressHistory = value;

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
                    SettingsManager.Current.Traceroute_MaximumHops = value;

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
                    SettingsManager.Current.Traceroute_Timeout = value;

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
                    SettingsManager.Current.Traceroute_Buffer = value;

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
                    SettingsManager.Current.Traceroute_ResolveHostnamePreferIPv4 = value;

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

        private ObservableCollection<TracerouteHopInfo> _traceResult = new ObservableCollection<TracerouteHopInfo>();
        public ObservableCollection<TracerouteHopInfo> TraceResult
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
            if (SettingsManager.Current.Traceroute_HostnameOrIPAddressHistory != null)
                HostnameOrIPAddressHistory = new List<string>(SettingsManager.Current.Traceroute_HostnameOrIPAddressHistory);

            MaximumHops = SettingsManager.Current.Traceroute_MaximumHops;
            Timeout = SettingsManager.Current.Traceroute_Timeout;
            Buffer = SettingsManager.Current.Traceroute_Buffer;

            if (SettingsManager.Current.Traceroute_ResolveHostnamePreferIPv4)
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

        private void TraceAction()
        {
            if (IsTraceRunning)
                StopTrace();
            else
                StartTrace();
        }
        #endregion

        #region Methods
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
            IPAddress.TryParse(HostnameOrIPAddress, out IPAddress ipAddress);

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
                HostnameOrIPAddressHistory = new List<string>(HistoryListHelper.Modify(HostnameOrIPAddressHistory, HostnameOrIPAddress, SettingsManager.Current.Application_HistoryListEntries));
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
        private void Traceroute_HopReceived(object sender, TracerouteHopReceivedArgs e)
        {
            TracerouteHopInfo tracerouteInfo = TracerouteHopInfo.Parse(e);

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

        private async void Traceroute_UserHasCanceled(object sender, System.EventArgs e)
        {
            CancelTrace = false;
            IsTraceRunning = false;

            await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_Header_CanceledByUser"] as string, Application.Current.Resources["String_CanceledByUserMessage"] as string, MessageDialogStyle.Affirmative, dialogSettings);
        }

        private void Traceroute_TraceComplete(object sender, System.EventArgs e)
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