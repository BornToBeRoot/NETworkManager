using NETworkManager.Models.RemoteDesktop;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System;
using System.Windows.Threading;
using NETworkManager.Utilities;
using NETworkManager.Models.Settings;

namespace NETworkManager.Controls
{
    public partial class RemoteDesktopControl : INotifyPropertyChanged
    {
        #region PropertyChangedEventHandler
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Variables
        private bool _initialized;

        private const string RemoteDesktopDisconnectReasonIdentifier = "String_RemoteDesktopDisconnectReason_";

        private readonly RemoteDesktopSessionInfo _rdpProfileInfo;

        private readonly DispatcherTimer _reconnectAdjustScreenTimer = new DispatcherTimer();

        // Fix WindowsFormsHost width
        private double _rdpClientWidth;
        public double RdpClientWidth
        {
            get => _rdpClientWidth;
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
        public double RdpClientHeight
        {
            get => _rdpClientHeight;
            set
            {
                if (value == _rdpClientHeight)
                    return;

                _rdpClientHeight = value;
                OnPropertyChanged();
            }
        }

        private bool _isConnected = true;
        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                if (value == _isConnected)
                    return;

                _isConnected = value;
                OnPropertyChanged();
            }
        }

        private bool _isReconnecting;
        public bool IsReconnecting
        {
            get => _isReconnecting;
            set
            {
                if (value == _isReconnecting)
                    return;

                _isReconnecting = value;
                OnPropertyChanged();
            }
        }

        private string _disconnectReason;
        public string DisconnectReason
        {
            get => _disconnectReason;
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

            _rdpProfileInfo = info;

            _reconnectAdjustScreenTimer.Tick += ReconnectAdjustScreenTimer_Tick;
            _reconnectAdjustScreenTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);

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
            CloseTab();
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
            RdpClient.Server = _rdpProfileInfo.Hostname;
            RdpClient.AdvancedSettings9.RDPPort = _rdpProfileInfo.Port;

            if (_rdpProfileInfo.CustomCredentials)
            {
                RdpClient.UserName = _rdpProfileInfo.Username;
                RdpClient.AdvancedSettings9.ClearTextPassword = SecureStringHelper.ConvertToString(_rdpProfileInfo.Password);
            }

            // AdvancedSettings
            RdpClient.AdvancedSettings9.AuthenticationLevel = _rdpProfileInfo.AuthenticationLevel;
            RdpClient.AdvancedSettings9.EnableCredSspSupport = _rdpProfileInfo.EnableCredSspSupport;

            // Devices and resources
            RdpClient.AdvancedSettings9.RedirectClipboard = _rdpProfileInfo.RedirectClipboard;
            RdpClient.AdvancedSettings9.RedirectDevices = _rdpProfileInfo.RedirectDevices;
            RdpClient.AdvancedSettings9.RedirectDrives = _rdpProfileInfo.RedirectDrives;
            RdpClient.AdvancedSettings9.RedirectPorts = _rdpProfileInfo.RedirectPorts;
            RdpClient.AdvancedSettings9.RedirectSmartCards = _rdpProfileInfo.RedirectSmartCards;
            RdpClient.AdvancedSettings9.RedirectPrinters = _rdpProfileInfo.RedirectPrinters;

            // Display
            RdpClient.ColorDepth = _rdpProfileInfo.ColorDepth;      // 8, 15, 16, 24

            if (_rdpProfileInfo.AdjustScreenAutomatically || _rdpProfileInfo.UseCurrentViewSize)
            {
                RdpClient.DesktopWidth = (int)RdpGrid.ActualWidth;
                RdpClient.DesktopHeight = (int)RdpGrid.ActualHeight;
            }
            else
            {
                RdpClient.DesktopWidth = _rdpProfileInfo.DesktopWidth;
                RdpClient.DesktopHeight = _rdpProfileInfo.DesktopHeight;
            }

            FixWindowsFormsHostSize();

            // Events
            RdpClient.OnConnected += RdpClient_OnConnected;
            RdpClient.OnDisconnected += RdpClient_OnDisconnected;

            RdpClient.Connect();
        }

        private void Reconnect()
        {
            IsReconnecting = true;

            if (_rdpProfileInfo.AdjustScreenAutomatically)
            {
                RdpClient.DesktopWidth = (int)RdpGrid.ActualWidth;
                RdpClient.DesktopHeight = (int)RdpGrid.ActualHeight;
            }

            FixWindowsFormsHostSize();

            RdpClient.Connect();
        }

        private void ReconnectAdjustScreen()
        {
            RdpClient.Reconnect((uint)RdpGrid.ActualWidth, (uint)RdpGrid.ActualHeight);
            FixWindowsFormsHostSize();
        }

        private void FixWindowsFormsHostSize()
        {
            RdpClientWidth = RdpClient.DesktopWidth;
            RdpClientHeight = RdpClient.DesktopHeight;
        }

        private void Disconnect()
        {
            if (IsConnected)
                RdpClient.Disconnect();
        }

        public void CloseTab()
        {
            Disconnect();
        }

        // Source: https://msdn.microsoft.com/en-us/library/aa382170(v=vs.85).aspx
        private static string GetDisconnectReasonFromResource(string reason)
        {
            try
            {
                return LocalizationManager.GetStringByKey(RemoteDesktopDisconnectReasonIdentifier + reason);
            }
            catch (NullReferenceException ex) // This happens when the application gets closed and the resources have already been released
            {
                return ex.Message; // The user should never see that message
            }
        }

        private static string GetDisconnectReason(int reason)
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
            IsConnected = true;
        }

        private void RdpClient_OnDisconnected(object sender, AxMSTSCLib.IMsTscAxEvents_OnDisconnectedEvent e)
        {
            IsConnected = false;
            IsReconnecting = false;

            DisconnectReason = GetDisconnectReason(e.discReason);
        }

        private void RdpGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Prevent with a timer, that the function (rdpClient.Reconnect()) is executed too often
            if (IsConnected && _rdpProfileInfo.AdjustScreenAutomatically)
                _reconnectAdjustScreenTimer.Start();
        }

        private void ReconnectAdjustScreenTimer_Tick(object sender, EventArgs e)
        {
            // Stop timer
            _reconnectAdjustScreenTimer.Stop();

            // Reconnect with new resulution
            ReconnectAdjustScreen();
        }
        #endregion
    }
}