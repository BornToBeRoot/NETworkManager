using System;
using System.Collections.Generic;
using System.Linq;
using NETworkManager.Models.RemoteDesktop;
using NETworkManager.Settings;

namespace NETworkManager.ViewModels;

public class RemoteDesktopSettingsViewModel : ViewModelBase
{
    #region Methods

    private void ChangeNetworkConnectionTypeSettings(NetworkConnectionType networkConnectionType)
    {
        switch (networkConnectionType)
        {
            case NetworkConnectionType.Modem:
                DesktopBackground = false;
                FontSmoothing = false;
                DesktopComposition = false;
                ShowWindowContentsWhileDragging = false;
                MenuAndWindowAnimation = false;
                VisualStyles = false;
                break;
            case NetworkConnectionType.BroadbandLow:
                DesktopBackground = false;
                FontSmoothing = false;
                DesktopComposition = false;
                ShowWindowContentsWhileDragging = false;
                MenuAndWindowAnimation = false;
                VisualStyles = true;
                break;
            case NetworkConnectionType.Satellite:
            case NetworkConnectionType.BroadbandHigh:
                DesktopBackground = false;
                FontSmoothing = false;
                DesktopComposition = true;
                ShowWindowContentsWhileDragging = false;
                MenuAndWindowAnimation = false;
                VisualStyles = true;
                break;
            case NetworkConnectionType.WAN:
            case NetworkConnectionType.LAN:
                DesktopBackground = true;
                FontSmoothing = true;
                DesktopComposition = true;
                ShowWindowContentsWhileDragging = true;
                MenuAndWindowAnimation = true;
                VisualStyles = true;
                break;
        }
    }

    #endregion

    #region Variables

    private readonly bool _isLoading;

    public bool AdjustScreenAutomatically
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.RemoteDesktop_AdjustScreenAutomatically = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool UseCurrentViewSize
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.RemoteDesktop_UseCurrentViewSize = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool UseFixedScreenSize
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.RemoteDesktop_UseFixedScreenSize = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public List<string> ScreenResolutions => RemoteDesktop.ScreenResolutions;

    public string SelectedScreenResolution
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
            {
                var resolution = value.Split('x');

                SettingsManager.Current.RemoteDesktop_ScreenWidth = int.Parse(resolution[0]);
                SettingsManager.Current.RemoteDesktop_ScreenHeight = int.Parse(resolution[1]);
            }

