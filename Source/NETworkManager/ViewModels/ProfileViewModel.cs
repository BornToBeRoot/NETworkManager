using NETworkManager.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models;
using NETworkManager.Models.Network;
using NETworkManager.Models.PowerShell;
using NETworkManager.Models.PuTTY;
using NETworkManager.Models.RemoteDesktop;
using NETworkManager.Profiles;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

// ReSharper disable InconsistentNaming

namespace NETworkManager.ViewModels;

public class ProfileViewModel : ViewModelBase
{
    #region Constructor

    public ProfileViewModel(Action<ProfileViewModel> saveCommand, Action<ProfileViewModel> cancelHandler,
        IReadOnlyCollection<string> groups, string group = null, ProfileEditMode editMode = ProfileEditMode.Add,
        ProfileInfo profile = null, ApplicationName applicationName = ApplicationName.None)
    {
        // Load the view
        ProfileViews = new CollectionViewSource { Source = ProfileViewManager.List }.View;
        ProfileViews.SortDescriptions.Add(
            new SortDescription(nameof(ProfileViewInfo.Name), ListSortDirection.Ascending));

        SaveCommand = new RelayCommand(_ => saveCommand(this));
        CancelCommand = new RelayCommand(_ => cancelHandler(this));

        var profileInfo = profile ?? new ProfileInfo();

        Name = profileInfo.Name;

        if (editMode == ProfileEditMode.Copy)
            Name += " - " + Strings.CopyNoun;

        Host = profileInfo.Host;

        Description = profileInfo.Description;

        // Try to get group (name) as parameter, then from profile, then the first in the list of groups, then the default group            
        Group = group ?? (string.IsNullOrEmpty(profileInfo.Group)
            ? groups.Count > 0 ? groups.OrderBy(x => x).First() : Strings.Default
            : profileInfo.Group);

        Groups = CollectionViewSource.GetDefaultView(groups);
        Groups.SortDescriptions.Add(new SortDescription());

        TagsCollection = new ObservableSetCollection<string>(profileInfo.TagsCollection);

        Tags = CollectionViewSource.GetDefaultView(TagsCollection);
        Tags.SortDescriptions.Add(new SortDescription("", ListSortDirection.Ascending));

        // Network Interface
        NetworkInterface_Enabled = editMode == ProfileEditMode.Add
            ? applicationName == ApplicationName.NetworkInterface
            : profileInfo.NetworkInterface_Enabled;
        NetworkInterface_EnableDynamicIPAddress = !profileInfo.NetworkInterface_EnableStaticIPAddress;
        NetworkInterface_EnableStaticIPAddress = profileInfo.NetworkInterface_EnableStaticIPAddress;
        NetworkInterface_IPAddress = profileInfo.NetworkInterface_IPAddress;
        NetworkInterface_Gateway = profileInfo.NetworkInterface_Gateway;
        NetworkInterface_Subnetmask = profileInfo.NetworkInterface_Subnetmask;
        NetworkInterface_EnableDynamicDNS = !profileInfo.NetworkInterface_EnableStaticDNS;
        NetworkInterface_EnableStaticDNS = profileInfo.NetworkInterface_EnableStaticDNS;
        NetworkInterface_PrimaryDNSServer = profileInfo.NetworkInterface_PrimaryDNSServer;
        NetworkInterface_SecondaryDNSServer = profileInfo.NetworkInterface_SecondaryDNSServer;

        // IP Scanner
        IPScanner_Enabled = editMode == ProfileEditMode.Add
            ? applicationName == ApplicationName.IPScanner
            : profileInfo.IPScanner_Enabled;
        IPScanner_InheritHost = profileInfo.IPScanner_InheritHost;
        IPScanner_HostOrIPRange = profileInfo.IPScanner_HostOrIPRange;

        // Port Scanner
        PortScanner_Enabled = editMode == ProfileEditMode.Add
            ? applicationName == ApplicationName.PortScanner
            : profileInfo.PortScanner_Enabled;
        PortScanner_InheritHost = profileInfo.PortScanner_InheritHost;
        PortScanner_Host = profileInfo.PortScanner_Host;
        PortScanner_Ports = profileInfo.PortScanner_Ports;

        // Ping Monitor
        PingMonitor_Enabled = editMode == ProfileEditMode.Add
            ? applicationName == ApplicationName.PingMonitor
            : profileInfo.PingMonitor_Enabled;
        PingMonitor_InheritHost = profileInfo.PingMonitor_InheritHost;
        PingMonitor_Host = profileInfo.PingMonitor_Host;

        // Traceroute
        Traceroute_Enabled = editMode == ProfileEditMode.Add
            ? applicationName == ApplicationName.Traceroute
            : profileInfo.Traceroute_Enabled;
        Traceroute_InheritHost = profileInfo.Traceroute_InheritHost;
        Traceroute_Host = profileInfo.Traceroute_Host;

        // DNS Lookup
        DNSLookup_Enabled = editMode == ProfileEditMode.Add
            ? applicationName == ApplicationName.DNSLookup
            : profileInfo.DNSLookup_Enabled;
        DNSLookup_InheritHost = profileInfo.DNSLookup_InheritHost;
        DNSLookup_Host = profileInfo.DNSLookup_Host;

        // Remote Desktop
        RemoteDesktop_Enabled = editMode == ProfileEditMode.Add
            ? applicationName == ApplicationName.RemoteDesktop
            : profileInfo.RemoteDesktop_Enabled;
        RemoteDesktop_InheritHost = profileInfo.RemoteDesktop_InheritHost;
        RemoteDesktop_Host = profileInfo.RemoteDesktop_Host;
        RemoteDesktop_UseCredentials = profileInfo.RemoteDesktop_UseCredentials;
        RemoteDesktop_Username = profileInfo.RemoteDesktop_Username;
        RemoteDesktop_Domain = profileInfo.RemoteDesktop_Domain;
        RemoteDesktop_Password = profileInfo.RemoteDesktop_Password;
        RemoteDesktop_AdminSession = profileInfo.RemoteDesktop_AdminSession;
        RemoteDesktop_OverrideDisplay = profileInfo.RemoteDesktop_OverrideDisplay;
        RemoteDesktop_AdjustScreenAutomatically = profileInfo.RemoteDesktop_AdjustScreenAutomatically;
        RemoteDesktop_UseCurrentViewSize = profileInfo.RemoteDesktop_UseCurrentViewSize;
        RemoteDesktop_UseFixedScreenSize = profileInfo.RemoteDesktop_UseFixedScreenSize;
        RemoteDesktop_SelectedScreenResolution = RemoteDesktop_ScreenResolutions.FirstOrDefault(x =>
            x == $"{profileInfo.RemoteDesktop_ScreenWidth}x{profileInfo.RemoteDesktop_ScreenHeight}");
        RemoteDesktop_UseCustomScreenSize = profileInfo.RemoteDesktop_UseCustomScreenSize;
        RemoteDesktop_CustomScreenWidth = profileInfo.RemoteDesktop_CustomScreenWidth.ToString();
        RemoteDesktop_CustomScreenHeight = profileInfo.RemoteDesktop_CustomScreenHeight.ToString();
        RemoteDesktop_OverrideColorDepth = profileInfo.RemoteDesktop_OverrideColorDepth;
        RemoteDesktop_SelectedColorDepth =
            RemoteDesktop_ColorDepths.FirstOrDefault(x => x == profileInfo.RemoteDesktop_ColorDepth);
        RemoteDesktop_OverridePort = profileInfo.RemoteDesktop_OverridePort;
        RemoteDesktop_Port = profileInfo.RemoteDesktop_Port;
        RemoteDesktop_OverrideCredSspSupport = profileInfo.RemoteDesktop_OverrideCredSspSupport;
        RemoteDesktop_EnableCredSspSupport = profileInfo.RemoteDesktop_EnableCredSspSupport;
        RemoteDesktop_OverrideAuthenticationLevel = profileInfo.RemoteDesktop_OverrideAuthenticationLevel;
        RemoteDesktop_AuthenticationLevel = profileInfo.RemoteDesktop_AuthenticationLevel;
        RemoteDesktop_OverrideGatewayServer = profileInfo.RemoteDesktop_OverrideGatewayServer;
        RemoteDesktop_EnableGatewayServer = profileInfo.RemoteDesktop_EnableGatewayServer;
        RemoteDesktop_GatewayServerHostname = profileInfo.RemoteDesktop_GatewayServerHostname;
        RemoteDesktop_GatewayServerBypassLocalAddresses = profileInfo.RemoteDesktop_GatewayServerBypassLocalAddresses;
        RemoteDesktop_GatewayServerLogonMethod = profileInfo.RemoteDesktop_GatewayServerLogonMethod;
        RemoteDesktop_GatewayServerShareCredentialsWithRemoteComputer =
            profileInfo.RemoteDesktop_GatewayServerShareCredentialsWithRemoteComputer;
        RemoteDesktop_UseGatewayServerCredentials = profileInfo.RemoteDesktop_UseGatewayServerCredentials;
        RemoteDesktop_GatewayServerUsername = profileInfo.RemoteDesktop_GatewayServerUsername;
        RemoteDesktop_GatewayServerDomain = profileInfo.RemoteDesktop_GatewayServerDomain;
        RemoteDesktop_GatewayServerPassword = profileInfo.RemoteDesktop_GatewayServerPassword;
        RemoteDesktop_OverrideAudioRedirectionMode = profileInfo.RemoteDesktop_OverrideAudioRedirectionMode;
        RemoteDesktop_AudioRedirectionMode =
            RemoteDesktop_AudioRedirectionModes.FirstOrDefault(x =>
                x == profileInfo.RemoteDesktop_AudioRedirectionMode);
        RemoteDesktop_OverrideAudioCaptureRedirectionMode =
            profileInfo.RemoteDesktop_OverrideAudioCaptureRedirectionMode;
        RemoteDesktop_AudioCaptureRedirectionMode =
            RemoteDesktop_AudioCaptureRedirectionModes.FirstOrDefault(x =>
                x == profileInfo.RemoteDesktop_AudioCaptureRedirectionMode);
        RemoteDesktop_OverrideApplyWindowsKeyCombinations =
            profileInfo.RemoteDesktop_OverrideApplyWindowsKeyCombinations;
        RemoteDesktop_KeyboardHookMode =
            RemoteDesktop_KeyboardHookModes.FirstOrDefault(x => x == profileInfo.RemoteDesktop_KeyboardHookMode);
        RemoteDesktop_OverrideRedirectClipboard = profileInfo.RemoteDesktop_OverrideRedirectClipboard;
        RemoteDesktop_RedirectClipboard = profileInfo.RemoteDesktop_RedirectClipboard;
        RemoteDesktop_OverrideRedirectDevices = profileInfo.RemoteDesktop_OverrideRedirectDevices;
        RemoteDesktop_RedirectDevices = profileInfo.RemoteDesktop_RedirectDevices;
        RemoteDesktop_OverrideRedirectDrives = profileInfo.RemoteDesktop_OverrideRedirectDrives;
        RemoteDesktop_RedirectDrives = profileInfo.RemoteDesktop_RedirectDrives;
        RemoteDesktop_OverrideRedirectPorts = profileInfo.RemoteDesktop_OverrideRedirectPorts;
        RemoteDesktop_RedirectPorts = profileInfo.RemoteDesktop_RedirectPorts;
        RemoteDesktop_OverrideRedirectSmartcards = profileInfo.RemoteDesktop_OverrideRedirectSmartcards;
        RemoteDesktop_RedirectSmartCards = profileInfo.RemoteDesktop_RedirectSmartCards;
        RemoteDesktop_OverrideRedirectPrinters = profileInfo.RemoteDesktop_OverrideRedirectPrinters;
        RemoteDesktop_RedirectPrinters = profileInfo.RemoteDesktop_RedirectPrinters;
        RemoteDesktop_OverridePersistentBitmapCaching = profileInfo.RemoteDesktop_OverridePersistentBitmapCaching;
        RemoteDesktop_PersistentBitmapCaching = profileInfo.RemoteDesktop_PersistentBitmapCaching;
        RemoteDesktop_OverrideReconnectIfTheConnectionIsDropped =
            profileInfo.RemoteDesktop_OverrideReconnectIfTheConnectionIsDropped;
        RemoteDesktop_ReconnectIfTheConnectionIsDropped = profileInfo.RemoteDesktop_ReconnectIfTheConnectionIsDropped;
        RemoteDesktop_NetworkConnectionType =
            RemoteDesktop_NetworkConnectionTypes.FirstOrDefault(x =>
                x == profileInfo.RemoteDesktop_NetworkConnectionType);
        RemoteDesktop_DesktopBackground = profileInfo.RemoteDesktop_DesktopBackground;
        RemoteDesktop_FontSmoothing = profileInfo.RemoteDesktop_FontSmoothing;
        RemoteDesktop_DesktopComposition = profileInfo.RemoteDesktop_DesktopComposition;
        RemoteDesktop_ShowWindowContentsWhileDragging = profileInfo.RemoteDesktop_ShowWindowContentsWhileDragging;
        RemoteDesktop_MenuAndWindowAnimation = profileInfo.RemoteDesktop_MenuAndWindowAnimation;
        RemoteDesktop_VisualStyles = profileInfo.RemoteDesktop_VisualStyles;

        // PowerShell
        PowerShell_Enabled = editMode == ProfileEditMode.Add
            ? applicationName == ApplicationName.PowerShell
            : profileInfo.PowerShell_Enabled;
        PowerShell_EnableRemoteConsole = profileInfo.PowerShell_EnableRemoteConsole;
        PowerShell_InheritHost = profileInfo.PowerShell_InheritHost;
        PowerShell_Host = profileInfo.PowerShell_Host;
        PowerShell_OverrideCommand = profileInfo.PowerShell_OverrideCommand;
        PowerShell_Command = profileInfo.PowerShell_Command;
        PowerShell_OverrideAdditionalCommandLine = profileInfo.PowerShell_OverrideAdditionalCommandLine;
        PowerShell_AdditionalCommandLine = profileInfo.PowerShell_AdditionalCommandLine;
        PowerShell_ExecutionPolicies = Enum.GetValues(typeof(ExecutionPolicy)).Cast<ExecutionPolicy>().ToList();
        PowerShell_OverrideExecutionPolicy = profileInfo.PowerShell_OverrideExecutionPolicy;
        PowerShell_ExecutionPolicy = profileInfo.PowerShell_ExecutionPolicy;

        // PuTTY
        PuTTY_Enabled = editMode == ProfileEditMode.Add
            ? applicationName == ApplicationName.PuTTY
            : profileInfo.PuTTY_Enabled;

        switch (profileInfo.PuTTY_ConnectionMode)
        {
            // SSH is default
            case ConnectionMode.SSH:
                PuTTY_UseSSH = true;
                break;
            case ConnectionMode.Telnet:
                PuTTY_UseTelnet = true;
                break;
            case ConnectionMode.Serial:
                PuTTY_UseSerial = true;
                break;
            case ConnectionMode.Rlogin:
                PuTTY_UseRlogin = true;
                break;
            case ConnectionMode.RAW:
                PuTTY_UseRAW = true;
                break;
        }

        PuTTY_InheritHost = profileInfo.PuTTY_InheritHost;

        if (profileInfo.PuTTY_ConnectionMode != ConnectionMode.Serial)
            PuTTY_Host = profileInfo.PuTTY_HostOrSerialLine;
        else
            PuTTY_SerialLine = profileInfo.PuTTY_HostOrSerialLine;

        PuTTY_OverridePortOrBaud = profileInfo.PuTTY_OverridePortOrBaud;

        if (profileInfo.PuTTY_ConnectionMode != ConnectionMode.Serial)
            PuTTY_Port = profileInfo.PuTTY_PortOrBaud;
        else
            PuTTY_Baud = profileInfo.PuTTY_PortOrBaud;

        PuTTY_OverrideUsername = profileInfo.PuTTY_OverrideUsername;
        PuTTY_Username = profileInfo.PuTTY_Username;
        PuTTY_OverridePrivateKeyFile = profileInfo.PuTTY_OverridePrivateKeyFile;
        PuTTY_PrivateKeyFile = profileInfo.PuTTY_PrivateKeyFile;
        PuTTY_OverrideProfile = profileInfo.PuTTY_OverrideProfile;
        PuTTY_Profile = profileInfo.PuTTY_Profile;
        PuTTY_OverrideHostkey = profileInfo.PuTTY_OverrideHostkey;
        PuTTY_Hostkey = profileInfo.PuTTY_Hostkey;
        PuTTY_OverrideEnableLog = profileInfo.PuTTY_OverrideEnableLog;
        PuTTY_EnableLog = profileInfo.PuTTY_EnableLog;
        PuTTY_OverrideLogMode = profileInfo.PuTTY_OverrideLogMode;
        PuTTY_LogMode = PuTTY_LogModes.FirstOrDefault(x => x == profileInfo.PuTTY_LogMode);
        PuTTY_OverrideLogPath = profileInfo.PuTTY_OverrideLogPath;
        PuTTY_LogPath = profileInfo.PuTTY_LogPath;
        PuTTY_OverrideLogFileName = profileInfo.PuTTY_OverrideLogFileName;
        PuTTY_LogFileName = profileInfo.PuTTY_LogFileName;
        PuTTY_OverrideAdditionalCommandLine = profileInfo.PuTTY_OverrideAdditionalCommandLine;
        PuTTY_AdditionalCommandLine = profileInfo.PuTTY_AdditionalCommandLine;

        // TigerVNC
        TigerVNC_Enabled = editMode == ProfileEditMode.Add
            ? applicationName == ApplicationName.TigerVNC
            : profileInfo.TigerVNC_Enabled;
        TigerVNC_InheritHost = profileInfo.TigerVNC_InheritHost;
        TigerVNC_Host = profileInfo.TigerVNC_Host;
        TigerVNC_OverridePort = profileInfo.TigerVNC_OverridePort;
        TigerVNC_Port = profileInfo.TigerVNC_Port;

        // Web Console
        WebConsole_Enabled = editMode == ProfileEditMode.Add
            ? applicationName == ApplicationName.WebConsole
            : profileInfo.WebConsole_Enabled;
        WebConsole_Url = profileInfo.WebConsole_Url;

        // SNMP
        SNMP_Enabled = editMode == ProfileEditMode.Add
            ? applicationName == ApplicationName.SNMP
            : profileInfo.SNMP_Enabled;
        SNMP_InheritHost = profileInfo.SNMP_InheritHost;
        SNMP_Host = profileInfo.SNMP_Host;
        SNMP_OverrideOIDAndMode = profileInfo.SNMP_OverrideOIDAndMode;
        SNMP_OID = profileInfo.SNMP_OID;
        SNMP_Modes = new List<SNMPMode> { SNMPMode.Get, SNMPMode.Walk, SNMPMode.Set };
        SNMP_Mode = SNMP_Modes.FirstOrDefault(x => x == profileInfo.SNMP_Mode);
        SNMP_OverrideVersionAndAuth = profileInfo.SNMP_OverrideVersionAndAuth;
        SNMP_Versions = Enum.GetValues(typeof(SNMPVersion)).Cast<SNMPVersion>().ToList();
        SNMP_Version = SNMP_Versions.FirstOrDefault(x => x == profileInfo.SNMP_Version);
        SNMP_Community = profileInfo.SNMP_Community;
        SNMP_Securities = new List<SNMPV3Security>
            { SNMPV3Security.NoAuthNoPriv, SNMPV3Security.AuthNoPriv, SNMPV3Security.AuthPriv };
        SNMP_Security = SNMP_Securities.FirstOrDefault(x => x == profileInfo.SNMP_Security);
        SNMP_Username = profileInfo.SNMP_Username;
        SNMP_AuthenticationProviders = Enum.GetValues(typeof(SNMPV3AuthenticationProvider))
            .Cast<SNMPV3AuthenticationProvider>().ToList();
        SNMP_AuthenticationProvider =
            SNMP_AuthenticationProviders.FirstOrDefault(x => x == profileInfo.SNMP_AuthenticationProvider);
        SNMP_Auth = profileInfo.SNMP_Auth;
        SNMP_PrivacyProviders = Enum.GetValues(typeof(SNMPV3PrivacyProvider)).Cast<SNMPV3PrivacyProvider>().ToList();
        SNMP_PrivacyProvider = SNMP_PrivacyProviders.FirstOrDefault(x => x == profileInfo.SNMP_PrivacyProvider);
        SNMP_Priv = profileInfo.SNMP_Priv;

        // Firewall
        Firewall_Enabled = editMode == ProfileEditMode.Add
            ? applicationName == ApplicationName.Firewall
            : profileInfo.Firewall_Enabled;
        
        // Wake on LAN
        WakeOnLAN_Enabled = editMode == ProfileEditMode.Add
            ? applicationName == ApplicationName.WakeOnLAN
            : profileInfo.WakeOnLAN_Enabled;
        WakeOnLAN_MACAddress = profileInfo.WakeOnLAN_MACAddress;
        WakeOnLAN_Broadcast = profileInfo.WakeOnLAN_Broadcast;

        // Whois
        Whois_Enabled = editMode == ProfileEditMode.Add
            ? applicationName == ApplicationName.Whois
            : profileInfo.Whois_Enabled;
        Whois_InheritHost = profileInfo.Whois_InheritHost;
        Whois_Domain = profileInfo.Whois_Domain;

        // IP Geolocation
        IPGeolocation_Enabled = editMode == ProfileEditMode.Add
            ? applicationName == ApplicationName.IPGeolocation
            : profileInfo.IPGeolocation_Enabled;
        IPGeolocation_InheritHost = profileInfo.IPGeolocation_InheritHost;
        IPGeolocation_Host = profileInfo.IPGeolocation_Host;

        _isLoading = false;
    }
    #endregion

