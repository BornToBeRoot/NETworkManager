// Documenation: https://docs.microsoft.com/en-us/windows/desktop/termserv/remote-desktop-web-connection-reference

using AxMSTSCLib;
using log4net;
using MSTSCLib;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.RemoteDesktop;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace NETworkManager.Controls;

public partial class RemoteDesktopControl : UserControlBase, IDragablzTabItem
{
    #region Variables
    private static readonly ILog Log = LogManager.GetLogger(typeof(RemoteDesktopControl));

    private bool _initialized;
    private bool _closed;

    private readonly Guid _tabId;
    private readonly RemoteDesktopSessionInfo _sessionInfo;

    private double _windowsFormsHostMaxWidth;

    public double WindowsFormsHostMaxWidth
    {
        get => _windowsFormsHostMaxWidth;
        private set
        {
            if (Math.Abs(value - _windowsFormsHostMaxWidth) < double.Epsilon)
                return;

            _windowsFormsHostMaxWidth = value;
            OnPropertyChanged();
        }
    }

    private double _windowsFormsHostMaxHeight;

    public double WindowsFormsHostMaxHeight
    {
        get => _windowsFormsHostMaxHeight;
        private set
        {
            if (Math.Abs(value - _windowsFormsHostMaxHeight) < double.Epsilon)
                return;

            _windowsFormsHostMaxHeight = value;
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
        private set
        {
            if (value == _disconnectReason)
                return;

            _disconnectReason = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Constructor, load
    public RemoteDesktopControl(Guid tabId, RemoteDesktopSessionInfo sessionInfo)
    {
        InitializeComponent();
        DataContext = this;

        ConfigurationManager.Current.RemoteDesktopTabCount++;

        _tabId = tabId;
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
        get { return new RelayCommand(_ => ReconnectAction()); }
    }

    private void ReconnectAction()
    {
        Reconnect();
    }

    public ICommand DisconnectCommand
    {
        get { return new RelayCommand(_ => DisconnectAction()); }
    }

    private void DisconnectAction()
    {
        Disconnect();
    }

    #endregion

    #region Methods

    private Tuple<int, int> GetDesktopSize()
    {
        // Get the screen size
        int desktopWidth, desktopHeight;

        if (_sessionInfo.AdjustScreenAutomatically || _sessionInfo.UseCurrentViewSize)
        {
            desktopWidth = (int)Math.Floor(RdpGrid.ActualWidth);
            desktopHeight = (int)Math.Floor(RdpGrid.ActualHeight);
        }
        else
        {
            desktopWidth = _sessionInfo.DesktopWidth;
            desktopHeight = _sessionInfo.DesktopHeight;
        }

        // Scale the screen size based on the DPI
        var scaleFactor = GetDpiScaleFactor();

        desktopWidth = desktopWidth * scaleFactor / 100;
        desktopHeight = desktopHeight * scaleFactor / 100;

        // Round the screen size to an even number
        desktopWidth = desktopWidth % 2 == 0 ? desktopWidth : desktopWidth - 1;
        desktopHeight = desktopHeight % 2 == 0 ? desktopHeight : desktopHeight - 1;

        return new Tuple<int, int>(desktopWidth, desktopHeight);
    }

    /// <summary>
    /// Connect to the remote session with the given session info.
    /// </summary>
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
        RdpClient.ColorDepth = _sessionInfo.ColorDepth; // 8, 15, 16, 24

        var desktopSize = GetDesktopSize();

        RdpClient.DesktopWidth = desktopSize.Item1;
        RdpClient.DesktopHeight = desktopSize.Item2;

        FixWindowsFormsHostSize(desktopSize.Item1, desktopSize.Item2);

        // Initial scaling before connecting
        ((IMsRdpExtendedSettings)RdpClient.GetOcx()).set_Property("DesktopScaleFactor", GetDesktopScaleFactor());
        ((IMsRdpExtendedSettings)RdpClient.GetOcx()).set_Property("DeviceScaleFactor", GetDeviceScaleFactor());

        // Authentication
        RdpClient.AdvancedSettings9.AuthenticationLevel = _sessionInfo.AuthenticationLevel;
        RdpClient.AdvancedSettings9.EnableCredSspSupport = _sessionInfo.EnableCredSspSupport;

        // Gateway server
        if (_sessionInfo.EnableGatewayServer && !string.IsNullOrEmpty(_sessionInfo.GatewayServerHostname))
        {
            RdpClient.TransportSettings2.GatewayProfileUsageMethod = (uint)GatewayProfileUsageMethod.Explicit;
            RdpClient.TransportSettings2.GatewayUsageMethod = (uint)(_sessionInfo.GatewayServerBypassLocalAddresses
                ? GatewayUsageMethod.Detect
                : GatewayUsageMethod.Direct);
            RdpClient.TransportSettings2.GatewayHostname = _sessionInfo.GatewayServerHostname;
            RdpClient.TransportSettings2.GatewayCredsSource = (uint)_sessionInfo.GatewayServerLogonMethod;
            RdpClient.TransportSettings2.GatewayCredSharing =
                _sessionInfo.GatewayServerShareCredentialsWithRemoteComputer ? 1u : 0u;

            // Credentials            
            if (_sessionInfo.UseGatewayServerCredentials && Equals(_sessionInfo.GatewayServerLogonMethod,
                    GatewayUserSelectedCredsSource.Userpass))
            {
                RdpClient.TransportSettings2.GatewayUsername = _sessionInfo.GatewayServerUsername;

                if (!string.IsNullOrEmpty(_sessionInfo.GatewayServerDomain))
                    RdpClient.TransportSettings2.GatewayDomain = _sessionInfo.GatewayServerDomain;

                RdpClient.TransportSettings2.GatewayPassword =
                    SecureStringHelper.ConvertToString(_sessionInfo.GatewayServerPassword);
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
                RdpClient.AdvancedSettings9.PerformanceFlags |=
                    RemoteDesktopPerformanceConstants.TS_PERF_DISABLE_WALLPAPER;

            if (_sessionInfo.FontSmoothing)
                RdpClient.AdvancedSettings9.PerformanceFlags |=
                    RemoteDesktopPerformanceConstants.TS_PERF_ENABLE_FONT_SMOOTHING;

            if (_sessionInfo.DesktopComposition)
                RdpClient.AdvancedSettings9.PerformanceFlags |=
                    RemoteDesktopPerformanceConstants.TS_PERF_ENABLE_DESKTOP_COMPOSITION;

            if (!_sessionInfo.ShowWindowContentsWhileDragging)
                RdpClient.AdvancedSettings9.PerformanceFlags |=
                    RemoteDesktopPerformanceConstants.TS_PERF_DISABLE_FULLWINDOWDRAG;

            if (!_sessionInfo.MenuAndWindowAnimation)
                RdpClient.AdvancedSettings9.PerformanceFlags |=
                    RemoteDesktopPerformanceConstants.TS_PERF_DISABLE_MENUANIMATIONS;

            if (!_sessionInfo.VisualStyles)
                RdpClient.AdvancedSettings9.PerformanceFlags |=
                    RemoteDesktopPerformanceConstants.TS_PERF_DISABLE_THEMING;
        }

        // Events
        RdpClient.OnConnected += RdpClient_OnConnected;
        RdpClient.OnDisconnected += RdpClient_OnDisconnected;

        // Static settings
        RdpClient.AdvancedSettings9.EnableWindowsKey = 1; // Enable window key
        RdpClient.AdvancedSettings9.allowBackgroundInput = 1; // Background input to send keystrokes like ctrl+alt+del

        // Connect
        RdpClient.Connect();
    }

    private void Reconnect()
    {
        if (IsConnected)
            return;

        IsConnecting = true;

        var desktopSize = GetDesktopSize();

        RdpClient.DesktopWidth = desktopSize.Item1;
        RdpClient.DesktopHeight = desktopSize.Item2;

        FixWindowsFormsHostSize(desktopSize.Item1, desktopSize.Item2);

        RdpClient.Connect();
    }

    public void FullScreen()
    {
        if (!IsConnected)
            return;

        RdpClient.FullScreen = true;
    }

    public async void AdjustScreen()
    {
        // Check preconditions
        if (IsConnecting)
            return;

        if (!IsConnected)
            return;

        // Wait for the control to be drawn (if window is resized or the DPI changes)
        await Task.Delay(250);

        var desktopSize = GetDesktopSize();

        // Check if we need to adjust the screen
        if (RdpClient.DesktopWidth == desktopSize.Item1 && RdpClient.DesktopHeight == desktopSize.Item2)
            return;

        if (RdpClient.Width == desktopSize.Item1 && RdpClient.Height == desktopSize.Item2)
            return;

        try
        {
            // This may fail if the RDP session was connected recently
            RdpClient.UpdateSessionDisplaySettings((uint)desktopSize.Item1, (uint)desktopSize.Item2, (uint)desktopSize.Item1, (uint)desktopSize.Item2, 0, GetDesktopScaleFactor(), GetDeviceScaleFactor());

            // Fix the size of the WindowsFormsHost and the RDP control
            FixWindowsFormsHostSize(desktopSize.Item1, desktopSize.Item2);
        }
        catch (Exception ex)
        {
            Log.Error("Error while adjusting screen", ex);
        }
    }

    /// <summary>
    ///    Fix the size of the WindowsFormsHost and the RDP control after the size
    ///    or DPI of the control has changed.
    /// </summary>
    /// <param name="width">Width of the RDP session.</param>
    /// <param name="height">Height of the RDP session.</param>
    private void FixWindowsFormsHostSize(double width, double height)
    {
        // Set the max width and height for the WindowsFormsHost
        WindowsFormsHostMaxWidth = (int)(width / GetDpiScaleFactor() * 100);
        WindowsFormsHostMaxHeight = (int)(height / GetDpiScaleFactor() * 100);

        // Update the size of the RDP control
        RdpClient.Width = (int)width;
        RdpClient.Height = (int)height;
    }

    /// <summary>
    /// Send a keystroke to the remote session.
    /// </summary>
    /// <param name="keystroke">Keystroke to send.</param>
    public void SendKey(Keystroke keystroke)
    {
        if (!IsConnected)
            return;

        var ocx = (IMsRdpClientNonScriptable)RdpClient.GetOcx();

        var info = RemoteDesktop.GetKeystroke(keystroke);

        RdpClient.Focus();

        ocx.SendKeys(info.KeyData.Length, info.ArrayKeyUp, info.KeyData);
    }

    /// <summary>
    /// Disconnect the RDP session.
    /// </summary>
    private void Disconnect()
    {
        if (!IsConnected)
            return;

        RdpClient.Disconnect();
    }

    /// <summary>
    /// Close the tab.
    /// </summary>
    public void CloseTab()
    {
        // Prevent multiple calls
        if (_closed)
            return;

        _closed = true;

        // Disconnect the session
        Disconnect();

        ConfigurationManager.Current.RemoteDesktopTabCount--;
    }

    /// <summary>
    ///     Get disconnect reason by code.
    ///     Docs:
    ///     https://social.technet.microsoft.com/wiki/contents/articles/37870.remote-desktop-client-troubleshooting-disconnect-codes-and-reasons.aspx
    /// </summary>
    /// <param name="reason">Disconnect code</param>
    /// <returns>Disconnect message</returns>
    private static string GetDisconnectReason(int reason)
    {
        return reason switch
        {
            0 => Strings.RemoteDesktopDisconnectReason_NoInfo,
            1 => Strings.RemoteDesktopDisconnectReason_LocalNotError,
            2 => Strings.RemoteDesktopDisconnectReason_RemoteByUser,
            3 => Strings.RemoteDesktopDisconnectReason_ByServer,
            4 => Strings.RemoteDesktopDisconnectReason_TotalLoginTimeLimitReached,
            260 => Strings.RemoteDesktopDisconnectReason_DNSLookupFailed,
            262 => Strings.RemoteDesktopDisconnectReason_OutOfMemory,
            264 => Strings.RemoteDesktopDisconnectReason_ConnectionTimedOut,
            516 => Strings.RemoteDesktopDisconnectReason_SocketConnectFailed,
            518 => Strings.RemoteDesktopDisconnectReason_OutOfMemory2,
            520 => Strings.RemoteDesktopDisconnectReason_HostNotFound,
            772 => Strings.RemoteDesktopDisconnectReason_WinsockSendFailed,
            774 => Strings.RemoteDesktopDisconnectReason_OutOfMemory3,
            776 => Strings.RemoteDesktopDisconnectReason_InvalidIPAddr,
            1028 => Strings.RemoteDesktopDisconnectReason_SocketRecvFailed,
            1030 => Strings.RemoteDesktopDisconnectReason_InvalidSecurityData,
            1032 => Strings.RemoteDesktopDisconnectReason_InternalError,
            1286 => Strings.RemoteDesktopDisconnectReason_InvalidEncryption,
            1288 => Strings.RemoteDesktopDisconnectReason_DNSLookupFailed2,
            1540 => Strings.RemoteDesktopDisconnectReason_GetHostByNameFailed,
            1542 => Strings.RemoteDesktopDisconnectReason_InvalidServerSecurityInfo,
            1544 => Strings.RemoteDesktopDisconnectReason_TimerError,
            1796 => Strings.RemoteDesktopDisconnectReason_TimeoutOccurred,
            1798 => Strings.RemoteDesktopDisconnectReason_ServerCertificateUnpackErr,
            2052 => Strings.RemoteDesktopDisconnectReason_InvalidIP,
            2055 => Strings.RemoteDesktopDisconnectReason_SslErrLogonFailure,
            2056 => Strings.RemoteDesktopDisconnectReason_LicensingFailed,
            2308 => Strings.RemoteDesktopDisconnectReason_AtClientWinsockFDCLOSE,
            2310 => Strings.RemoteDesktopDisconnectReason_InternalSecurityError,
            2312 => Strings.RemoteDesktopDisconnectReason_LicensingTimeout,
            2566 => Strings.RemoteDesktopDisconnectReason_InternalSecurityError2,
            2567 => Strings.RemoteDesktopDisconnectReason_SslErrNoSuchUser,
            2822 => Strings.RemoteDesktopDisconnectReason_EncryptionError,
            2823 => Strings.RemoteDesktopDisconnectReason_SslErrAccountDisabled,
            3078 => Strings.RemoteDesktopDisconnectReason_DecryptionError,
            3079 => Strings.RemoteDesktopDisconnectReason_SslErrAccountRestriction,
            3080 => Strings.RemoteDesktopDisconnectReason_ClientDecompressionError,
            3335 => Strings.RemoteDesktopDisconnectReason_SslErrAccountLockedOut,
            3591 => Strings.RemoteDesktopDisconnectReason_SslErrAccountExpired,
            3847 => Strings.RemoteDesktopDisconnectReason_SslErrPasswordExpired,
            4360 => Strings.RemoteDesktopDisconnectReason_UnableToReconnectToRemoteSession,
            4615 => Strings.RemoteDesktopDisconnectReason_SslErrPasswordMustChange,
            5639 => Strings.RemoteDesktopDisconnectReason_SslErrDelegationPolicy,
            5895 => Strings.RemoteDesktopDisconnectReason_SslErrPolicyNTLMOnly,
            6151 => Strings.RemoteDesktopDisconnectReason_SslErrNoAuthenticatingAuthority,
            6919 => Strings.RemoteDesktopDisconnectReason_SslErrCertExpired,
            7175 => Strings.RemoteDesktopDisconnectReason_SslErrSmartcardWrongPIN,
            8455 => Strings.RemoteDesktopDisconnectReason_SslErrFreshCredRequiredByServer,
            8711 => Strings.RemoteDesktopDisconnectReason_SslErrSmartcardCardBlocked,
            50331651 => Strings.RemoteDesktopDisconnectReason_50331651,
            50331653 => Strings.RemoteDesktopDisconnectReason_50331653,
            50331654 => Strings.RemoteDesktopDisconnectReason_50331654,
            50331655 => Strings.RemoteDesktopDisconnectReason_50331655,
            50331657 => Strings.RemoteDesktopDisconnectReason_50331657,
            50331658 => Strings.RemoteDesktopDisconnectReason_50331658,
            50331660 => Strings.RemoteDesktopDisconnectReason_50331660,
            50331661 => Strings.RemoteDesktopDisconnectReason_50331661,
            50331663 => Strings.RemoteDesktopDisconnectReason_50331663,
            50331672 => Strings.RemoteDesktopDisconnectReason_50331672,
            50331673 => Strings.RemoteDesktopDisconnectReason_50331673,
            50331675 => Strings.RemoteDesktopDisconnectReason_50331675,
            50331676 => Strings.RemoteDesktopDisconnectReason_50331676,
            50331679 => Strings.RemoteDesktopDisconnectReason_50331679,
            50331680 => Strings.RemoteDesktopDisconnectReason_50331680,
            50331682 => Strings.RemoteDesktopDisconnectReason_50331682,
            50331683 => Strings.RemoteDesktopDisconnectReason_50331683,
            50331684 => Strings.RemoteDesktopDisconnectReason_50331684,
            50331685 => Strings.RemoteDesktopDisconnectReason_50331685,
            50331688 => Strings.RemoteDesktopDisconnectReason_50331688,
            50331689 => Strings.RemoteDesktopDisconnectReason_50331689,
            50331690 => Strings.RemoteDesktopDisconnectReason_50331690,
            50331691 => Strings.RemoteDesktopDisconnectReason_50331691,
            50331692 => Strings.RemoteDesktopDisconnectReason_50331692,
            50331700 => Strings.RemoteDesktopDisconnectReason_50331700,
            50331701 => Strings.RemoteDesktopDisconnectReason_50331701,
            50331703 => Strings.RemoteDesktopDisconnectReason_50331703,
            50331704 => Strings.RemoteDesktopDisconnectReason_50331704,
            50331705 => Strings.RemoteDesktopDisconnectReason_50331705,
            50331707 => Strings.RemoteDesktopDisconnectReason_50331707,
            50331713 => Strings.RemoteDesktopDisconnectReason_50331713,
            _ => "Disconnect reason code " + reason + " not found in resources!" + Environment.NewLine +
                 "(Please report this on GitHub issues)"
        };
    }

    /// <summary>
    /// Get the desktop scale factor based on the DPI scale factor.
    /// Supported values are 100, 125, 150, 175, 200.
    /// See docs:
    /// https://learn.microsoft.com/en-us/windows/win32/termserv/imsrdpextendedsettings-property --> DesktopScaleFactor
    /// https://cdnweb.devolutions.net/blog/pdf/smart-resizing-and-high-dpi-issues-in-remote-desktop-manager.pdf
    /// </summary>
    /// <returns></returns>
    protected uint GetDesktopScaleFactor()
    {
        var scaleFactor = GetDpiScaleFactor();

        switch (scaleFactor)
        {
            case 125:
                return 125;
            case 150:
            case 175:
                return 150;
            case 200:
                return 200;
        }

        if (scaleFactor > 200)
            return 200;

        return 100;
    }

    /// <summary>
    /// Get the device scale factor based on the DPI scale factor.
    /// Supported values are 100, 140, 180.
    /// See docs:
    /// https://learn.microsoft.com/en-us/windows/win32/termserv/imsrdpextendedsettings-property --> DeviceScaleFactor
    /// https://cdnweb.devolutions.net/blog/pdf/smart-resizing-and-high-dpi-issues-in-remote-desktop-manager.pdf
    /// </summary>
    /// <returns>Device scale factor.</returns>
    protected uint GetDeviceScaleFactor()
    {
        var scaleFactor = GetDpiScaleFactor();

        switch (scaleFactor)
        {
            case 125:
            case 150:
            case 175:
                return 140;
            case 200:
                return 180;
        }

        if (scaleFactor > 200)
            return 180;

        return 100;
    }

    /// <summary>
    /// Get the current DPI scale factor like 100, 125, 150, 175, 200, 225, etc.
    /// </summary>
    /// <returns>Returns the DPI scale factor.</returns>
    public int GetDpiScaleFactor()
    {
        var x = System.Windows.Media.VisualTreeHelper.GetDpi(this);

        return (int)(x.PixelsPerDip * 100);
    }
    #endregion

    #region Events

    private void RdpClient_OnConnected(object sender, EventArgs e)
    {
        IsConnected = true;
        IsConnecting = false;
    }

    private void RdpClient_OnDisconnected(object sender, IMsTscAxEvents_OnDisconnectedEvent e)
    {
        IsConnected = false;
        IsConnecting = false;

        DisconnectReason = GetDisconnectReason(e.discReason);
    }
    #endregion

    public void UpdateOnWindowResize()
    {
        if (_sessionInfo.AdjustScreenAutomatically)
            AdjustScreen();
    }

    private void WindowsFormsHost_DpiChanged(object sender, DpiChangedEventArgs e)
    {
        AdjustScreen();
    }
}
