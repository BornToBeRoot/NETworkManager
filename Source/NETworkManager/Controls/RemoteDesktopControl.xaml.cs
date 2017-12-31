using NETworkManager.Models.RemoteDesktop;
using NETworkManager.ViewModels;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System;
using System.Windows.Threading;
using NETworkManager.Helpers;

namespace NETworkManager.Controls
{

    public partial class RemoteDesktopControl : UserControl, INotifyPropertyChanged
    {
        #region PropertyChangedEventHandler
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Variables
        private bool _initialized = false;

        private const string RemoteDesktopDisconnectReasonIdentifier = "String_RemoteDesktopDisconnectReason_";

        private RemoteDesktopSessionInfo _rdpSessionInfo;

        DispatcherTimer reconnectAdjustScreenTimer = new DispatcherTimer();

        // Fix WindowsFormsHost width
        private double _rdpClientWidth;
        public double RDPClientWidth
        {
            get { return _rdpClientWidth; }
            set
            {
                if (value == _rdpClientWidth)
                    return;

                _rdpClientWidth = value;
                OnPropertyChanged();
            }
        }

        // Fix WindowsFormsHost height
        private double _rdpClientHeight;
        public double RDPClientHeight
        {
            get { return _rdpClientHeight; }
            set
            {
                if (value == _rdpClientHeight)
                    return;

                _rdpClientHeight = value;
                OnPropertyChanged();
            }
        }

        private bool _connected;
        public bool Connected
        {
            get { return _connected; }
            set
            {
                if (value == _connected)
                    return;

                _connected = value;
                OnPropertyChanged();
            }
        }

        private bool _disconnected;
        public bool Disconnected
        {
            get { return _disconnected; }
            set
            {
                if (value == _disconnected)
                    return;

                _disconnected = value;
                OnPropertyChanged();
            }
        }

