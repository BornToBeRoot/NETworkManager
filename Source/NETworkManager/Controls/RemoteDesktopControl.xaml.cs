// Documenation: https://docs.microsoft.com/en-us/windows/desktop/termserv/remote-desktop-web-connection-reference

using NETworkManager.Models.RemoteDesktop;
using System.Windows;
using System.Windows.Input;
using System;
using System.Threading.Tasks;
using NETworkManager.Utilities;

namespace NETworkManager.Controls;

public partial class RemoteDesktopControl : UserControlBase
{
    #region Variables
    private bool _initialized;

    private readonly RemoteDesktopSessionInfo _sessionInfo;

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

    private bool _isConnected;
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

    private bool _isConnecting;
    public bool IsConnecting
    {
        get => _isConnecting;
        set
        {
            if (value == _isConnecting)
                return;

            _isConnecting = value;
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
    #endregion

    #region Constructor, load
    public RemoteDesktopControl(RemoteDesktopSessionInfo sessionInfo)
    {
        InitializeComponent();
        DataContext = this;

        _sessionInfo = sessionInfo;

        Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        // Connect after the control is drawn and only on the first init
        if (_initialized)
            return;

        Connect();
        _initialized = true;
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

    public ICommand DisconnectCommand
    {
        get { return new RelayCommand(p => DisconnectAction()); }
    }

    private void DisconnectAction()
    {
        Disconnect();
    }
    #endregion

    #region Methods
    private void Connect()
    {
        IsConnecting = true;

        RdpClient.CreateControl();

        // General
        RdpClient.Server = _sessionInfo.Hostname;

        // Credentials
        if (_sessionInfo.UseCredentials)
        {
            RdpClient.UserName = _sessionInfo.Username;

            if (!string.IsNullOrEmpty(_sessionInfo.Domain))
                RdpClient.Domain = _sessionInfo.Domain;

            RdpClient.AdvancedSettings9.ClearTextPassword = SecureStringHelper.ConvertToString(_sessionInfo.Password);
        }

        // Network
        RdpClient.AdvancedSettings9.RDPPort = _sessionInfo.Port;

        // Display
        RdpClient.ColorDepth = _sessionInfo.ColorDepth;      // 8, 15, 16, 24

        if (_sessionInfo.AdjustScreenAutomatically || _sessionInfo.UseCurrentViewSize)
        {
            RdpClient.DesktopWidth = (int)RdpGrid.ActualWidth;
            RdpClient.DesktopHeight = (int)RdpGrid.ActualHeight;
        }
        else
        {
            RdpClient.DesktopWidth = _sessionInfo.DesktopWidth;
            RdpClient.DesktopHeight = _sessionInfo.DesktopHeight;
        }
        
        // Authentication
        RdpClient.AdvancedSettings9.AuthenticationLevel = _sessionInfo.AuthenticationLevel;
        RdpClient.AdvancedSettings9.EnableCredSspSupport = _sessionInfo.EnableCredSspSupport;

        // Gateway server
        if (_sessionInfo.EnableGatewayServer && !string.IsNullOrEmpty(_sessionInfo.GatewayServerHostname))
        {
            RdpClient.TransportSettings2.GatewayProfileUsageMethod = (uint)GatewayProfileUsageMethod.Explicit;
            RdpClient.TransportSettings2.GatewayUsageMethod = (uint)(_sessionInfo.GatewayServerBypassLocalAddresses ? GatewayUsageMethod.Detect : GatewayUsageMethod.Direct);
            RdpClient.TransportSettings2.GatewayHostname = _sessionInfo.GatewayServerHostname;
            RdpClient.TransportSettings2.GatewayCredsSource = (uint)_sessionInfo.GatewayServerLogonMethod;
            RdpClient.TransportSettings2.GatewayCredSharing = _sessionInfo.GatewayServerShareCredentialsWithRemoteComputer ? 1u : 0u;

            // Credentials            
            if (_sessionInfo.UseGatewayServerCredentials && Equals(_sessionInfo.GatewayServerLogonMethod, GatewayUserSelectedCredsSource.Userpass))
            {
                RdpClient.TransportSettings2.GatewayUsername = _sessionInfo.GatewayServerUsername;

                if (!string.IsNullOrEmpty(_sessionInfo.GatewayServerDomain))
                    RdpClient.TransportSettings2.GatewayDomain = _sessionInfo.GatewayServerDomain;

                RdpClient.TransportSettings2.GatewayPassword = SecureStringHelper.ConvertToString(_sessionInfo.GatewayServerPassword);
            }
        }
        else
        {
            RdpClient.TransportSettings2.GatewayProfileUsageMethod = (uint)GatewayProfileUsageMethod.Default;
            RdpClient.TransportSettings2.GatewayUsageMethod = (uint)GatewayUsageMethod.NoneDirect;
        }
        
        // Remote audio
        RdpClient.AdvancedSettings9.AudioRedirectionMode = (uint)_sessionInfo.AudioRedirectionMode;
        RdpClient.AdvancedSettings9.AudioCaptureRedirectionMode = _sessionInfo.AudioCaptureRedirectionMode == 0;
                        
        // Keyboard
        RdpClient.SecuredSettings3.KeyboardHookMode = (int)_sessionInfo.KeyboardHookMode;

        // Devices and resources
        RdpClient.AdvancedSettings9.RedirectClipboard = _sessionInfo.RedirectClipboard;
        RdpClient.AdvancedSettings9.RedirectDevices = _sessionInfo.RedirectDevices;
        RdpClient.AdvancedSettings9.RedirectDrives = _sessionInfo.RedirectDrives;
        RdpClient.AdvancedSettings9.RedirectPorts = _sessionInfo.RedirectPorts;
        RdpClient.AdvancedSettings9.RedirectSmartCards = _sessionInfo.RedirectSmartCards;
        RdpClient.AdvancedSettings9.RedirectPrinters = _sessionInfo.RedirectPrinters;

        // Performance
        RdpClient.AdvancedSettings9.BitmapPeristence = _sessionInfo.PersistentBitmapCaching ? 1 : 0;
        RdpClient.AdvancedSettings9.EnableAutoReconnect = _sessionInfo.ReconnectIfTheConnectionIsDropped;

        // Experience
        if (_sessionInfo.NetworkConnectionType != 0)
        {
            RdpClient.AdvancedSettings9.NetworkConnectionType = (uint)_sessionInfo.NetworkConnectionType;

            if (!_sessionInfo.DesktopBackground)
                RdpClient.AdvancedSettings9.PerformanceFlags |= RemoteDesktopPerformanceConstants.TS_PERF_DISABLE_WALLPAPER;

            if (_sessionInfo.FontSmoothing)
                RdpClient.AdvancedSettings9.PerformanceFlags |= RemoteDesktopPerformanceConstants.TS_PERF_ENABLE_FONT_SMOOTHING;

            if (_sessionInfo.DesktopComposition)
                RdpClient.AdvancedSettings9.PerformanceFlags |= RemoteDesktopPerformanceConstants.TS_PERF_ENABLE_DESKTOP_COMPOSITION;

            if (!_sessionInfo.ShowWindowContentsWhileDragging)
                RdpClient.AdvancedSettings9.PerformanceFlags |= RemoteDesktopPerformanceConstants.TS_PERF_DISABLE_FULLWINDOWDRAG;

            if (!_sessionInfo.MenuAndWindowAnimation)
                RdpClient.AdvancedSettings9.PerformanceFlags |= RemoteDesktopPerformanceConstants.TS_PERF_DISABLE_MENUANIMATIONS;

            if (!_sessionInfo.VisualStyles)
                RdpClient.AdvancedSettings9.PerformanceFlags |= RemoteDesktopPerformanceConstants.TS_PERF_DISABLE_THEMING;
        }
        
        // Events
        RdpClient.OnConnected += RdpClient_OnConnected;
        RdpClient.OnDisconnected += RdpClient_OnDisconnected;

        // Static settings
        RdpClient.AdvancedSettings9.EnableWindowsKey = 1;       // Enable window key
        RdpClient.AdvancedSettings9.allowBackgroundInput = 1;   // Background input to send keystrokes like ctrl+alt+del

        // Connect
        RdpClient.Connect();

        FixWindowsFormsHostSize();
    }

    private void Reconnect()
    {
        if (IsConnected)
            return;

        IsConnecting = true;
        
        // Update screen size
        if (_sessionInfo.AdjustScreenAutomatically || _sessionInfo.UseCurrentViewSize)
        {
            RdpClient.DesktopWidth = (int)RdpGrid.ActualWidth;
            RdpClient.DesktopHeight = (int)RdpGrid.ActualHeight;
        }

        RdpClient.Connect();
        
        FixWindowsFormsHostSize();       
    }

    public void FullScreen()
    {
        if (!IsConnected)
            return;

        RdpClient.FullScreen = true;
    }

    public void AdjustScreen()
    {
        if (!IsConnected)
            return;

        // Adjust screen size 
        if (_sessionInfo.AdjustScreenAutomatically || _sessionInfo.UseCurrentViewSize)
        {
            RdpClient.Reconnect((uint)RdpGrid.ActualWidth, (uint)RdpGrid.ActualHeight);
        }

        FixWindowsFormsHostSize();
    }

    private void FixWindowsFormsHostSize()
    {
        RdpClientWidth = RdpClient.DesktopWidth;
        RdpClientHeight = RdpClient.DesktopHeight;
    }

    public void SendKey(Keystroke keystroke)
    {
        if (!IsConnected)
            return;

        MSTSCLib.IMsRdpClientNonScriptable ocx = (MSTSCLib.IMsRdpClientNonScriptable)RdpClient.GetOcx();

        var info = RemoteDesktop.GetKeystroke(keystroke);

        RdpClient.Focus();

        ocx.SendKeys(info.KeyData.Length, info.ArrayKeyUp, info.KeyData);
    }

    private void Disconnect()
    {
        if (!IsConnected)
            return;

        RdpClient.Disconnect();
    }

    public void CloseTab()
    {
        Disconnect();
    }

    /// <summary>
    /// Get disconnect reason by code.
    /// Docs: https://social.technet.microsoft.com/wiki/contents/articles/37870.remote-desktop-client-troubleshooting-disconnect-codes-and-reasons.aspx
    /// </summary>
    /// <param name="reason">Disconnect code</param>
    /// <returns>Disconnect message</returns>
    private static string GetDisconnectReason(int reason)
    {
        return reason switch
        {
            0 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_NoInfo,
            1 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_LocalNotError,
            2 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_RemoteByUser,
            3 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_ByServer,
            4 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_TotalLoginTimeLimitReached,
            260 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_DNSLookupFailed,
            262 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_OutOfMemory,
            264 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_ConnectionTimedOut,
            516 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_SocketConnectFailed,
            518 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_OutOfMemory2,
            520 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_HostNotFound,
            772 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_WinsockSendFailed,
            774 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_OutOfMemory3,
            776 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_InvalidIPAddr,
            1028 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_SocketRecvFailed,
            1030 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_InvalidSecurityData,
            1032 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_InternalError,
            1286 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_InvalidEncryption,
            1288 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_DNSLookupFailed2,
            1540 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_GetHostByNameFailed,
            1542 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_InvalidServerSecurityInfo,
            1544 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_TimerError,
            1796 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_TimeoutOccurred,
            1798 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_ServerCertificateUnpackErr,
            2052 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_InvalidIP,
            2055 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_SslErrLogonFailure,
            2056 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_LicensingFailed,
            2308 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_AtClientWinsockFDCLOSE,
            2310 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_InternalSecurityError,
            2312 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_LicensingTimeout,
            2566 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_InternalSecurityError2,
            2567 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_SslErrNoSuchUser,
            2822 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_EncryptionError,
            2823 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_SslErrAccountDisabled,
            3078 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_DecryptionError,
            3079 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_SslErrAccountRestriction,
            3080 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_ClientDecompressionError,
            3335 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_SslErrAccountLockedOut,
            3591 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_SslErrAccountExpired,
            3847 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_SslErrPasswordExpired,
            4360 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_UnableToReconnectToRemoteSession,
            4615 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_SslErrPasswordMustChange,
            5639 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_SslErrDelegationPolicy,
            5895 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_SslErrPolicyNTLMOnly,
            6151 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_SslErrNoAuthenticatingAuthority,
            6919 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_SslErrCertExpired,
            7175 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_SslErrSmartcardWrongPIN,
            8455 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_SslErrFreshCredRequiredByServer,
            8711 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_SslErrSmartcardCardBlocked,
            50331651 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_50331651,
            50331653 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_50331653,
            50331654 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_50331654,
            50331655 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_50331655,
            50331657 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_50331657,
            50331658 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_50331658,
            50331660 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_50331660,            
            50331661 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_50331661,
            50331663 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_50331663,
            50331672 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_50331672,
            50331673 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_50331673,
            50331675 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_50331675,
            50331676 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_50331676,
            50331679 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_50331679,
            50331680 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_50331680,
            50331682 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_50331682,
            50331683 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_50331683,
            50331684 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_50331684,
            50331685 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_50331685,
            50331688 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_50331688,
            50331689 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_50331689,
            50331690 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_50331690,
            50331691 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_50331691,
            50331692 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_50331692,
            50331700 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_50331700,
            50331701 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_50331701,
            50331703 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_50331703,
            50331704 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_50331704,
            50331705 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_50331705,
            50331707 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_50331707,
            50331713 => Localization.Resources.Strings.RemoteDesktopDisconnectReason_50331713,
            _ => "Disconnect reason code " + reason + " not found in resources!" + Environment.NewLine + "(Please report this on GitHub issues)",
        };
    }
    #endregion

    #region Events
    private void RdpClient_OnConnected(object sender, EventArgs e)
    {
        IsConnected = true;
        IsConnecting = false;
    }

    private void RdpClient_OnDisconnected(object sender, AxMSTSCLib.IMsTscAxEvents_OnDisconnectedEvent e)
    {
        IsConnected = false;
        IsConnecting = false;

        DisconnectReason = GetDisconnectReason(e.discReason);
    }

    private void RdpGrid_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        // Resize the RDP screen size when the window size changes
        if (IsConnected && _sessionInfo.AdjustScreenAutomatically && !IsReconnecting)
            ReconnectOnSizeChanged().ConfigureAwait(false);
    }

    private async Task ReconnectOnSizeChanged()
    {
        IsReconnecting = true;

        do // Prevent to many requests
        {
            await Task.Delay(250);

        } while (Mouse.LeftButton == MouseButtonState.Pressed);

        // Reconnect with the new screen size
        RdpClient.Reconnect((uint)RdpGrid.ActualWidth, (uint)RdpGrid.ActualHeight);
        
        FixWindowsFormsHostSize();

        IsReconnecting = false;
    }
    #endregion
}