            field = value;
            OnPropertyChanged();
        }
    }

    public bool UseCustomScreenSize
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.RemoteDesktop_UseCustomScreenSize = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public string CustomScreenWidth
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.RemoteDesktop_CustomScreenWidth = int.Parse(value);

            field = value;
            OnPropertyChanged();
        }
    }

    public string CustomScreenHeight
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.RemoteDesktop_CustomScreenHeight = int.Parse(value);

            field = value;
            OnPropertyChanged();
        }
    }

    public List<int> ColorDepths => RemoteDesktop.ColorDepths;

    public int SelectedColorDepth
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.RemoteDesktop_ColorDepth = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public int Port
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.RemoteDesktop_Port = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool EnableCredSspSupport
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.RemoteDesktop_EnableCredSspSupport = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public uint AuthenticationLevel
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.RemoteDesktop_AuthenticationLevel = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool EnableGatewayServer
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.RemoteDesktop_EnableGatewayServer = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public string GatewayServerHostname
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.RemoteDesktop_GatewayServerHostname = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool GatewayServerBypassLocalAddresses
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.RemoteDesktop_GatewayServerBypassLocalAddresses = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<GatewayUserSelectedCredsSource> GatewayServerLogonMethods => Enum
        .GetValues(typeof(GatewayUserSelectedCredsSource)).Cast<GatewayUserSelectedCredsSource>();

    public GatewayUserSelectedCredsSource GatewayServerLogonMethod
    {
        get;
        set
        {
            if (Equals(value, field))
                return;

            if (!_isLoading)
                SettingsManager.Current.RemoteDesktop_GatewayServerLogonMethod = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool GatewayServerShareCredentialsWithRemoteComputer
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.RemoteDesktop_GatewayServerShareCredentialsWithRemoteComputer = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<AudioRedirectionMode> AudioRedirectionModes =>
        Enum.GetValues(typeof(AudioRedirectionMode)).Cast<AudioRedirectionMode>();

    public AudioRedirectionMode AudioRedirectionMode
    {
        get;
        set
        {
            if (Equals(value, field))
                return;

            if (!_isLoading)
                SettingsManager.Current.RemoteDesktop_AudioRedirectionMode = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<AudioCaptureRedirectionMode> AudioCaptureRedirectionModes => Enum
        .GetValues(typeof(AudioCaptureRedirectionMode)).Cast<AudioCaptureRedirectionMode>();

    public AudioCaptureRedirectionMode AudioCaptureRedirectionMode
    {
        get;
        set
        {
            if (Equals(value, field))
                return;

            if (!_isLoading)
                SettingsManager.Current.RemoteDesktop_AudioCaptureRedirectionMode = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<KeyboardHookMode> KeyboardHookModes =>
        Enum.GetValues(typeof(KeyboardHookMode)).Cast<KeyboardHookMode>();

    public KeyboardHookMode KeyboardHookMode
    {
        get;
        set
        {
            if (Equals(value, field))
                return;

            if (!_isLoading)
                SettingsManager.Current.RemoteDesktop_KeyboardHookMode = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RedirectClipboard
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.RemoteDesktop_RedirectClipboard = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RedirectDevices
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.RemoteDesktop_RedirectDevices = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RedirectDrives
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.RemoteDesktop_RedirectDrives = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RedirectPorts
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.RemoteDesktop_RedirectPorts = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RedirectSmartCards
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.RemoteDesktop_RedirectSmartCards = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RedirectPrinters
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.RemoteDesktop_RedirectPrinters = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool PersistentBitmapCaching
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.RemoteDesktop_PersistentBitmapCaching = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool ReconnectIfTheConnectionIsDropped
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.RemoteDesktop_ReconnectIfTheConnectionIsDropped = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<NetworkConnectionType> NetworkConnectionTypes =>
        Enum.GetValues(typeof(NetworkConnectionType)).Cast<NetworkConnectionType>();

    public NetworkConnectionType NetworkConnectionType
    {
        get;
        set
        {
            if (Equals(value, field))
                return;

            if (!_isLoading)
            {
                ChangeNetworkConnectionTypeSettings(value);
                SettingsManager.Current.RemoteDesktop_NetworkConnectionType = value;
            }

            field = value;
            OnPropertyChanged();
        }
    }

    public bool DesktopBackground
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.RemoteDesktop_DesktopBackground = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool FontSmoothing
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.RemoteDesktop_FontSmoothing = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool DesktopComposition
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.RemoteDesktop_DesktopComposition = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool ShowWindowContentsWhileDragging
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.RemoteDesktop_ShowWindowContentsWhileDragging = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool MenuAndWindowAnimation
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.RemoteDesktop_MenuAndWindowAnimation = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool VisualStyles
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.RemoteDesktop_VisualStyles = value;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Constructor, load settings

    public RemoteDesktopSettingsViewModel()
    {
        _isLoading = true;

        LoadSettings();

        _isLoading = false;
    }

    private void LoadSettings()
    {
        AdjustScreenAutomatically = SettingsManager.Current.RemoteDesktop_AdjustScreenAutomatically;
        UseCurrentViewSize = SettingsManager.Current.RemoteDesktop_UseCurrentViewSize;
        UseFixedScreenSize = SettingsManager.Current.RemoteDesktop_UseFixedScreenSize;
        SelectedScreenResolution = ScreenResolutions.FirstOrDefault(x =>
            x ==
            $"{SettingsManager.Current.RemoteDesktop_ScreenWidth}x{SettingsManager.Current.RemoteDesktop_ScreenHeight}");
        UseCustomScreenSize = SettingsManager.Current.RemoteDesktop_UseCustomScreenSize;
        CustomScreenWidth = SettingsManager.Current.RemoteDesktop_CustomScreenWidth.ToString();
        CustomScreenHeight = SettingsManager.Current.RemoteDesktop_CustomScreenHeight.ToString();
        SelectedColorDepth = ColorDepths.FirstOrDefault(x => x == SettingsManager.Current.RemoteDesktop_ColorDepth);
        Port = SettingsManager.Current.RemoteDesktop_Port;
        EnableCredSspSupport = SettingsManager.Current.RemoteDesktop_EnableCredSspSupport;
        AuthenticationLevel = SettingsManager.Current.RemoteDesktop_AuthenticationLevel;
        EnableGatewayServer = SettingsManager.Current.RemoteDesktop_EnableGatewayServer;
        GatewayServerHostname = SettingsManager.Current.RemoteDesktop_GatewayServerHostname;
        GatewayServerBypassLocalAddresses = SettingsManager.Current.RemoteDesktop_GatewayServerBypassLocalAddresses;
        GatewayServerLogonMethod =
            GatewayServerLogonMethods.FirstOrDefault(x =>
                x == SettingsManager.Current.RemoteDesktop_GatewayServerLogonMethod);
        GatewayServerShareCredentialsWithRemoteComputer =
            SettingsManager.Current.RemoteDesktop_GatewayServerShareCredentialsWithRemoteComputer;
        AudioRedirectionMode =
            AudioRedirectionModes.FirstOrDefault(x => x == SettingsManager.Current.RemoteDesktop_AudioRedirectionMode);
        AudioCaptureRedirectionMode = AudioCaptureRedirectionModes.FirstOrDefault(x =>
            x == SettingsManager.Current.RemoteDesktop_AudioCaptureRedirectionMode);
        KeyboardHookMode =
            KeyboardHookModes.FirstOrDefault(x => x == SettingsManager.Current.RemoteDesktop_KeyboardHookMode);
        RedirectClipboard = SettingsManager.Current.RemoteDesktop_RedirectClipboard;
        RedirectDevices = SettingsManager.Current.RemoteDesktop_RedirectDevices;
        RedirectDrives = SettingsManager.Current.RemoteDesktop_RedirectDrives;
        RedirectPorts = SettingsManager.Current.RemoteDesktop_RedirectPorts;
        RedirectSmartCards = SettingsManager.Current.RemoteDesktop_RedirectSmartCards;
        RedirectPrinters = SettingsManager.Current.RemoteDesktop_RedirectPrinters;
        PersistentBitmapCaching = SettingsManager.Current.RemoteDesktop_PersistentBitmapCaching;
        ReconnectIfTheConnectionIsDropped = SettingsManager.Current.RemoteDesktop_ReconnectIfTheConnectionIsDropped;
        NetworkConnectionType =
            NetworkConnectionTypes.FirstOrDefault(x =>
                x == SettingsManager.Current.RemoteDesktop_NetworkConnectionType);
        DesktopBackground = SettingsManager.Current.RemoteDesktop_DesktopBackground;
        FontSmoothing = SettingsManager.Current.RemoteDesktop_FontSmoothing;
        DesktopComposition = SettingsManager.Current.RemoteDesktop_DesktopComposition;
        ShowWindowContentsWhileDragging = SettingsManager.Current.RemoteDesktop_ShowWindowContentsWhileDragging;
        MenuAndWindowAnimation = SettingsManager.Current.RemoteDesktop_MenuAndWindowAnimation;
        VisualStyles = SettingsManager.Current.RemoteDesktop_VisualStyles;
    }

    #endregion
}