    #region Methods

    private void ChangeNetworkConnectionTypeSettings(NetworkConnectionType connectionSpeed)
    {
        switch (connectionSpeed)
        {
            case NetworkConnectionType.Modem:
                RemoteDesktop_DesktopBackground = false;
                RemoteDesktop_FontSmoothing = false;
                RemoteDesktop_DesktopComposition = false;
                RemoteDesktop_ShowWindowContentsWhileDragging = false;
                RemoteDesktop_MenuAndWindowAnimation = false;
                RemoteDesktop_VisualStyles = false;
                break;
            case NetworkConnectionType.BroadbandLow:
                RemoteDesktop_DesktopBackground = false;
                RemoteDesktop_FontSmoothing = false;
                RemoteDesktop_DesktopComposition = false;
                RemoteDesktop_ShowWindowContentsWhileDragging = false;
                RemoteDesktop_MenuAndWindowAnimation = false;
                RemoteDesktop_VisualStyles = true;
                break;
            case NetworkConnectionType.Satellite:
            case NetworkConnectionType.BroadbandHigh:
                RemoteDesktop_DesktopBackground = false;
                RemoteDesktop_FontSmoothing = false;
                RemoteDesktop_DesktopComposition = true;
                RemoteDesktop_ShowWindowContentsWhileDragging = false;
                RemoteDesktop_MenuAndWindowAnimation = false;
                RemoteDesktop_VisualStyles = true;
                break;
            case NetworkConnectionType.WAN:
            case NetworkConnectionType.LAN:
                RemoteDesktop_DesktopBackground = true;
                RemoteDesktop_FontSmoothing = true;
                RemoteDesktop_DesktopComposition = true;
                RemoteDesktop_ShowWindowContentsWhileDragging = true;
                RemoteDesktop_MenuAndWindowAnimation = true;
                RemoteDesktop_VisualStyles = true;
                break;
        }
    }
    #endregion

