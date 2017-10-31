using NETworkManager.Models.RemoteDesktop;
using NETworkManager.ViewModels;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

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
        private const string RemoteDesktopDisconnectReasonIdentifier = "String_RemoteDesktopDisconnectReason_";

        private bool _hideRdpClient;
        public bool HideRdpClient
        {
            get { return _hideRdpClient; }
            set
            {
                if (value == _hideRdpClient)
                    return;

                _hideRdpClient = value;
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

        public RemoteDesktopControl(RemoteDesktopSessionInfo info)
        {
            InitializeComponent();
            DataContext = this;

            Disconnected = false;

            HideRdpClient = false;

            rdpClient.Server = info.Hostname;

            // AdvancedSettings
            rdpClient.AdvancedSettings9.AuthenticationLevel = 2;
            rdpClient.AdvancedSettings9.EnableCredSspSupport = true;
            rdpClient.AdvancedSettings9.RedirectClipboard = false;
            rdpClient.AdvancedSettings9.RedirectDevices = false;
            rdpClient.AdvancedSettings9.RedirectDrives = false;
            rdpClient.AdvancedSettings9.RedirectPorts = false;
            rdpClient.AdvancedSettings9.RedirectSmartCards = false;
            rdpClient.AdvancedSettings9.RedirectPrinters = false;

            // Display
            rdpClient.ColorDepth = 24;      // 8, 15, 16, 24
            rdpClient.DesktopHeight = 768;
            rdpClient.DesktopWidth = 1280;

            // Events
            rdpClient.OnDisconnected += RdpClient_OnDisconnected; ;

            rdpClient.Connect();
        }

        private void RdpClient_OnDisconnected(object sender, AxMSTSCLib.IMsTscAxEvents_OnDisconnectedEvent e)
        {
            HideRdpClient = true;

            DisconnectReason = GetDisconnectReason(e.discReason);
            Disconnected = true;
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
    }
}