        private string _disconnectReason;
        public string DisconnectReason
        {
            get { return _disconnectReason; }
            set
            {
                if (value == _disconnectReason)
                    return;

                _disconnectReason = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor, load
        public RemoteDesktopControl(RemoteDesktopSessionInfo info)
        {
            InitializeComponent();
            DataContext = this;

            _rdpSessionInfo = info;

            reconnectAdjustScreenTimer.Tick += ReconnectAdjustScreenTimer_Tick;
            reconnectAdjustScreenTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);

            Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Connect after the control is drawn and only on the first init
            if (!_initialized)
            {
                Connect();
                _initialized = true;
            }
        }

        private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
        {
            OnClose();
        }
        #endregion

        #region ICommands & Actions
        public ICommand ReconnectCommand
        {
            get { return new RelayCommand(p => ReconnectAction()); }
        }

        private void ReconnectAction()
        {
            Reconnect();
        }
        #endregion

        #region Methods
        private void Connect()
        {
            rdpClient.Server = _rdpSessionInfo.Hostname;

            if (_rdpSessionInfo.CustomCredentials)
            {
                rdpClient.UserName = _rdpSessionInfo.Username;
                rdpClient.AdvancedSettings9.ClearTextPassword = SecureStringHelper.ConvertToString(_rdpSessionInfo.Password);
            }

            // AdvancedSettings
            rdpClient.AdvancedSettings9.AuthenticationLevel = _rdpSessionInfo.AuthenticationLevel;
            rdpClient.AdvancedSettings9.EnableCredSspSupport = _rdpSessionInfo.EnableCredSspSupport;

            // Devices and resources
            rdpClient.AdvancedSettings9.RedirectClipboard = _rdpSessionInfo.RedirectClipboard;
            rdpClient.AdvancedSettings9.RedirectDevices = _rdpSessionInfo.RedirectDevices;
            rdpClient.AdvancedSettings9.RedirectDrives = _rdpSessionInfo.RedirectDrives;
            rdpClient.AdvancedSettings9.RedirectPorts = _rdpSessionInfo.RedirectPorts;
            rdpClient.AdvancedSettings9.RedirectSmartCards = _rdpSessionInfo.RedirectSmartCards;
            rdpClient.AdvancedSettings9.RedirectPrinters = _rdpSessionInfo.RedirectPrinters;

            // Display
            rdpClient.ColorDepth = _rdpSessionInfo.ColorDepth;      // 8, 15, 16, 24

            if (_rdpSessionInfo.AdjustScreenAutomatically)
            {
                rdpClient.DesktopWidth = (int)rdpGrid.ActualWidth;
                rdpClient.DesktopHeight = (int)rdpGrid.ActualHeight;
            }
            else
            {
                rdpClient.DesktopWidth = _rdpSessionInfo.DesktopWidth;
                rdpClient.DesktopHeight = _rdpSessionInfo.DesktopHeight;
            }

            FixWindowsFormsHostSize();

            // Events
            rdpClient.OnConnected += RdpClient_OnConnected;
            rdpClient.OnDisconnected += RdpClient_OnDisconnected;

            rdpClient.Connect();
        }
               
        private void Reconnect()
        {
            if (_rdpSessionInfo.AdjustScreenAutomatically)
            {
                rdpClient.DesktopWidth = (int)rdpGrid.ActualWidth;
                rdpClient.DesktopHeight = (int)rdpGrid.ActualHeight;
            }

            FixWindowsFormsHostSize();

            rdpClient.Connect();
        }

        private void Disconnect()
        {
            rdpClient.Disconnect();
        }

        public void OnClose()
        {
            if (Connected)
                Disconnect();
        }

        private void ReconnectAdjustScreen()
        {
            rdpClient.Reconnect((uint)rdpGrid.ActualWidth, (uint)rdpGrid.ActualHeight);
            FixWindowsFormsHostSize();
        }

        private void FixWindowsFormsHostSize()
        {
            RDPClientWidth = rdpClient.DesktopWidth;
            RDPClientHeight = rdpClient.DesktopHeight;
        }

        // Source: https://msdn.microsoft.com/en-us/library/aa382170(v=vs.85).aspx
        private string GetDisconnectReasonFromResource(string reason)
        {
            return Application.Current.Resources[RemoteDesktopDisconnectReasonIdentifier + reason] as string;
        }

        private string GetDisconnectReason(int reason)
        {
            switch (reason)
            {
                case 0:
                    return GetDisconnectReasonFromResource("NoInfo");
                case 1:
                    return GetDisconnectReasonFromResource("LocalNotError");
                case 2:
                    return GetDisconnectReasonFromResource("RemoteByUser");
                case 3:
                    return GetDisconnectReasonFromResource("ByServer");
                case 260:
                    return GetDisconnectReasonFromResource("DNSLookupFailed");
                case 262:
                    return GetDisconnectReasonFromResource("OutOfMemory");
                case 264:
                    return GetDisconnectReasonFromResource("ConnectionTimedOut");
                case 516:
                    return GetDisconnectReasonFromResource("SocketConnectFailed");
                case 518:
                    return GetDisconnectReasonFromResource("OutOfMemory2");
                case 520:
                    return GetDisconnectReasonFromResource("HostNotFound");
                case 772:
                    return GetDisconnectReasonFromResource("WinsockSendFailed");
                case 774:
                    return GetDisconnectReasonFromResource("OutOfMemory3");
                case 776:
                    return GetDisconnectReasonFromResource("InvalidIPAddr");
                case 1028:
                    return GetDisconnectReasonFromResource("SocketRecvFailed");
                case 1030:
                    return GetDisconnectReasonFromResource("InvalidSecurityData");
                case 1032:
                    return GetDisconnectReasonFromResource("InternalError");
                case 1286:
                    return GetDisconnectReasonFromResource("InvalidEncryption");
                case 1288:
                    return GetDisconnectReasonFromResource("DNSLookupFailed2");
                case 1540:
                    return GetDisconnectReasonFromResource("GetHostByNameFailed");
                case 1542:
                    return GetDisconnectReasonFromResource("InvalidServerSecurityInfo");
                case 1544:
                    return GetDisconnectReasonFromResource("TimerError");
                case 1796:
                    return GetDisconnectReasonFromResource("TimeoutOccurred");
                case 1798:
                    return GetDisconnectReasonFromResource("ServerCertificateUnpackErr");
                case 2052:
                    return GetDisconnectReasonFromResource("InvalidIP");
                case 2055:
                    return GetDisconnectReasonFromResource("SslErrLogonFailure");
                case 2056:
                    return GetDisconnectReasonFromResource("LicensingFailed");
                case 2308:
                    return GetDisconnectReasonFromResource("AtClientWinsockFDCLOSE");
                case 2310:
                    return GetDisconnectReasonFromResource("InternalSecurityError");
                case 2312:
                    return GetDisconnectReasonFromResource("LicensingTimeout");
                case 2566:
                    return GetDisconnectReasonFromResource("InternalSecurityError2");
                case 2567:
                    return GetDisconnectReasonFromResource("SslErrNoSuchUser");
                case 2822:
                    return GetDisconnectReasonFromResource("EncryptionError");
                case 2823:
                    return GetDisconnectReasonFromResource("SslErrAccountDisabled");
                case 3078:
                    return GetDisconnectReasonFromResource("DecryptionError");
                case 3079:
                    return GetDisconnectReasonFromResource("SslErrAccountRestriction");
                case 3080:
                    return GetDisconnectReasonFromResource("ClientDecompressionError");
                case 3335:
                    return GetDisconnectReasonFromResource("SslErrAccountLockedOut");
                case 3591:
                    return GetDisconnectReasonFromResource("SslErrAccountExpired");
                case 3847:
                    return GetDisconnectReasonFromResource("SslErrPasswordExpired");
                case 4615:
                    return GetDisconnectReasonFromResource("SslErrPasswordMustChange");
                case 5639:
                    return GetDisconnectReasonFromResource("SslErrDelegationPolicy");
                case 5895:
                    return GetDisconnectReasonFromResource("SslErrPolicyNTLMOnly");
                case 6151:
                    return GetDisconnectReasonFromResource("SslErrNoAuthenticatingAuthority");
                case 6919:
                    return GetDisconnectReasonFromResource("SslErrCertExpired");
                case 7175:
                    return GetDisconnectReasonFromResource("SslErrSmartcardWrongPIN");
                case 8455:
                    return GetDisconnectReasonFromResource("SslErrFreshCredRequiredByServer");
                case 8711:
                    return GetDisconnectReasonFromResource("SslErrSmartcardCardBlocked");
                default:
                    return "reason not found!";
            }
        }
        #endregion

        #region Events
        private void RdpClient_OnConnected(object sender, EventArgs e)
        {
            Disconnected = false;
            Connected = true;
        }
        
        private void RdpClient_OnDisconnected(object sender, AxMSTSCLib.IMsTscAxEvents_OnDisconnectedEvent e)
        {
            Connected = false;
            Disconnected = true;

            DisconnectReason = GetDisconnectReason(e.discReason);
        }

        private void RDPGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Prevent with a timer, that the function (rdpClient.Reconnect()) is executed too often
            if (Connected && _rdpSessionInfo.AdjustScreenAutomatically)
                reconnectAdjustScreenTimer.Start();
        }

        private void ReconnectAdjustScreenTimer_Tick(object sender, EventArgs e)
        {
            // Stop timer
            reconnectAdjustScreenTimer.Stop();

            // Reconnect with new resulution
            ReconnectAdjustScreen();
        }
        #endregion
    }
}