    #region Variables

    private readonly bool _isLoading = true;

    public bool IsProfileFileEncrypted => ProfileManager.LoadedProfileFile.IsEncrypted;

    public ICollectionView ProfileViews { get; }

    #region General

    public string Name
    {
        get;
        set
        {
            if (value == field)
                return;
           
            field = value;
            OnPropertyChanged();
        }
    }

    public string Host
    {
        get;
        set
        {
            if (value == field)
                return;

            // Reset, if string has changed
            if (!IsResolveHostnameRunning)
                ShowCouldNotResolveHostnameWarning = false;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsResolveHostnameRunning
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool ShowCouldNotResolveHostnameWarning
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string Description
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string Group
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public ICollectionView Groups { get; }

    public string Tag
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public ICollectionView Tags { get; }

    public ObservableSetCollection<string> TagsCollection
    {
        get;
        private init
        {
            if (Equals(value, field))
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Network Interface

    public bool NetworkInterface_Enabled
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
        }
    }

    public bool NetworkInterface_EnableDynamicIPAddress
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool NetworkInterface_EnableStaticIPAddress
    {
        get;
        set
        {
            if (value == field)
                return;

            if (value)
                NetworkInterface_EnableStaticDNS = true;

            field = value;
            OnPropertyChanged();
        }
    }

    public string NetworkInterface_IPAddress
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string NetworkInterface_Subnetmask
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string NetworkInterface_Gateway
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool NetworkInterface_EnableDynamicDNS
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool NetworkInterface_EnableStaticDNS
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string NetworkInterface_PrimaryDNSServer
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string NetworkInterface_SecondaryDNSServer
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region IP Scanner

    public bool IPScanner_Enabled
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
        }
    }

