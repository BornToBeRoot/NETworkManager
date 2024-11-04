﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using NETworkManager.Localization.Resources;
using NETworkManager.Models;
using NETworkManager.Models.Network;
using NETworkManager.Models.PowerShell;
using NETworkManager.Models.PuTTY;
using NETworkManager.Models.RemoteDesktop;
using NETworkManager.Profiles;
using NETworkManager.Settings;
using NETworkManager.Utilities;

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

        Tags = profileInfo.Tags;
        
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

        // AWS Session Manager
        AWSSessionManager_Enabled = editMode == ProfileEditMode.Add
            ? applicationName == ApplicationName.AWSSessionManager
            : profileInfo.AWSSessionManager_Enabled;
        AWSSessionManager_InstanceID = profileInfo.AWSSessionManager_InstanceID;
        AWSSessionManager_OverrideProfile = profileInfo.AWSSessionManager_OverrideProfile;
        AWSSessionManager_Profile = profileInfo.AWSSessionManager_Profile;
        AWSSessionManager_OverrideRegion = profileInfo.AWSSessionManager_OverrideRegion;
        AWSSessionManager_Region = profileInfo.AWSSessionManager_Region;

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

    private string _name;

    public string Name
    {
        get => _name;
        set
        {
            if (value == _name)
                return;

            _name = value;
            OnPropertyChanged();
        }
    }

    private string _host;

    public string Host
    {
        get => _host;
        set
        {
            if (value == _host)
                return;

            // Reset, if string has changed
            if (!IsResolveHostnameRunning)
                ShowCouldNotResolveHostnameWarning = false;

            _host = value;
            OnPropertyChanged();
        }
    }

    private bool _isResolveHostnameRunning;

    public bool IsResolveHostnameRunning
    {
        get => _isResolveHostnameRunning;
        set
        {
            if (value == _isResolveHostnameRunning)
                return;

            _isResolveHostnameRunning = value;
            OnPropertyChanged();
        }
    }

    private bool _showCouldNotResolveHostnameWarning;

    public bool ShowCouldNotResolveHostnameWarning
    {
        get => _showCouldNotResolveHostnameWarning;
        set
        {
            if (value == _showCouldNotResolveHostnameWarning)
                return;

            _showCouldNotResolveHostnameWarning = value;
            OnPropertyChanged();
        }
    }
    
    private string _description;

    public string Description
    {
        get => _description;
        set
        {
            if (value == _description)
                return;

            _description = value;
            OnPropertyChanged();
        }
    }

    private string _group;

    public string Group
    {
        get => _group;
        set
        {
            if (value == _group)
                return;

            _group = value;
            OnPropertyChanged();
        }
    }

    public ICollectionView Groups { get; }

    private string _tags;

    public string Tags
    {
        get => _tags;
        set
        {
            if (value == _tags)
                return;

            _tags = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Network Interface

    private bool _networkInterface_Enabled;

    public bool NetworkInterface_Enabled
    {
        get => _networkInterface_Enabled;
        set
        {
            if (value == _networkInterface_Enabled)
                return;

            _networkInterface_Enabled = value;

            OnPropertyChanged();
        }
    }

    private bool _networkInterface_EnableDynamicIPAddress = true;

    public bool NetworkInterface_EnableDynamicIPAddress
    {
        get => _networkInterface_EnableDynamicIPAddress;
        set
        {
            if (value == _networkInterface_EnableDynamicIPAddress)
                return;

            _networkInterface_EnableDynamicIPAddress = value;
            OnPropertyChanged();
        }
    }

    private bool _networkInterface_EnableStaticIPAddress;

    public bool NetworkInterface_EnableStaticIPAddress
    {
        get => _networkInterface_EnableStaticIPAddress;
        set
        {
            if (value == _networkInterface_EnableStaticIPAddress)
                return;

            if (value)
                NetworkInterface_EnableStaticDNS = true;

            _networkInterface_EnableStaticIPAddress = value;
            OnPropertyChanged();
        }
    }

    private string _networkInterface_IPAddress;

    public string NetworkInterface_IPAddress
    {
        get => _networkInterface_IPAddress;
        set
        {
            if (value == _networkInterface_IPAddress)
                return;

            _networkInterface_IPAddress = value;
            OnPropertyChanged();
        }
    }

    private string _networkInterface_Subnetmask;

    public string NetworkInterface_Subnetmask
    {
        get => _networkInterface_Subnetmask;
        set
        {
            if (value == _networkInterface_Subnetmask)
                return;

            _networkInterface_Subnetmask = value;
            OnPropertyChanged();
        }
    }

    private string _networkInterface_Gateway;

    public string NetworkInterface_Gateway
    {
        get => _networkInterface_Gateway;
        set
        {
            if (value == _networkInterface_Gateway)
                return;

            _networkInterface_Gateway = value;
            OnPropertyChanged();
        }
    }

    private bool _networkInterface_EnableDynamicDNS = true;

    public bool NetworkInterface_EnableDynamicDNS
    {
        get => _networkInterface_EnableDynamicDNS;
        set
        {
            if (value == _networkInterface_EnableDynamicDNS)
                return;

            _networkInterface_EnableDynamicDNS = value;
            OnPropertyChanged();
        }
    }

    private bool _networkInterface_EnableStaticDNS;

    public bool NetworkInterface_EnableStaticDNS
    {
        get => _networkInterface_EnableStaticDNS;
        set
        {
            if (value == _networkInterface_EnableStaticDNS)
                return;

            _networkInterface_EnableStaticDNS = value;
            OnPropertyChanged();
        }
    }

    private string _networkInterface_PrimaryDNSServer;

    public string NetworkInterface_PrimaryDNSServer
    {
        get => _networkInterface_PrimaryDNSServer;
        set
        {
            if (value == _networkInterface_PrimaryDNSServer)
                return;

            _networkInterface_PrimaryDNSServer = value;
            OnPropertyChanged();
        }
    }

    private string _networkInterface_SecondaryDNSServer;

    public string NetworkInterface_SecondaryDNSServer
    {
        get => _networkInterface_SecondaryDNSServer;
        set
        {
            if (value == _networkInterface_SecondaryDNSServer)
                return;

            _networkInterface_SecondaryDNSServer = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region IP Scanner

    private bool _ipScanner_Enabled;

    public bool IPScanner_Enabled
    {
        get => _ipScanner_Enabled;
        set
        {
            if (value == _ipScanner_Enabled)
                return;

            _ipScanner_Enabled = value;

            OnPropertyChanged();
        }
    }

    private bool _ipScanner_InheritHost;

    public bool IPScanner_InheritHost
    {
        get => _ipScanner_InheritHost;
        set
        {
            if (value == _ipScanner_InheritHost)
                return;

            _ipScanner_InheritHost = value;
            OnPropertyChanged();
        }
    }

    private string _ipScanner_HostOrIPRange;

    public string IPScanner_HostOrIPRange
    {
        get => _ipScanner_HostOrIPRange;
        set
        {
            if (value == _ipScanner_HostOrIPRange)
                return;

            _ipScanner_HostOrIPRange = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Port Scanner

    private bool _portScanner_Enabled;

    public bool PortScanner_Enabled
    {
        get => _portScanner_Enabled;
        set
        {
            if (value == _portScanner_Enabled)
                return;

            _portScanner_Enabled = value;

            OnPropertyChanged();
        }
    }

    private bool _portScanner_InheritHost;

    public bool PortScanner_InheritHost
    {
        get => _portScanner_InheritHost;
        set
        {
            if (value == _portScanner_InheritHost)
                return;

            _portScanner_InheritHost = value;
            OnPropertyChanged();
        }
    }

    private string _portScanner_Host;

    public string PortScanner_Host
    {
        get => _portScanner_Host;
        set
        {
            if (value == _portScanner_Host)
                return;

            _portScanner_Host = value;
            OnPropertyChanged();
        }
    }

    private string _portScanner_Ports;

    public string PortScanner_Ports
    {
        get => _portScanner_Ports;
        set
        {
            if (value == _portScanner_Ports)
                return;

            _portScanner_Ports = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Ping Monitor

    private bool _pingMonitor_Enabled;

    public bool PingMonitor_Enabled
    {
        get => _pingMonitor_Enabled;
        set
        {
            if (value == _pingMonitor_Enabled)
                return;

            _pingMonitor_Enabled = value;

            OnPropertyChanged();
        }
    }

    private bool _pingMonitor_InheritHost;

    public bool PingMonitor_InheritHost
    {
        get => _pingMonitor_InheritHost;
        set
        {
            if (value == _pingMonitor_InheritHost)
                return;

            _pingMonitor_InheritHost = value;
            OnPropertyChanged();
        }
    }

    private string _pingMonitor_Host;

    public string PingMonitor_Host
    {
        get => _pingMonitor_Host;
        set
        {
            if (value == _pingMonitor_Host)
                return;

            _pingMonitor_Host = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Traceroute

    private bool _traceroute_Enabled;

    public bool Traceroute_Enabled
    {
        get => _traceroute_Enabled;
        set
        {
            if (value == _traceroute_Enabled)
                return;

            _traceroute_Enabled = value;

            OnPropertyChanged();
        }
    }

    private bool _traceroute_InheritHost;

    public bool Traceroute_InheritHost
    {
        get => _traceroute_InheritHost;
        set
        {
            if (value == _traceroute_InheritHost)
                return;

            _traceroute_InheritHost = value;
            OnPropertyChanged();
        }
    }

    private string _traceroute_Host;

    public string Traceroute_Host
    {
        get => _traceroute_Host;
        set
        {
            if (value == _traceroute_Host)
                return;

            _traceroute_Host = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region DNS Lookup

    private bool _dnsLookup_Enabled;

    public bool DNSLookup_Enabled
    {
        get => _dnsLookup_Enabled;
        set
        {
            if (value == _dnsLookup_Enabled)
                return;

            _dnsLookup_Enabled = value;

            OnPropertyChanged();
        }
    }

    private bool _dnsLookup_InheritHost;

    public bool DNSLookup_InheritHost
    {
        get => _dnsLookup_InheritHost;
        set
        {
            if (value == _dnsLookup_InheritHost)
                return;

            _dnsLookup_InheritHost = value;
            OnPropertyChanged();
        }
    }

    private string _dnsLookup_Host;

    public string DNSLookup_Host
    {
        get => _dnsLookup_Host;
        set
        {
            if (value == _dnsLookup_Host)
                return;

            _dnsLookup_Host = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Remote Desktop

    private bool _remoteDesktop_Enabled;

    public bool RemoteDesktop_Enabled
    {
        get => _remoteDesktop_Enabled;
        set
        {
            if (value == _remoteDesktop_Enabled)
                return;

            _remoteDesktop_Enabled = value;

            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_InheritHost;

    public bool RemoteDesktop_InheritHost
    {
        get => _remoteDesktop_InheritHost;
        set
        {
            if (value == _remoteDesktop_InheritHost)
                return;

            _remoteDesktop_InheritHost = value;
            OnPropertyChanged();
        }
    }

    private string _remoteDesktop_Host;

    public string RemoteDesktop_Host
    {
        get => _remoteDesktop_Host;
        set
        {
            if (value == _remoteDesktop_Host)
                return;

            _remoteDesktop_Host = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_UseCredentials;

    public bool RemoteDesktop_UseCredentials
    {
        get => _remoteDesktop_UseCredentials;
        set
        {
            if (value == _remoteDesktop_UseCredentials)
                return;

            _remoteDesktop_UseCredentials = value;
            OnPropertyChanged();
        }
    }

    private string _remoteDesktop_Username;

    public string RemoteDesktop_Username
    {
        get => _remoteDesktop_Username;
        set
        {
            if (value == _remoteDesktop_Username)
                return;

            _remoteDesktop_Username = value;
            OnPropertyChanged();
        }
    }

    private string _remoteDesktop_Domain;

    public string RemoteDesktop_Domain
    {
        get => _remoteDesktop_Domain;
        set
        {
            if (value == _remoteDesktop_Domain)
                return;

            _remoteDesktop_Domain = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_IsPasswordEmpty = true; // Initial it's empty

    public bool RemoteDesktop_IsPasswordEmpty
    {
        get => _remoteDesktop_IsPasswordEmpty;
        set
        {
            if (value == _remoteDesktop_IsPasswordEmpty)
                return;

            _remoteDesktop_IsPasswordEmpty = value;
            OnPropertyChanged();
        }
    }

    private SecureString _remoteDesktop_Password;

    public SecureString RemoteDesktop_Password
    {
        get => _remoteDesktop_Password;
        set
        {
            if (value == _remoteDesktop_Password)
                return;

            // Validate the password string
            RemoteDesktop_IsPasswordEmpty =
                value == null || string.IsNullOrEmpty(SecureStringHelper.ConvertToString(value));

            _remoteDesktop_Password = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_OverrideDisplay;

    public bool RemoteDesktop_OverrideDisplay
    {
        get => _remoteDesktop_OverrideDisplay;
        set
        {
            if (value == _remoteDesktop_OverrideDisplay)
                return;

            _remoteDesktop_OverrideDisplay = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_AdjustScreenAutomatically;

    public bool RemoteDesktop_AdjustScreenAutomatically
    {
        get => _remoteDesktop_AdjustScreenAutomatically;
        set
        {
            if (value == _remoteDesktop_AdjustScreenAutomatically)
                return;

            _remoteDesktop_AdjustScreenAutomatically = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_UseCurrentViewSize;

    public bool RemoteDesktop_UseCurrentViewSize
    {
        get => _remoteDesktop_UseCurrentViewSize;
        set
        {
            if (value == _remoteDesktop_UseCurrentViewSize)
                return;

            _remoteDesktop_UseCurrentViewSize = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_UseFixedScreenSize;

    public bool RemoteDesktop_UseFixedScreenSize
    {
        get => _remoteDesktop_UseFixedScreenSize;
        set
        {
            if (value == _remoteDesktop_UseFixedScreenSize)
                return;

            _remoteDesktop_UseFixedScreenSize = value;
            OnPropertyChanged();
        }
    }

    public List<string> RemoteDesktop_ScreenResolutions => RemoteDesktop.ScreenResolutions;

    public int RemoteDesktop_ScreenWidth;
    public int RemoteDesktop_ScreenHeight;

    private string _remoteDesktop_SelectedScreenResolution;

    public string RemoteDesktop_SelectedScreenResolution
    {
        get => _remoteDesktop_SelectedScreenResolution;
        set
        {
            if (value == _remoteDesktop_SelectedScreenResolution)
                return;

            var resolution = value.Split('x');

            RemoteDesktop_ScreenWidth = int.Parse(resolution[0]);
            RemoteDesktop_ScreenHeight = int.Parse(resolution[1]);

            _remoteDesktop_SelectedScreenResolution = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_UseCustomScreenSize;

    public bool RemoteDesktop_UseCustomScreenSize
    {
        get => _remoteDesktop_UseCustomScreenSize;
        set
        {
            if (value == _remoteDesktop_UseCustomScreenSize)
                return;

            _remoteDesktop_UseCustomScreenSize = value;
            OnPropertyChanged();
        }
    }

    private string _remoteDesktop_CustomScreenWidth;

    public string RemoteDesktop_CustomScreenWidth
    {
        get => _remoteDesktop_CustomScreenWidth;
        set
        {
            if (value == _remoteDesktop_CustomScreenWidth)
                return;

            _remoteDesktop_CustomScreenWidth = value;
            OnPropertyChanged();
        }
    }

    private string _remoteDesktop_CustomScreenHeight;

    public string RemoteDesktop_CustomScreenHeight
    {
        get => _remoteDesktop_CustomScreenHeight;
        set
        {
            if (value == _remoteDesktop_CustomScreenHeight)
                return;

            _remoteDesktop_CustomScreenHeight = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_OverrideColorDepth;

    public bool RemoteDesktop_OverrideColorDepth
    {
        get => _remoteDesktop_OverrideColorDepth;
        set
        {
            if (value == _remoteDesktop_OverrideColorDepth)
                return;

            _remoteDesktop_OverrideColorDepth = value;
            OnPropertyChanged();
        }
    }

    public List<int> RemoteDesktop_ColorDepths => RemoteDesktop.ColorDepths;

    private int _remoteDesktop_SelectedColorDepth;

    public int RemoteDesktop_SelectedColorDepth
    {
        get => _remoteDesktop_SelectedColorDepth;
        set
        {
            if (value == _remoteDesktop_SelectedColorDepth)
                return;

            _remoteDesktop_SelectedColorDepth = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_OverridePort;

    public bool RemoteDesktop_OverridePort
    {
        get => _remoteDesktop_OverridePort;
        set
        {
            if (value == _remoteDesktop_OverridePort)
                return;

            _remoteDesktop_OverridePort = value;
            OnPropertyChanged();
        }
    }

    private int _remoteDesktop_Port;

    public int RemoteDesktop_Port
    {
        get => _remoteDesktop_Port;
        set
        {
            if (value == _remoteDesktop_Port)
                return;

            _remoteDesktop_Port = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_OverrideCredSspSupport;

    public bool RemoteDesktop_OverrideCredSspSupport
    {
        get => _remoteDesktop_OverrideCredSspSupport;
        set
        {
            if (value == _remoteDesktop_OverrideCredSspSupport)
                return;

            _remoteDesktop_OverrideCredSspSupport = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_EnableCredSspSupport;

    public bool RemoteDesktop_EnableCredSspSupport
    {
        get => _remoteDesktop_EnableCredSspSupport;
        set
        {
            if (value == _remoteDesktop_EnableCredSspSupport)
                return;

            _remoteDesktop_EnableCredSspSupport = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_OverrideAuthenticationLevel;

    public bool RemoteDesktop_OverrideAuthenticationLevel
    {
        get => _remoteDesktop_OverrideAuthenticationLevel;
        set
        {
            if (value == _remoteDesktop_OverrideAuthenticationLevel)
                return;

            _remoteDesktop_OverrideAuthenticationLevel = value;
            OnPropertyChanged();
        }
    }

    private uint _remoteDesktop_AuthenticationLevel;

    public uint RemoteDesktop_AuthenticationLevel
    {
        get => _remoteDesktop_AuthenticationLevel;
        set
        {
            if (value == _remoteDesktop_AuthenticationLevel)
                return;

            _remoteDesktop_AuthenticationLevel = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_OverrideGatewayServer;

    public bool RemoteDesktop_OverrideGatewayServer
    {
        get => _remoteDesktop_OverrideGatewayServer;
        set
        {
            if (value == _remoteDesktop_OverrideGatewayServer)
                return;

            _remoteDesktop_OverrideGatewayServer = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_EnableGatewayServer;

    public bool RemoteDesktop_EnableGatewayServer
    {
        get => _remoteDesktop_EnableGatewayServer;
        set
        {
            if (value == _remoteDesktop_EnableGatewayServer)
                return;

            _remoteDesktop_EnableGatewayServer = value;
            OnPropertyChanged();
        }
    }

    private string _remoteDesktop_GatewayServerHostname;

    public string RemoteDesktop_GatewayServerHostname
    {
        get => _remoteDesktop_GatewayServerHostname;
        set
        {
            if (value == _remoteDesktop_GatewayServerHostname)
                return;

            _remoteDesktop_GatewayServerHostname = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_GatewayServerBypassLocalAddresses;

    public bool RemoteDesktop_GatewayServerBypassLocalAddresses
    {
        get => _remoteDesktop_GatewayServerBypassLocalAddresses;
        set
        {
            if (value == _remoteDesktop_GatewayServerBypassLocalAddresses)
                return;

            _remoteDesktop_GatewayServerBypassLocalAddresses = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<GatewayUserSelectedCredsSource> RemoteDesktop_GatewayServerLogonMethods =>
        Enum.GetValues(typeof(GatewayUserSelectedCredsSource)).Cast<GatewayUserSelectedCredsSource>();

    private GatewayUserSelectedCredsSource _remoteDesktop_GatewayServerLogonMethod;

    public GatewayUserSelectedCredsSource RemoteDesktop_GatewayServerLogonMethod
    {
        get => _remoteDesktop_GatewayServerLogonMethod;
        set
        {
            if (Equals(value, _remoteDesktop_GatewayServerLogonMethod))
                return;

            _remoteDesktop_GatewayServerLogonMethod = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_GatewayServerShareCredentialsWithRemoteComputer;

    public bool RemoteDesktop_GatewayServerShareCredentialsWithRemoteComputer
    {
        get => _remoteDesktop_GatewayServerShareCredentialsWithRemoteComputer;
        set
        {
            if (value == _remoteDesktop_GatewayServerShareCredentialsWithRemoteComputer)
                return;

            _remoteDesktop_GatewayServerShareCredentialsWithRemoteComputer = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_UseGatewayServerCredentials;

    public bool RemoteDesktop_UseGatewayServerCredentials
    {
        get => _remoteDesktop_UseGatewayServerCredentials;
        set
        {
            if (value == _remoteDesktop_UseGatewayServerCredentials)
                return;

            _remoteDesktop_UseGatewayServerCredentials = value;
            OnPropertyChanged();
        }
    }

    private string _remoteDesktop_GatewayServerUsername;

    public string RemoteDesktop_GatewayServerUsername
    {
        get => _remoteDesktop_GatewayServerUsername;
        set
        {
            if (value == _remoteDesktop_GatewayServerUsername)
                return;

            _remoteDesktop_GatewayServerUsername = value;
            OnPropertyChanged();
        }
    }

    private string _remoteDesktop_GatewayServerDomain;

    public string RemoteDesktop_GatewayServerDomain
    {
        get => _remoteDesktop_GatewayServerDomain;
        set
        {
            if (value == _remoteDesktop_GatewayServerDomain)
                return;

            _remoteDesktop_GatewayServerDomain = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_IsGatewayServerPasswordEmpty = true; // Initial it's empty

    public bool RemoteDesktop_IsGatewayServerPasswordEmpty
    {
        get => _remoteDesktop_IsGatewayServerPasswordEmpty;
        set
        {
            if (value == _remoteDesktop_IsGatewayServerPasswordEmpty)
                return;

            _remoteDesktop_IsGatewayServerPasswordEmpty = value;
            OnPropertyChanged();
        }
    }

    private SecureString _remoteDesktop_GatewayServerPassword;

    public SecureString RemoteDesktop_GatewayServerPassword
    {
        get => _remoteDesktop_GatewayServerPassword;
        set
        {
            if (value == _remoteDesktop_GatewayServerPassword)
                return;

            // Validate the password string
            RemoteDesktop_IsGatewayServerPasswordEmpty =
                value == null || string.IsNullOrEmpty(SecureStringHelper.ConvertToString(value));

            _remoteDesktop_GatewayServerPassword = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_OverrideAudioRedirectionMode;

    public bool RemoteDesktop_OverrideAudioRedirectionMode
    {
        get => _remoteDesktop_OverrideAudioRedirectionMode;
        set
        {
            if (value == _remoteDesktop_OverrideAudioRedirectionMode)
                return;

            _remoteDesktop_OverrideAudioRedirectionMode = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<AudioRedirectionMode> RemoteDesktop_AudioRedirectionModes =>
        Enum.GetValues(typeof(AudioRedirectionMode)).Cast<AudioRedirectionMode>();

    private AudioRedirectionMode _remoteDesktop_AudioRedirectionMode;

    public AudioRedirectionMode RemoteDesktop_AudioRedirectionMode
    {
        get => _remoteDesktop_AudioRedirectionMode;
        set
        {
            if (Equals(value, _remoteDesktop_AudioRedirectionMode))
                return;

            _remoteDesktop_AudioRedirectionMode = value;
            OnPropertyChanged();
        }
    }


    private bool _remoteDesktop_OverrideAudioCaptureRedirectionMode;

    public bool RemoteDesktop_OverrideAudioCaptureRedirectionMode
    {
        get => _remoteDesktop_OverrideAudioCaptureRedirectionMode;
        set
        {
            if (value == _remoteDesktop_OverrideAudioCaptureRedirectionMode)
                return;

            _remoteDesktop_OverrideAudioCaptureRedirectionMode = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<AudioCaptureRedirectionMode> RemoteDesktop_AudioCaptureRedirectionModes =>
        Enum.GetValues(typeof(AudioCaptureRedirectionMode)).Cast<AudioCaptureRedirectionMode>();

    private AudioCaptureRedirectionMode _remoteDesktop_AudioCaptureRedirectionMode;

    public AudioCaptureRedirectionMode RemoteDesktop_AudioCaptureRedirectionMode
    {
        get => _remoteDesktop_AudioCaptureRedirectionMode;
        set
        {
            if (Equals(value, _remoteDesktop_AudioCaptureRedirectionMode))
                return;

            _remoteDesktop_AudioCaptureRedirectionMode = value;
            OnPropertyChanged();
        }
    }


    private bool _remoteDesktop_OverrideApplyWindowsKeyCombinations;

    public bool RemoteDesktop_OverrideApplyWindowsKeyCombinations
    {
        get => _remoteDesktop_OverrideApplyWindowsKeyCombinations;
        set
        {
            if (value == _remoteDesktop_OverrideApplyWindowsKeyCombinations)
                return;

            _remoteDesktop_OverrideApplyWindowsKeyCombinations = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<KeyboardHookMode> RemoteDesktop_KeyboardHookModes =>
        Enum.GetValues(typeof(KeyboardHookMode)).Cast<KeyboardHookMode>();

    private KeyboardHookMode _remoteDesktop_KeyboardHookMode;

    public KeyboardHookMode RemoteDesktop_KeyboardHookMode
    {
        get => _remoteDesktop_KeyboardHookMode;
        set
        {
            if (Equals(value, _remoteDesktop_KeyboardHookMode))
                return;

            _remoteDesktop_KeyboardHookMode = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_OverrideRedirectClipboard;

    public bool RemoteDesktop_OverrideRedirectClipboard
    {
        get => _remoteDesktop_OverrideRedirectClipboard;
        set
        {
            if (value == _remoteDesktop_OverrideRedirectClipboard)
                return;

            _remoteDesktop_OverrideRedirectClipboard = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_RedirectClipboard;

    public bool RemoteDesktop_RedirectClipboard
    {
        get => _remoteDesktop_RedirectClipboard;
        set
        {
            if (value == _remoteDesktop_RedirectClipboard)
                return;

            _remoteDesktop_RedirectClipboard = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_OverrideRedirectDevices;

    public bool RemoteDesktop_OverrideRedirectDevices
    {
        get => _remoteDesktop_OverrideRedirectDevices;
        set
        {
            if (value == _remoteDesktop_OverrideRedirectDevices)
                return;

            _remoteDesktop_OverrideRedirectDevices = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_RedirectDevices;

    public bool RemoteDesktop_RedirectDevices
    {
        get => _remoteDesktop_RedirectDevices;
        set
        {
            if (value == _remoteDesktop_RedirectDevices)
                return;

            _remoteDesktop_RedirectDevices = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_OverrideRedirectDrives;

    public bool RemoteDesktop_OverrideRedirectDrives
    {
        get => _remoteDesktop_OverrideRedirectDrives;
        set
        {
            if (value == _remoteDesktop_OverrideRedirectDrives)
                return;

            _remoteDesktop_OverrideRedirectDrives = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_RedirectDrives;

    public bool RemoteDesktop_RedirectDrives
    {
        get => _remoteDesktop_RedirectDrives;
        set
        {
            if (value == _remoteDesktop_RedirectDrives)
                return;

            _remoteDesktop_RedirectDrives = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_OverrideRedirectPorts;

    public bool RemoteDesktop_OverrideRedirectPorts
    {
        get => _remoteDesktop_OverrideRedirectPorts;
        set
        {
            if (value == _remoteDesktop_OverrideRedirectPorts)
                return;

            _remoteDesktop_OverrideRedirectPorts = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_RedirectPorts;

    public bool RemoteDesktop_RedirectPorts
    {
        get => _remoteDesktop_RedirectPorts;
        set
        {
            if (value == _remoteDesktop_RedirectPorts)
                return;

            _remoteDesktop_RedirectPorts = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_OverrideRedirectSmartcards;

    public bool RemoteDesktop_OverrideRedirectSmartcards
    {
        get => _remoteDesktop_OverrideRedirectSmartcards;
        set
        {
            if (value == _remoteDesktop_OverrideRedirectSmartcards)
                return;

            _remoteDesktop_OverrideRedirectSmartcards = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_RedirectSmartCards;

    public bool RemoteDesktop_RedirectSmartCards
    {
        get => _remoteDesktop_RedirectSmartCards;
        set
        {
            if (value == _remoteDesktop_RedirectSmartCards)
                return;

            _remoteDesktop_RedirectSmartCards = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_OverrideRedirectPrinters;

    public bool RemoteDesktop_OverrideRedirectPrinters
    {
        get => _remoteDesktop_OverrideRedirectPrinters;
        set
        {
            if (value == _remoteDesktop_OverrideRedirectPrinters)
                return;

            _remoteDesktop_OverrideRedirectPrinters = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_RedirectPrinters;

    public bool RemoteDesktop_RedirectPrinters
    {
        get => _remoteDesktop_RedirectPrinters;
        set
        {
            if (value == _remoteDesktop_RedirectPrinters)
                return;

            _remoteDesktop_RedirectPrinters = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_OverridePersistentBitmapCaching;

    public bool RemoteDesktop_OverridePersistentBitmapCaching
    {
        get => _remoteDesktop_OverridePersistentBitmapCaching;
        set
        {
            if (value == _remoteDesktop_OverridePersistentBitmapCaching)
                return;

            _remoteDesktop_OverridePersistentBitmapCaching = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_PersistentBitmapCaching;

    public bool RemoteDesktop_PersistentBitmapCaching
    {
        get => _remoteDesktop_PersistentBitmapCaching;
        set
        {
            if (value == _remoteDesktop_PersistentBitmapCaching)
                return;

            _remoteDesktop_PersistentBitmapCaching = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_OverrideReconnectIfTheConnectionIsDropped;

    public bool RemoteDesktop_OverrideReconnectIfTheConnectionIsDropped
    {
        get => _remoteDesktop_OverrideReconnectIfTheConnectionIsDropped;
        set
        {
            if (value == _remoteDesktop_OverrideReconnectIfTheConnectionIsDropped)
                return;

            _remoteDesktop_OverrideReconnectIfTheConnectionIsDropped = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_ReconnectIfTheConnectionIsDropped;

    public bool RemoteDesktop_ReconnectIfTheConnectionIsDropped
    {
        get => _remoteDesktop_ReconnectIfTheConnectionIsDropped;
        set
        {
            if (value == _remoteDesktop_ReconnectIfTheConnectionIsDropped)
                return;

            _remoteDesktop_ReconnectIfTheConnectionIsDropped = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_OverrideNetworkConnectionType;

    public bool RemoteDesktop_OverrideNetworkConnectionType
    {
        get => _remoteDesktop_OverrideNetworkConnectionType;
        set
        {
            if (value == _remoteDesktop_OverrideNetworkConnectionType)
                return;

            _remoteDesktop_OverrideNetworkConnectionType = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<NetworkConnectionType> RemoteDesktop_NetworkConnectionTypes =>
        Enum.GetValues(typeof(NetworkConnectionType)).Cast<NetworkConnectionType>();

    private NetworkConnectionType _remoteDesktop_NetworkConnectionType;

    public NetworkConnectionType RemoteDesktop_NetworkConnectionType
    {
        get => _remoteDesktop_NetworkConnectionType;
        set
        {
            if (Equals(value, _remoteDesktop_NetworkConnectionType))
                return;

            if (!_isLoading)
                ChangeNetworkConnectionTypeSettings(value);

            _remoteDesktop_NetworkConnectionType = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_DesktopBackground;

    public bool RemoteDesktop_DesktopBackground
    {
        get => _remoteDesktop_DesktopBackground;
        set
        {
            if (value == _remoteDesktop_DesktopBackground)
                return;

            _remoteDesktop_DesktopBackground = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_FontSmoothing;

    public bool RemoteDesktop_FontSmoothing
    {
        get => _remoteDesktop_FontSmoothing;
        set
        {
            if (value == _remoteDesktop_FontSmoothing)
                return;

            _remoteDesktop_FontSmoothing = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_DesktopComposition;

    public bool RemoteDesktop_DesktopComposition
    {
        get => _remoteDesktop_DesktopComposition;
        set
        {
            if (value == _remoteDesktop_DesktopComposition)
                return;

            _remoteDesktop_DesktopComposition = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_ShowWindowContentsWhileDragging;

    public bool RemoteDesktop_ShowWindowContentsWhileDragging
    {
        get => _remoteDesktop_ShowWindowContentsWhileDragging;
        set
        {
            if (value == _remoteDesktop_ShowWindowContentsWhileDragging)
                return;

            _remoteDesktop_ShowWindowContentsWhileDragging = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_MenuAndWindowAnimation;

    public bool RemoteDesktop_MenuAndWindowAnimation
    {
        get => _remoteDesktop_MenuAndWindowAnimation;
        set
        {
            if (value == _remoteDesktop_MenuAndWindowAnimation)
                return;

            _remoteDesktop_MenuAndWindowAnimation = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_VisualStyles;

    public bool RemoteDesktop_VisualStyles
    {
        get => _remoteDesktop_VisualStyles;
        set
        {
            if (value == _remoteDesktop_VisualStyles)
                return;

            _remoteDesktop_VisualStyles = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region PowerShell

    private bool _powerShell_Enabled;

    public bool PowerShell_Enabled
    {
        get => _powerShell_Enabled;
        set
        {
            if (value == _powerShell_Enabled)
                return;

            _powerShell_Enabled = value;

            OnPropertyChanged();
        }
    }

    private bool _powerShell_EnableRemoteConsole;

    public bool PowerShell_EnableRemoteConsole
    {
        get => _powerShell_EnableRemoteConsole;
        set
        {
            if (value == _powerShell_EnableRemoteConsole)
                return;

            _powerShell_EnableRemoteConsole = value;
            OnPropertyChanged();
        }
    }

    private bool _powerShell_InheritHost;

    public bool PowerShell_InheritHost
    {
        get => _powerShell_InheritHost;
        set
        {
            if (value == _powerShell_InheritHost)
                return;

            _powerShell_InheritHost = value;
            OnPropertyChanged();
        }
    }

    private string _powerShell_Host;

    public string PowerShell_Host
    {
        get => _powerShell_Host;
        set
        {
            if (value == _powerShell_Host)
                return;

            _powerShell_Host = value;
            OnPropertyChanged();
        }
    }

    private bool _powerShell_OverrideCommand;

    public bool PowerShell_OverrideCommand
    {
        get => _powerShell_OverrideCommand;
        set
        {
            if (value == _powerShell_OverrideCommand)
                return;

            _powerShell_OverrideCommand = value;
            OnPropertyChanged();
        }
    }

    private string _powerShell_Command;

    public string PowerShell_Command
    {
        get => _powerShell_Command;
        set
        {
            if (value == _powerShell_Command)
                return;

            _powerShell_Command = value;
            OnPropertyChanged();
        }
    }

    private bool _powerShell_OverrideAdditionalCommandLine;

    public bool PowerShell_OverrideAdditionalCommandLine
    {
        get => _powerShell_OverrideAdditionalCommandLine;
        set
        {
            if (value == _powerShell_OverrideAdditionalCommandLine)
                return;

            _powerShell_OverrideAdditionalCommandLine = value;
            OnPropertyChanged();
        }
    }

    private string _powerShell_AdditionalCommandLine;

    public string PowerShell_AdditionalCommandLine
    {
        get => _powerShell_AdditionalCommandLine;
        set
        {
            if (value == _powerShell_AdditionalCommandLine)
                return;

            _powerShell_AdditionalCommandLine = value;
            OnPropertyChanged();
        }
    }

    private readonly List<ExecutionPolicy> _powerShell_ExecutionPolicies = new();

    public List<ExecutionPolicy> PowerShell_ExecutionPolicies
    {
        get => _powerShell_ExecutionPolicies;
        private init
        {
            if (value == _powerShell_ExecutionPolicies)
                return;

            _powerShell_ExecutionPolicies = value;
            OnPropertyChanged();
        }
    }

    private bool _powerShell_OverrideExecutionPolicy;

    public bool PowerShell_OverrideExecutionPolicy
    {
        get => _powerShell_OverrideExecutionPolicy;
        set
        {
            if (value == _powerShell_OverrideExecutionPolicy)
                return;

            _powerShell_OverrideExecutionPolicy = value;
            OnPropertyChanged();
        }
    }

    private ExecutionPolicy _powerShell_ExecutionPolicy;

    public ExecutionPolicy PowerShell_ExecutionPolicy
    {
        get => _powerShell_ExecutionPolicy;
        set
        {
            if (value == _powerShell_ExecutionPolicy)
                return;

            _powerShell_ExecutionPolicy = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region PuTTY

    private bool _puTTY_Enabled;

    public bool PuTTY_Enabled
    {
        get => _puTTY_Enabled;
        set
        {
            if (value == _puTTY_Enabled)
                return;

            _puTTY_Enabled = value;

            OnPropertyChanged();
        }
    }

    private bool _puTTY_InheritHost;

    public bool PuTTY_InheritHost
    {
        get => _puTTY_InheritHost;
        set
        {
            if (value == _puTTY_InheritHost)
                return;

            _puTTY_InheritHost = value;
            OnPropertyChanged();
        }
    }

    private bool _puTTY_UseSSH; // Default is SSH

    public bool PuTTY_UseSSH
    {
        get => _puTTY_UseSSH;
        set
        {
            if (value == _puTTY_UseSSH)
                return;

            if (value)
            {
                PuTTY_Port = SettingsManager.Current.PuTTY_SSHPort;
                PuTTY_ConnectionMode = ConnectionMode.SSH;
            }

            _puTTY_UseSSH = value;
            OnPropertyChanged();
        }
    }

    private bool _puTTY_UseTelnet;

    public bool PuTTY_UseTelnet
    {
        get => _puTTY_UseTelnet;
        set
        {
            if (value == _puTTY_UseTelnet)
                return;

            if (value)
            {
                PuTTY_Port = SettingsManager.Current.PuTTY_TelnetPort;
                PuTTY_ConnectionMode = ConnectionMode.Telnet;
            }

            _puTTY_UseTelnet = value;
            OnPropertyChanged();
        }
    }

    private bool _puTTY_UseSerial;

    public bool PuTTY_UseSerial
    {
        get => _puTTY_UseSerial;
        set
        {
            if (value == _puTTY_UseSerial)
                return;

            if (value)
            {
                PuTTY_Baud = SettingsManager.Current.PuTTY_BaudRate;
                PuTTY_ConnectionMode = ConnectionMode.Serial;
            }

            _puTTY_UseSerial = value;
            OnPropertyChanged();
        }
    }

    private bool _puTTY_UseRlogin;

    public bool PuTTY_UseRlogin
    {
        get => _puTTY_UseRlogin;
        set
        {
            if (value == _puTTY_UseRlogin)
                return;

            if (value)
            {
                PuTTY_Port = SettingsManager.Current.PuTTY_RloginPort;
                PuTTY_ConnectionMode = ConnectionMode.Rlogin;
            }

            _puTTY_UseRlogin = value;
            OnPropertyChanged();
        }
    }

    private bool _puTTY_UseRAW;

    public bool PuTTY_UseRAW
    {
        get => _puTTY_UseRAW;
        set
        {
            if (value == _puTTY_UseRAW)
                return;

            if (value)
            {
                PuTTY_Port = SettingsManager.Current.PuTTY_RawPort;
                PuTTY_ConnectionMode = ConnectionMode.RAW;
            }

            _puTTY_UseRAW = value;
            OnPropertyChanged();
        }
    }

    private string _puTTY_Host;

    public string PuTTY_Host
    {
        get => _puTTY_Host;
        set
        {
            if (value == _puTTY_Host)
                return;

            _puTTY_Host = value;
            OnPropertyChanged();
        }
    }

    private string _puTTY_SerialLine;

    public string PuTTY_SerialLine
    {
        get => _puTTY_SerialLine;
        set
        {
            if (value == _puTTY_SerialLine)
                return;

            _puTTY_SerialLine = value;
            OnPropertyChanged();
        }
    }

    private bool _puTTY_OverridePortOrBaud;

    public bool PuTTY_OverridePortOrBaud
    {
        get => _puTTY_OverridePortOrBaud;
        set
        {
            if (value == _puTTY_OverridePortOrBaud)
                return;

            _puTTY_OverridePortOrBaud = value;
            OnPropertyChanged();
        }
    }

    private int _puTTY_Port;

    public int PuTTY_Port
    {
        get => _puTTY_Port;
        set
        {
            if (value == _puTTY_Port)
                return;

            _puTTY_Port = value;
            OnPropertyChanged();
        }
    }

    private int _puTTY_Baud;

    public int PuTTY_Baud
    {
        get => _puTTY_Baud;
        set
        {
            if (value == _puTTY_Baud)
                return;

            _puTTY_Baud = value;
            OnPropertyChanged();
        }
    }

    private bool _puTTY_OverrideUsername;

    public bool PuTTY_OverrideUsername
    {
        get => _puTTY_OverrideUsername;
        set
        {
            if (value == _puTTY_OverrideUsername)
                return;

            _puTTY_OverrideUsername = value;
            OnPropertyChanged();
        }
    }

    private string _puTTY__Username;

    public string PuTTY_Username
    {
        get => _puTTY__Username;
        set
        {
            if (value == _puTTY__Username)
                return;

            _puTTY__Username = value;
            OnPropertyChanged();
        }
    }

    private bool _puTTY_OverridePrivateKeyFile;

    public bool PuTTY_OverridePrivateKeyFile
    {
        get => _puTTY_OverridePrivateKeyFile;
        set
        {
            if (value == _puTTY_OverridePrivateKeyFile)
                return;

            _puTTY_OverridePrivateKeyFile = value;
            OnPropertyChanged();
        }
    }

    private string _puTTY__PrivateKeyFile;

    public string PuTTY_PrivateKeyFile
    {
        get => _puTTY__PrivateKeyFile;
        set
        {
            if (value == _puTTY__PrivateKeyFile)
                return;

            _puTTY__PrivateKeyFile = value;
            OnPropertyChanged();
        }
    }

    private bool _puTTY_OverrideProfile;

    public bool PuTTY_OverrideProfile
    {
        get => _puTTY_OverrideProfile;
        set
        {
            if (value == _puTTY_OverrideProfile)
                return;

            _puTTY_OverrideProfile = value;
            OnPropertyChanged();
        }
    }

    private string _puTTY_Profile;

    public string PuTTY_Profile
    {
        get => _puTTY_Profile;
        set
        {
            if (value == _puTTY_Profile)
                return;

            _puTTY_Profile = value;
            OnPropertyChanged();
        }
    }

    private bool _puTTY_OverrideHostkey;

    public bool PuTTY_OverrideHostkey
    {
        get => _puTTY_OverrideHostkey;
        set
        {
            if (value == _puTTY_OverrideHostkey)
                return;

            _puTTY_OverrideHostkey = value;
            OnPropertyChanged();
        }
    }

    private string _puTTY_Hostkey;

    public string PuTTY_Hostkey
    {
        get => _puTTY_Hostkey;
        set
        {
            if (value == _puTTY_Hostkey)
                return;

            _puTTY_Hostkey = value;
            OnPropertyChanged();
        }
    }

    private bool _puTTY_OverrideEnableLog;

    public bool PuTTY_OverrideEnableLog
    {
        get => _puTTY_OverrideEnableLog;
        set
        {
            if (value == _puTTY_OverrideEnableLog)
                return;

            _puTTY_OverrideEnableLog = value;
            OnPropertyChanged();
        }
    }

    private bool _puTTY_EnableLog;

    public bool PuTTY_EnableLog
    {
        get => _puTTY_EnableLog;
        set
        {
            if (value == _puTTY_EnableLog)
                return;

            _puTTY_EnableLog = value;
            OnPropertyChanged();
        }
    }

    private bool _puTTY_OverrideLogMode;

    public bool PuTTY_OverrideLogMode
    {
        get => _puTTY_OverrideLogMode;
        set
        {
            if (value == _puTTY_OverrideLogMode)
                return;

            _puTTY_OverrideLogMode = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<LogMode> PuTTY_LogModes => Enum.GetValues(typeof(LogMode)).Cast<LogMode>();

    private LogMode _puTTY_LogMode;

    public LogMode PuTTY_LogMode
    {
        get => _puTTY_LogMode;
        set
        {
            if (Equals(value, _puTTY_LogMode))
                return;

            _puTTY_LogMode = value;
            OnPropertyChanged();
        }
    }

    private bool _puTTY_OverrideLogPath;

    public bool PuTTY_OverrideLogPath
    {
        get => _puTTY_OverrideLogPath;
        set
        {
            if (value == _puTTY_OverrideLogPath)
                return;

            _puTTY_OverrideLogPath = value;
            OnPropertyChanged();
        }
    }

    private string _puTTY_LogPath;

    public string PuTTY_LogPath
    {
        get => _puTTY_LogPath;
        set
        {
            if (value == _puTTY_LogPath)
                return;

            _puTTY_LogPath = value;
            OnPropertyChanged();
        }
    }

    private bool _puTTY_OverrideLogFileName;

    public bool PuTTY_OverrideLogFileName
    {
        get => _puTTY_OverrideLogFileName;
        set
        {
            if (value == _puTTY_OverrideLogFileName)
                return;

            _puTTY_OverrideLogFileName = value;
            OnPropertyChanged();
        }
    }

    private string _puTTY_LogFileName;

    public string PuTTY_LogFileName
    {
        get => _puTTY_LogFileName;
        set
        {
            if (value == _puTTY_LogFileName)
                return;

            _puTTY_LogFileName = value;
            OnPropertyChanged();
        }
    }

    private bool _puTTY_OverrideAdditionalCommandLine;

    public bool PuTTY_OverrideAdditionalCommandLine
    {
        get => _puTTY_OverrideAdditionalCommandLine;
        set
        {
            if (value == _puTTY_OverrideAdditionalCommandLine)
                return;

            _puTTY_OverrideAdditionalCommandLine = value;
            OnPropertyChanged();
        }
    }

    private string _puTTY_AdditionalCommandLine;

    public string PuTTY_AdditionalCommandLine
    {
        get => _puTTY_AdditionalCommandLine;
        set
        {
            if (value == _puTTY_AdditionalCommandLine)
                return;

            _puTTY_AdditionalCommandLine = value;
            OnPropertyChanged();
        }
    }

    private ConnectionMode _puTTY_ConnectionMode;

    public ConnectionMode PuTTY_ConnectionMode
    {
        get => _puTTY_ConnectionMode;
        private set
        {
            if (value == _puTTY_ConnectionMode)
                return;

            _puTTY_ConnectionMode = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region AWS Session Manager

    private bool _awsSessionManager_Enabled;

    public bool AWSSessionManager_Enabled
    {
        get => _awsSessionManager_Enabled;
        set
        {
            if (value == _awsSessionManager_Enabled)
                return;

            _awsSessionManager_Enabled = value;
            OnPropertyChanged();
        }
    }

    private string _awsSessionManager_InstanceID;

    public string AWSSessionManager_InstanceID
    {
        get => _awsSessionManager_InstanceID;
        set
        {
            if (value == _awsSessionManager_InstanceID)
                return;

            _awsSessionManager_InstanceID = value;
            OnPropertyChanged();
        }
    }

    private bool _awsSessionManager_OverrideProfile;

    public bool AWSSessionManager_OverrideProfile
    {
        get => _awsSessionManager_OverrideProfile;
        set
        {
            if (value == _awsSessionManager_OverrideProfile)
                return;

            _awsSessionManager_OverrideProfile = value;
            OnPropertyChanged();
        }
    }

    private string _awsSessionManager_Profile;

    public string AWSSessionManager_Profile
    {
        get => _awsSessionManager_Profile;
        set
        {
            if (value == _awsSessionManager_Profile)
                return;

            _awsSessionManager_Profile = value;
            OnPropertyChanged();
        }
    }

    private bool _awsSessionManager_OverrideRegion;

    public bool AWSSessionManager_OverrideRegion
    {
        get => _awsSessionManager_OverrideRegion;
        set
        {
            if (value == _awsSessionManager_OverrideRegion)
                return;

            _awsSessionManager_OverrideRegion = value;
            OnPropertyChanged();
        }
    }

    private string _awsSessionManager_Region;

    public string AWSSessionManager_Region
    {
        get => _awsSessionManager_Region;
        set
        {
            if (value == _awsSessionManager_Region)
                return;

            _awsSessionManager_Region = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region TigerVNC

    private bool _tigerVNC_Enabled;

    public bool TigerVNC_Enabled
    {
        get => _tigerVNC_Enabled;
        set
        {
            if (value == _tigerVNC_Enabled)
                return;

            _tigerVNC_Enabled = value;
            OnPropertyChanged();
        }
    }

    private bool _tigerVNC_InheritHost;

    public bool TigerVNC_InheritHost
    {
        get => _tigerVNC_InheritHost;
        set
        {
            if (value == _tigerVNC_InheritHost)
                return;

            _tigerVNC_InheritHost = value;
            OnPropertyChanged();
        }
    }

    private string _tigerVNC_Host;

    public string TigerVNC_Host
    {
        get => _tigerVNC_Host;
        set
        {
            if (value == _tigerVNC_Host)
                return;

            _tigerVNC_Host = value;
            OnPropertyChanged();
        }
    }


    private bool _tigerVNC_OverridePort;

    public bool TigerVNC_OverridePort
    {
        get => _tigerVNC_OverridePort;
        set
        {
            if (value == _tigerVNC_OverridePort)
                return;

            _tigerVNC_OverridePort = value;
            OnPropertyChanged();
        }
    }

    private int _tigerVNC_Port;

    public int TigerVNC_Port
    {
        get => _tigerVNC_Port;
        set
        {
            if (value == _tigerVNC_Port)
                return;

            _tigerVNC_Port = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Web Console

    private bool _webConsole_Enabled;

    public bool WebConsole_Enabled
    {
        get => _webConsole_Enabled;
        set
        {
            if (value == _webConsole_Enabled)
                return;

            _webConsole_Enabled = value;
            OnPropertyChanged();
        }
    }

    private string _webConsole_Url;

    public string WebConsole_Url
    {
        get => _webConsole_Url;
        set
        {
            if (value == _webConsole_Url)
                return;

            _webConsole_Url = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region SNMP

    private bool _snmp_Enabled;

    public bool SNMP_Enabled
    {
        get => _snmp_Enabled;
        set
        {
            if (value == _snmp_Enabled)
                return;

            _snmp_Enabled = value;

            OnPropertyChanged();
        }
    }

    private bool _snmp_InheritHost;

    public bool SNMP_InheritHost
    {
        get => _snmp_InheritHost;
        set
        {
            if (value == _snmp_InheritHost)
                return;

            _snmp_InheritHost = value;
            OnPropertyChanged();
        }
    }

    private string _snmp_Host;

    public string SNMP_Host
    {
        get => _snmp_Host;
        set
        {
            if (value == _snmp_Host)
                return;

            _snmp_Host = value;
            OnPropertyChanged();
        }
    }

    private bool _snmp_OverrideOIDAndMode;

    public bool SNMP_OverrideOIDAndMode
    {
        get => _snmp_OverrideOIDAndMode;
        set
        {
            if (value == _snmp_OverrideOIDAndMode)
                return;

            _snmp_OverrideOIDAndMode = value;
            OnPropertyChanged();
        }
    }

    private string _snmp_OID;

    public string SNMP_OID
    {
        get => _snmp_OID;
        set
        {
            if (value == _snmp_OID)
                return;

            _snmp_OID = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<SNMPMode> SNMP_Modes { get; set; }

    private SNMPMode _snmp_Mode;

    public SNMPMode SNMP_Mode
    {
        get => _snmp_Mode;
        set
        {
            if (value == _snmp_Mode)
                return;

            _snmp_Mode = value;
            OnPropertyChanged();

            // Re-validate OID if mode changed
            OnPropertyChanged(nameof(SNMP_OID));
        }
    }

    private bool _snmp_OverrideVersionAndAuth;

    public bool SNMP_OverrideVersionAndAuth
    {
        get => _snmp_OverrideVersionAndAuth;
        set
        {
            if (value == _snmp_OverrideVersionAndAuth)
                return;

            _snmp_OverrideVersionAndAuth = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<SNMPVersion> SNMP_Versions { get; }

    private SNMPVersion _snmp_Version;

    public SNMPVersion SNMP_Version
    {
        get => _snmp_Version;
        set
        {
            if (value == _snmp_Version)
                return;

            _snmp_Version = value;
            OnPropertyChanged();
        }
    }

    private bool _snmp_IsCommunityEmpty = true; // Initial it's empty

    public bool SNMP_IsCommunityEmpty
    {
        get => _snmp_IsCommunityEmpty;
        set
        {
            if (value == _snmp_IsCommunityEmpty)
                return;

            _snmp_IsCommunityEmpty = value;
            OnPropertyChanged();
        }
    }

    private SecureString _snmp_Community;

    public SecureString SNMP_Community
    {
        get => _snmp_Community;
        set
        {
            if (value == _snmp_Community)
                return;

            // Validate the password string
            SNMP_IsCommunityEmpty = value == null || string.IsNullOrEmpty(SecureStringHelper.ConvertToString(value));

            _snmp_Community = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<SNMPV3Security> SNMP_Securities { get; }

    private SNMPV3Security _snmp_Security;

    public SNMPV3Security SNMP_Security
    {
        get => _snmp_Security;
        set
        {
            if (value == _snmp_Security)
                return;

            _snmp_Security = value;
            OnPropertyChanged();
        }
    }

    private string _snmp_Username;

    public string SNMP_Username
    {
        get => _snmp_Username;
        set
        {
            if (value == _snmp_Username)
                return;

            _snmp_Username = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<SNMPV3AuthenticationProvider> SNMP_AuthenticationProviders { get; }

    private SNMPV3AuthenticationProvider _snmp_AuthenticationProvider;

    public SNMPV3AuthenticationProvider SNMP_AuthenticationProvider
    {
        get => _snmp_AuthenticationProvider;
        set
        {
            if (value == _snmp_AuthenticationProvider)
                return;

            _snmp_AuthenticationProvider = value;
            OnPropertyChanged();
        }
    }

    private bool _snmp_IsAuthEmpty = true; // Initial it's empty

    public bool SNMP_IsAuthEmpty
    {
        get => _snmp_IsAuthEmpty;
        set
        {
            if (value == _snmp_IsAuthEmpty)
                return;

            _snmp_IsAuthEmpty = value;
            OnPropertyChanged();
        }
    }

    private SecureString _snmp_Auth;

    public SecureString SNMP_Auth
    {
        get => _snmp_Auth;
        set
        {
            if (value == _snmp_Auth)
                return;

            // Validate the password string
            SNMP_IsAuthEmpty = value == null || string.IsNullOrEmpty(SecureStringHelper.ConvertToString(value));

            _snmp_Auth = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<SNMPV3PrivacyProvider> SNMP_PrivacyProviders { get; }

    private SNMPV3PrivacyProvider _snmp_PrivacyProvider;

    public SNMPV3PrivacyProvider SNMP_PrivacyProvider
    {
        get => _snmp_PrivacyProvider;
        set
        {
            if (value == _snmp_PrivacyProvider)
                return;

            _snmp_PrivacyProvider = value;
            OnPropertyChanged();
        }
    }

    private bool _snmp_IsPrivEmpty = true; // Initial it's empty

    public bool SNMP_IsPrivEmpty
    {
        get => _snmp_IsPrivEmpty;
        set
        {
            if (value == _snmp_IsPrivEmpty)
                return;

            _snmp_IsPrivEmpty = value;
            OnPropertyChanged();
        }
    }

    private SecureString _snmp_Priv;

    public SecureString SNMP_Priv
    {
        get => _snmp_Priv;
        set
        {
            if (value == _snmp_Priv)
                return;

            // Validate the password string
            SNMP_IsPrivEmpty = value == null || string.IsNullOrEmpty(SecureStringHelper.ConvertToString(value));

            _snmp_Priv = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Wake on LAN

    private bool _wakeOnLAN_Enabled;

    public bool WakeOnLAN_Enabled
    {
        get => _wakeOnLAN_Enabled;
        set
        {
            if (value == _wakeOnLAN_Enabled)
                return;

            _wakeOnLAN_Enabled = value;

            OnPropertyChanged();
        }
    }

    private string _wakeOnLAN_MACAddress;

    public string WakeOnLAN_MACAddress
    {
        get => _wakeOnLAN_MACAddress;
        set
        {
            if (value == _wakeOnLAN_MACAddress)
                return;

            _wakeOnLAN_MACAddress = value;
            OnPropertyChanged();
        }
    }

    private string _wakeOnLAN_Broadcast;

    public string WakeOnLAN_Broadcast
    {
        get => _wakeOnLAN_Broadcast;
        set
        {
            if (value == _wakeOnLAN_Broadcast)
                return;

            _wakeOnLAN_Broadcast = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Whois

    private bool _whois_Enabled;

    public bool Whois_Enabled
    {
        get => _whois_Enabled;
        set
        {
            if (value == _whois_Enabled)
                return;

            _whois_Enabled = value;

            OnPropertyChanged();
        }
    }

    private bool _whois_InheritHost;

    public bool Whois_InheritHost
    {
        get => _whois_InheritHost;
        set
        {
            if (value == _whois_InheritHost)
                return;

            _whois_InheritHost = value;
            OnPropertyChanged();
        }
    }

    private string _whois_Domain;

    public string Whois_Domain
    {
        get => _whois_Domain;
        set
        {
            if (value == _whois_Domain)
                return;

            _whois_Domain = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region IP Geolocation

    private bool _ipGeolocation_Enabled;

    public bool IPGeolocation_Enabled
    {
        get => _ipGeolocation_Enabled;
        set
        {
            if (value == _ipGeolocation_Enabled)
                return;

            _ipGeolocation_Enabled = value;

            OnPropertyChanged();
        }
    }

    private bool _ipGeolocation_InheritHost;

    public bool IPGeolocation_InheritHost
    {
        get => _ipGeolocation_InheritHost;
        set
        {
            if (value == _ipGeolocation_InheritHost)
                return;

            _ipGeolocation_InheritHost = value;
            OnPropertyChanged();
        }
    }

    private string _ipGeolocation_Host;

    public string IPGeolocation_Host
    {
        get => _ipGeolocation_Host;
        set
        {
            if (value == _ipGeolocation_Host)
                return;

            _ipGeolocation_Host = value;
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

        var dnsResult =
            await DNSClientHelper.ResolveAorAaaaAsync(Host, SettingsManager.Current.Network_ResolveHostnamePreferIPv4);

        if (!dnsResult.HasError)
            Host = dnsResult.Value.ToString();
        else
            ShowCouldNotResolveHostnameWarning = true;

        IsResolveHostnameRunning = false;
    }

    #endregion
}