    public bool IPScanner_InheritHost
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string IPScanner_HostOrIPRange
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Port Scanner

    public bool PortScanner_Enabled
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
        }
    }

    public bool PortScanner_InheritHost
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string PortScanner_Host
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string PortScanner_Ports
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Ping Monitor

    public bool PingMonitor_Enabled
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
        }
    }

    public bool PingMonitor_InheritHost
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string PingMonitor_Host
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Traceroute

    public bool Traceroute_Enabled
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
        }
    }

    public bool Traceroute_InheritHost
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string Traceroute_Host
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region DNS Lookup

    public bool DNSLookup_Enabled
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
        }
    }

    public bool DNSLookup_InheritHost
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string DNSLookup_Host
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Remote Desktop

    public bool RemoteDesktop_Enabled
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_InheritHost
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string RemoteDesktop_Host
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_UseCredentials
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string RemoteDesktop_Username
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string RemoteDesktop_Domain
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_IsPasswordEmpty
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = true;

    public SecureString RemoteDesktop_Password
    {
        get;
        set
        {
            if (value == field)
                return;

            // Validate the password string
            RemoteDesktop_IsPasswordEmpty =
                value == null || string.IsNullOrEmpty(SecureStringHelper.ConvertToString(value));

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_AdminSession
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_OverrideDisplay
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_AdjustScreenAutomatically
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_UseCurrentViewSize
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_UseFixedScreenSize
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public List<string> RemoteDesktop_ScreenResolutions => RemoteDesktop.ScreenResolutions;

    public int RemoteDesktop_ScreenWidth;
    public int RemoteDesktop_ScreenHeight;

    public string RemoteDesktop_SelectedScreenResolution
    {
        get;
        set
        {
            if (value == field)
                return;

            var resolution = value.Split('x');

            RemoteDesktop_ScreenWidth = int.Parse(resolution[0]);
            RemoteDesktop_ScreenHeight = int.Parse(resolution[1]);

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_UseCustomScreenSize
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string RemoteDesktop_CustomScreenWidth
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string RemoteDesktop_CustomScreenHeight
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_OverrideColorDepth
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public List<int> RemoteDesktop_ColorDepths => RemoteDesktop.ColorDepths;

    public int RemoteDesktop_SelectedColorDepth
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_OverridePort
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public int RemoteDesktop_Port
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_OverrideCredSspSupport
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_EnableCredSspSupport
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_OverrideAuthenticationLevel
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public uint RemoteDesktop_AuthenticationLevel
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_OverrideGatewayServer
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_EnableGatewayServer
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string RemoteDesktop_GatewayServerHostname
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_GatewayServerBypassLocalAddresses
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<GatewayUserSelectedCredsSource> RemoteDesktop_GatewayServerLogonMethods =>
        Enum.GetValues(typeof(GatewayUserSelectedCredsSource)).Cast<GatewayUserSelectedCredsSource>();

    public GatewayUserSelectedCredsSource RemoteDesktop_GatewayServerLogonMethod
    {
        get;
        set
        {
            if (Equals(value, field))
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_GatewayServerShareCredentialsWithRemoteComputer
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_UseGatewayServerCredentials
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string RemoteDesktop_GatewayServerUsername
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string RemoteDesktop_GatewayServerDomain
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_IsGatewayServerPasswordEmpty
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = true;

    public SecureString RemoteDesktop_GatewayServerPassword
    {
        get;
        set
        {
            if (value == field)
                return;

            // Validate the password string
            RemoteDesktop_IsGatewayServerPasswordEmpty =
                value == null || string.IsNullOrEmpty(SecureStringHelper.ConvertToString(value));

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_OverrideAudioRedirectionMode
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<AudioRedirectionMode> RemoteDesktop_AudioRedirectionModes =>
        Enum.GetValues(typeof(AudioRedirectionMode)).Cast<AudioRedirectionMode>();

    public AudioRedirectionMode RemoteDesktop_AudioRedirectionMode
    {
        get;
        set
        {
            if (Equals(value, field))
                return;

            field = value;
            OnPropertyChanged();
        }
    }


    public bool RemoteDesktop_OverrideAudioCaptureRedirectionMode
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<AudioCaptureRedirectionMode> RemoteDesktop_AudioCaptureRedirectionModes =>
        Enum.GetValues(typeof(AudioCaptureRedirectionMode)).Cast<AudioCaptureRedirectionMode>();

    public AudioCaptureRedirectionMode RemoteDesktop_AudioCaptureRedirectionMode
    {
        get;
        set
        {
            if (Equals(value, field))
                return;

            field = value;
            OnPropertyChanged();
        }
    }


    public bool RemoteDesktop_OverrideApplyWindowsKeyCombinations
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<KeyboardHookMode> RemoteDesktop_KeyboardHookModes =>
        Enum.GetValues(typeof(KeyboardHookMode)).Cast<KeyboardHookMode>();

    public KeyboardHookMode RemoteDesktop_KeyboardHookMode
    {
        get;
        set
        {
            if (Equals(value, field))
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_OverrideRedirectClipboard
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_RedirectClipboard
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_OverrideRedirectDevices
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_RedirectDevices
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_OverrideRedirectDrives
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_RedirectDrives
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_OverrideRedirectPorts
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_RedirectPorts
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_OverrideRedirectSmartcards
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_RedirectSmartCards
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_OverrideRedirectPrinters
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_RedirectPrinters
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_OverridePersistentBitmapCaching
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_PersistentBitmapCaching
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_OverrideReconnectIfTheConnectionIsDropped
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_ReconnectIfTheConnectionIsDropped
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_OverrideNetworkConnectionType
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<NetworkConnectionType> RemoteDesktop_NetworkConnectionTypes =>
        Enum.GetValues(typeof(NetworkConnectionType)).Cast<NetworkConnectionType>();

    public NetworkConnectionType RemoteDesktop_NetworkConnectionType
    {
        get;
        set
        {
            if (Equals(value, field))
                return;

            if (!_isLoading)
                ChangeNetworkConnectionTypeSettings(value);

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_DesktopBackground
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_FontSmoothing
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_DesktopComposition
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_ShowWindowContentsWhileDragging
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_MenuAndWindowAnimation
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_VisualStyles
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region PowerShell

    public bool PowerShell_Enabled
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
        }
    }

    public bool PowerShell_EnableRemoteConsole
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool PowerShell_InheritHost
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string PowerShell_Host
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool PowerShell_OverrideCommand
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string PowerShell_Command
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool PowerShell_OverrideAdditionalCommandLine
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string PowerShell_AdditionalCommandLine
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public List<ExecutionPolicy> PowerShell_ExecutionPolicies
    {
        get;
        private init
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = new();

    public bool PowerShell_OverrideExecutionPolicy
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public ExecutionPolicy PowerShell_ExecutionPolicy
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region PuTTY

    public bool PuTTY_Enabled
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
        }
    }

    public bool PuTTY_InheritHost
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool PuTTY_UseSSH
    {
        get;
        set
        {
            if (value == field)
                return;

            if (value)
            {
                PuTTY_Port = SettingsManager.Current.PuTTY_SSHPort;
                PuTTY_ConnectionMode = ConnectionMode.SSH;
            }

            field = value;
            OnPropertyChanged();
        }
    }

    public bool PuTTY_UseTelnet
    {
        get;
        set
        {
            if (value == field)
                return;

            if (value)
            {
                PuTTY_Port = SettingsManager.Current.PuTTY_TelnetPort;
                PuTTY_ConnectionMode = ConnectionMode.Telnet;
            }

            field = value;
            OnPropertyChanged();
        }
    }

    public bool PuTTY_UseSerial
    {
        get;
        set
        {
            if (value == field)
                return;

            if (value)
            {
                PuTTY_Baud = SettingsManager.Current.PuTTY_BaudRate;
                PuTTY_ConnectionMode = ConnectionMode.Serial;
            }

            field = value;
            OnPropertyChanged();
        }
    }

    public bool PuTTY_UseRlogin
    {
        get;
        set
        {
            if (value == field)
                return;

            if (value)
            {
                PuTTY_Port = SettingsManager.Current.PuTTY_RloginPort;
                PuTTY_ConnectionMode = ConnectionMode.Rlogin;
            }

            field = value;
            OnPropertyChanged();
        }
    }

    public bool PuTTY_UseRAW
    {
        get;
        set
        {
            if (value == field)
                return;

            if (value)
            {
                PuTTY_Port = SettingsManager.Current.PuTTY_RawPort;
                PuTTY_ConnectionMode = ConnectionMode.RAW;
            }

            field = value;
            OnPropertyChanged();
        }
    }

    public string PuTTY_Host
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string PuTTY_SerialLine
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool PuTTY_OverridePortOrBaud
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public int PuTTY_Port
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public int PuTTY_Baud
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool PuTTY_OverrideUsername
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string PuTTY_Username
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool PuTTY_OverridePrivateKeyFile
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string PuTTY_PrivateKeyFile
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool PuTTY_OverrideProfile
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string PuTTY_Profile
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool PuTTY_OverrideHostkey
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string PuTTY_Hostkey
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool PuTTY_OverrideEnableLog
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool PuTTY_EnableLog
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool PuTTY_OverrideLogMode
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<LogMode> PuTTY_LogModes => Enum.GetValues(typeof(LogMode)).Cast<LogMode>();

    public LogMode PuTTY_LogMode
    {
        get;
        set
        {
            if (Equals(value, field))
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool PuTTY_OverrideLogPath
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string PuTTY_LogPath
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool PuTTY_OverrideLogFileName
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string PuTTY_LogFileName
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool PuTTY_OverrideAdditionalCommandLine
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string PuTTY_AdditionalCommandLine
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public ConnectionMode PuTTY_ConnectionMode
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region TigerVNC

    public bool TigerVNC_Enabled
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool TigerVNC_InheritHost
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string TigerVNC_Host
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }


    public bool TigerVNC_OverridePort
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public int TigerVNC_Port
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Web Console

    public bool WebConsole_Enabled
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string WebConsole_Url
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region SNMP

    public bool SNMP_Enabled
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
        }
    }

    public bool SNMP_InheritHost
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string SNMP_Host
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool SNMP_OverrideOIDAndMode
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string SNMP_OID
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<SNMPMode> SNMP_Modes { get; set; }

    public SNMPMode SNMP_Mode
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();

            // Re-validate OID if mode changed
            OnPropertyChanged(nameof(SNMP_OID));
        }
    }

    public bool SNMP_OverrideVersionAndAuth
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<SNMPVersion> SNMP_Versions { get; }

    public SNMPVersion SNMP_Version
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool SNMP_IsCommunityEmpty
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = true;

    public SecureString SNMP_Community
    {
        get;
        set
        {
            if (value == field)
                return;

            // Validate the password string
            SNMP_IsCommunityEmpty = value == null || string.IsNullOrEmpty(SecureStringHelper.ConvertToString(value));

            field = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<SNMPV3Security> SNMP_Securities { get; }

    public SNMPV3Security SNMP_Security
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string SNMP_Username
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<SNMPV3AuthenticationProvider> SNMP_AuthenticationProviders { get; }

    public SNMPV3AuthenticationProvider SNMP_AuthenticationProvider
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool SNMP_IsAuthEmpty
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = true;

    public SecureString SNMP_Auth
    {
        get;
        set
        {
            if (value == field)
                return;

            // Validate the password string
            SNMP_IsAuthEmpty = value == null || string.IsNullOrEmpty(SecureStringHelper.ConvertToString(value));

            field = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<SNMPV3PrivacyProvider> SNMP_PrivacyProviders { get; }

    public SNMPV3PrivacyProvider SNMP_PrivacyProvider
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool SNMP_IsPrivEmpty
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = true;

    public SecureString SNMP_Priv
    {
        get;
        set
        {
            if (value == field)
                return;

            // Validate the password string
            SNMP_IsPrivEmpty = value == null || string.IsNullOrEmpty(SecureStringHelper.ConvertToString(value));

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Firewall

    public bool Firewall_Enabled
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Name));
        }
    }
  
    #endregion Firewall
    
    #region Wake on LAN

    public bool WakeOnLAN_Enabled
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
        }
    }

    public string WakeOnLAN_MACAddress
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string WakeOnLAN_Broadcast
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Whois

    public bool Whois_Enabled
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
        }
    }

    public bool Whois_InheritHost
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string Whois_Domain
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region IP Geolocation

    public bool IPGeolocation_Enabled
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
        }
    }

    public bool IPGeolocation_InheritHost
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string IPGeolocation_Host
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #endregion

    #region ICommands & Actions

    public ICommand SaveCommand { get; }

    public ICommand CancelCommand { get; }

    public ICommand ResolveHostCommand => new RelayCommand(async _ => await ResolveHostActionAsync());

    private async Task ResolveHostActionAsync()
    {
        IsResolveHostnameRunning = true;

        await Task.Delay(GlobalStaticConfiguration.ApplicationUIDelayInterval);

        var dnsResult =
            await DNSClientHelper.ResolveAorAaaaAsync(Host, SettingsManager.Current.Network_ResolveHostnamePreferIPv4);

        if (!dnsResult.HasError)
            Host = dnsResult.Value.ToString();
        else
            ShowCouldNotResolveHostnameWarning = true;

        IsResolveHostnameRunning = false;
    }

    public ICommand AddTagCommand => new RelayCommand(_ => AddTagAction());

    private void AddTagAction()
    {
        var tagsToAdd = Tag.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var tag in tagsToAdd)
            TagsCollection.Add(tag);

        Tag = string.Empty;
    }

    public ICommand RemoveTagCommand => new RelayCommand(RemoveTagAction);

    private void RemoveTagAction(object param)
    {
        if (param is not string tag)
            return;

        TagsCollection.Remove(tag);
    }
    #endregion
}