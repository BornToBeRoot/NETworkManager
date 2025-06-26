﻿using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Localization.Resources;
using NETworkManager.Models;
using NETworkManager.Models.Network;
using NETworkManager.Models.PuTTY;
using NETworkManager.Models.RemoteDesktop;
using NETworkManager.Profiles;
using NETworkManager.ViewModels;
using NETworkManager.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.SimpleChildWindow;

namespace NETworkManager;

public static class ProfileDialogManager
{
    #region Variables

    private static string DialogResourceKey => "LargeMetroDialog";

    #endregion

    #region Methods to add and remove profile

    private static ProfileInfo ParseProfileInfo(ProfileViewModel instance)
    {
        return new ProfileInfo
        {
            Name = instance.Name.Trim(),
            Host = instance.Host.Trim(),
            Description = instance.Description?.Trim(),
            Group = instance.Group.Trim(),
            Tags = instance.Tags?.Trim(),

            // Network Interface
            NetworkInterface_Enabled = instance.NetworkInterface_Enabled,
            NetworkInterface_EnableStaticIPAddress = instance.NetworkInterface_EnableStaticIPAddress,
            NetworkInterface_IPAddress = instance.NetworkInterface_IPAddress?.Trim(),
            NetworkInterface_Gateway = instance.NetworkInterface_Gateway?.Trim(),
            NetworkInterface_Subnetmask = instance.NetworkInterface_Subnetmask?.Trim(),
            NetworkInterface_EnableStaticDNS = instance.NetworkInterface_EnableStaticDNS,
            NetworkInterface_PrimaryDNSServer = instance.NetworkInterface_PrimaryDNSServer?.Trim(),
            NetworkInterface_SecondaryDNSServer = instance.NetworkInterface_SecondaryDNSServer?.Trim(),

            // IP Scanner
            IPScanner_Enabled = instance.IPScanner_Enabled,
            IPScanner_InheritHost = instance.IPScanner_InheritHost,
            IPScanner_HostOrIPRange = instance.IPScanner_InheritHost
                ? instance.Host?.Trim()
                : instance.IPScanner_HostOrIPRange?.Trim(),

            // Port Scanner
            PortScanner_Enabled = instance.PortScanner_Enabled,
            PortScanner_InheritHost = instance.PortScanner_InheritHost,
            PortScanner_Host = instance.PortScanner_InheritHost
                ? instance.Host?.Trim()
                : instance.PortScanner_Host?.Trim(),
            PortScanner_Ports = instance.PortScanner_Ports?.Trim(),

            // Ping Monitor
            PingMonitor_Enabled = instance.PingMonitor_Enabled,
            PingMonitor_InheritHost = instance.PingMonitor_InheritHost,
            PingMonitor_Host = instance.PingMonitor_InheritHost
                ? instance.Host?.Trim()
                : instance.PingMonitor_Host?.Trim(),

            // Traceroute
            Traceroute_Enabled = instance.Traceroute_Enabled,
            Traceroute_InheritHost = instance.Traceroute_InheritHost,
            Traceroute_Host =
                instance.Traceroute_InheritHost ? instance.Host?.Trim() : instance.Traceroute_Host?.Trim(),

            // DNS Lookup
            DNSLookup_Enabled = instance.DNSLookup_Enabled,
            DNSLookup_InheritHost = instance.Traceroute_InheritHost,
            DNSLookup_Host = instance.DNSLookup_InheritHost ? instance.Host?.Trim() : instance.DNSLookup_Host?.Trim(),

            // Remote Desktop
            RemoteDesktop_Enabled = instance.RemoteDesktop_Enabled,
            RemoteDesktop_InheritHost = instance.RemoteDesktop_InheritHost,
            RemoteDesktop_Host = instance.RemoteDesktop_InheritHost
                ? instance.Host?.Trim()
                : instance.RemoteDesktop_Host?.Trim(),
            RemoteDesktop_UseCredentials = instance.RemoteDesktop_UseCredentials,
            RemoteDesktop_Username =
                instance.RemoteDesktop_UseCredentials
                    ? instance.RemoteDesktop_Username
                    : string.Empty, // Remove sensitive info on disable
            RemoteDesktop_Domain =
                instance.RemoteDesktop_UseCredentials
                    ? instance.RemoteDesktop_Domain
                    : string.Empty, // Remove sensitive info on disable
            RemoteDesktop_Password = instance.RemoteDesktop_UseCredentials
                ? instance.RemoteDesktop_Password
                : new SecureString(), // Remove sensitive info on disable
            RemoteDesktop_OverrideDisplay = instance.RemoteDesktop_OverrideDisplay,
            RemoteDesktop_AdjustScreenAutomatically = instance.RemoteDesktop_AdjustScreenAutomatically,
            RemoteDesktop_UseCurrentViewSize = instance.RemoteDesktop_UseCurrentViewSize,
            RemoteDesktop_UseFixedScreenSize = instance.RemoteDesktop_UseFixedScreenSize,
            RemoteDesktop_ScreenWidth = instance.RemoteDesktop_ScreenWidth,
            RemoteDesktop_ScreenHeight = instance.RemoteDesktop_ScreenHeight,
            RemoteDesktop_UseCustomScreenSize = instance.RemoteDesktop_UseCustomScreenSize,
            RemoteDesktop_CustomScreenWidth = int.Parse(instance.RemoteDesktop_CustomScreenWidth),
            RemoteDesktop_CustomScreenHeight = int.Parse(instance.RemoteDesktop_CustomScreenHeight),
            RemoteDesktop_OverrideColorDepth = instance.RemoteDesktop_OverrideColorDepth,
            RemoteDesktop_ColorDepth = instance.RemoteDesktop_SelectedColorDepth,
            RemoteDesktop_OverridePort = instance.RemoteDesktop_OverridePort,
            RemoteDesktop_Port = instance.RemoteDesktop_Port,
            RemoteDesktop_OverrideCredSspSupport = instance.RemoteDesktop_OverrideCredSspSupport,
            RemoteDesktop_EnableCredSspSupport = instance.RemoteDesktop_EnableCredSspSupport,
            RemoteDesktop_OverrideAuthenticationLevel = instance.RemoteDesktop_OverrideAuthenticationLevel,
            RemoteDesktop_AuthenticationLevel = instance.RemoteDesktop_AuthenticationLevel,
            RemoteDesktop_OverrideGatewayServer = instance.RemoteDesktop_OverrideGatewayServer,
            RemoteDesktop_EnableGatewayServer = instance.RemoteDesktop_EnableGatewayServer,
            RemoteDesktop_GatewayServerHostname = instance.RemoteDesktop_GatewayServerHostname,
            RemoteDesktop_GatewayServerBypassLocalAddresses = instance.RemoteDesktop_GatewayServerBypassLocalAddresses,
            RemoteDesktop_GatewayServerLogonMethod = instance.RemoteDesktop_GatewayServerLogonMethod,
            RemoteDesktop_GatewayServerShareCredentialsWithRemoteComputer =
                instance.RemoteDesktop_GatewayServerShareCredentialsWithRemoteComputer,
            RemoteDesktop_UseGatewayServerCredentials = instance.RemoteDesktop_UseGatewayServerCredentials,
            RemoteDesktop_GatewayServerUsername = instance.RemoteDesktop_EnableGatewayServer &&
                                                  Equals(instance.RemoteDesktop_GatewayServerLogonMethod,
                                                      GatewayUserSelectedCredsSource.Userpass) &&
                                                  instance.RemoteDesktop_UseGatewayServerCredentials
                ? instance.RemoteDesktop_GatewayServerUsername
                : string.Empty, // Remove sensitive info on disable
            RemoteDesktop_GatewayServerDomain = instance.RemoteDesktop_EnableGatewayServer &&
                                                Equals(instance.RemoteDesktop_GatewayServerLogonMethod,
                                                    GatewayUserSelectedCredsSource.Userpass) &&
                                                instance.RemoteDesktop_UseGatewayServerCredentials
                ? instance.RemoteDesktop_GatewayServerDomain
                : string.Empty, // Remove sensitive info on disable
            RemoteDesktop_GatewayServerPassword = instance.RemoteDesktop_EnableGatewayServer &&
                                                  Equals(instance.RemoteDesktop_GatewayServerLogonMethod,
                                                      GatewayUserSelectedCredsSource.Userpass) &&
                                                  instance.RemoteDesktop_UseGatewayServerCredentials
                ? instance.RemoteDesktop_GatewayServerPassword
                : new SecureString(), // Remove sensitive info on disable
            RemoteDesktop_OverrideAudioRedirectionMode = instance.RemoteDesktop_OverrideAudioRedirectionMode,
            RemoteDesktop_AudioRedirectionMode = instance.RemoteDesktop_AudioRedirectionMode,
            RemoteDesktop_OverrideAudioCaptureRedirectionMode =
                instance.RemoteDesktop_OverrideAudioCaptureRedirectionMode,
            RemoteDesktop_AudioCaptureRedirectionMode = instance.RemoteDesktop_AudioCaptureRedirectionMode,
            RemoteDesktop_OverrideApplyWindowsKeyCombinations =
                instance.RemoteDesktop_OverrideApplyWindowsKeyCombinations,
            RemoteDesktop_KeyboardHookMode = instance.RemoteDesktop_KeyboardHookMode,
            RemoteDesktop_OverrideRedirectClipboard = instance.RemoteDesktop_OverrideRedirectClipboard,
            RemoteDesktop_RedirectClipboard = instance.RemoteDesktop_RedirectClipboard,
            RemoteDesktop_OverrideRedirectDevices = instance.RemoteDesktop_OverrideRedirectDevices,
            RemoteDesktop_RedirectDevices = instance.RemoteDesktop_RedirectDevices,
            RemoteDesktop_OverrideRedirectDrives = instance.RemoteDesktop_OverrideRedirectDrives,
            RemoteDesktop_RedirectDrives = instance.RemoteDesktop_RedirectDrives,
            RemoteDesktop_OverrideRedirectPorts = instance.RemoteDesktop_OverrideRedirectPorts,
            RemoteDesktop_RedirectPorts = instance.RemoteDesktop_RedirectPorts,
            RemoteDesktop_OverrideRedirectSmartcards = instance.RemoteDesktop_OverrideRedirectSmartcards,
            RemoteDesktop_RedirectSmartCards = instance.RemoteDesktop_RedirectSmartCards,
            RemoteDesktop_OverrideRedirectPrinters = instance.RemoteDesktop_OverrideRedirectPrinters,
            RemoteDesktop_RedirectPrinters = instance.RemoteDesktop_RedirectPrinters,
            RemoteDesktop_OverridePersistentBitmapCaching = instance.RemoteDesktop_OverridePersistentBitmapCaching,
            RemoteDesktop_PersistentBitmapCaching = instance.RemoteDesktop_PersistentBitmapCaching,
            RemoteDesktop_OverrideReconnectIfTheConnectionIsDropped =
                instance.RemoteDesktop_OverrideReconnectIfTheConnectionIsDropped,
            RemoteDesktop_ReconnectIfTheConnectionIsDropped = instance.RemoteDesktop_ReconnectIfTheConnectionIsDropped,
            RemoteDesktop_OverrideNetworkConnectionType = instance.RemoteDesktop_OverrideNetworkConnectionType,
            RemoteDesktop_NetworkConnectionType = instance.RemoteDesktop_NetworkConnectionType,
            RemoteDesktop_DesktopBackground = instance.RemoteDesktop_DesktopBackground,
            RemoteDesktop_FontSmoothing = instance.RemoteDesktop_FontSmoothing,
            RemoteDesktop_DesktopComposition = instance.RemoteDesktop_DesktopComposition,
            RemoteDesktop_ShowWindowContentsWhileDragging = instance.RemoteDesktop_ShowWindowContentsWhileDragging,
            RemoteDesktop_MenuAndWindowAnimation = instance.RemoteDesktop_MenuAndWindowAnimation,
            RemoteDesktop_VisualStyles = instance.RemoteDesktop_VisualStyles,

            // PowerShell
            PowerShell_Enabled = instance.PowerShell_Enabled,
            PowerShell_EnableRemoteConsole = instance.PowerShell_EnableRemoteConsole,
            PowerShell_InheritHost = instance.PowerShell_InheritHost,
            PowerShell_Host =
                instance.PowerShell_InheritHost ? instance.Host?.Trim() : instance.PowerShell_Host?.Trim(),
            PowerShell_OverrideCommand = instance.PowerShell_OverrideCommand,
            PowerShell_Command = instance.PowerShell_Command,
            PowerShell_OverrideAdditionalCommandLine = instance.PowerShell_OverrideAdditionalCommandLine,
            PowerShell_AdditionalCommandLine = instance.PowerShell_AdditionalCommandLine,
            PowerShell_OverrideExecutionPolicy = instance.PowerShell_OverrideExecutionPolicy,
            PowerShell_ExecutionPolicy = instance.PowerShell_ExecutionPolicy,

            // PuTTY
            PuTTY_Enabled = instance.PuTTY_Enabled,
            PuTTY_ConnectionMode = instance.PuTTY_ConnectionMode,
            PuTTY_InheritHost = instance.PuTTY_InheritHost,
            PuTTY_HostOrSerialLine = instance.PuTTY_ConnectionMode == ConnectionMode.Serial
                ? instance.PuTTY_SerialLine?.Trim()
                : instance.PuTTY_InheritHost
                    ? instance.Host?.Trim()
                    : instance.PuTTY_Host?.Trim(),
            PuTTY_OverridePortOrBaud = instance.PuTTY_OverridePortOrBaud,
            PuTTY_PortOrBaud = instance.PuTTY_ConnectionMode == ConnectionMode.Serial
                ? instance.PuTTY_Baud
                : instance.PuTTY_Port,
            PuTTY_OverrideUsername = instance.PuTTY_OverrideUsername,
            PuTTY_Username = instance.PuTTY_Username?.Trim(),
            PuTTY_OverridePrivateKeyFile = instance.PuTTY_OverridePrivateKeyFile,
            PuTTY_PrivateKeyFile = instance.PuTTY_PrivateKeyFile,
            PuTTY_OverrideProfile = instance.PuTTY_OverrideProfile,
            PuTTY_Profile = instance.PuTTY_Profile?.Trim(),
            PuTTY_OverrideHostkey = instance.PuTTY_OverrideHostkey,
            PuTTY_Hostkey = instance.PuTTY_Hostkey?.Trim(),
            PuTTY_OverrideEnableLog = instance.PuTTY_OverrideEnableLog,
            PuTTY_EnableLog = instance.PuTTY_EnableLog,
            PuTTY_OverrideLogMode = instance.PuTTY_OverrideLogMode,
            PuTTY_LogMode = instance.PuTTY_LogMode,
            PuTTY_OverrideLogPath = instance.PuTTY_OverrideLogPath,
            PuTTY_LogPath = instance.PuTTY_LogPath,
            PuTTY_OverrideLogFileName = instance.PuTTY_OverrideLogFileName,
            PuTTY_LogFileName = instance.PuTTY_LogFileName,
            PuTTY_OverrideAdditionalCommandLine = instance.PuTTY_OverrideAdditionalCommandLine,
            PuTTY_AdditionalCommandLine = instance.PuTTY_AdditionalCommandLine?.Trim(),

            // AWS Session Manager
            AWSSessionManager_Enabled = instance.AWSSessionManager_Enabled,
            AWSSessionManager_InstanceID = instance.AWSSessionManager_InstanceID,
            AWSSessionManager_OverrideProfile = instance.AWSSessionManager_OverrideProfile,
            AWSSessionManager_Profile = instance.AWSSessionManager_Profile,
            AWSSessionManager_OverrideRegion = instance.AWSSessionManager_OverrideRegion,
            AWSSessionManager_Region = instance.AWSSessionManager_Region,

            // TigerVNC
            TigerVNC_Enabled = instance.TigerVNC_Enabled,
            TigerVNC_InheritHost = instance.TigerVNC_InheritHost,
            TigerVNC_Host = instance.TigerVNC_InheritHost ? instance.Host?.Trim() : instance.TigerVNC_Host?.Trim(),
            TigerVNC_OverridePort = instance.TigerVNC_OverridePort,
            TigerVNC_Port = instance.TigerVNC_Port,

            // WebConsole
            WebConsole_Enabled = instance.WebConsole_Enabled,
            WebConsole_Url = instance.WebConsole_Url,

            // SNMP
            SNMP_Enabled = instance.SNMP_Enabled,
            SNMP_InheritHost = instance.SNMP_InheritHost,
            SNMP_Host = instance.SNMP_InheritHost ? instance.Host?.Trim() : instance.SNMP_Host?.Trim(),
            SNMP_OverrideOIDAndMode = instance.SNMP_OverrideOIDAndMode,
            SNMP_OID = instance.SNMP_OID,
            SNMP_Mode = instance.SNMP_Mode,
            SNMP_OverrideVersionAndAuth = instance.SNMP_OverrideVersionAndAuth,
            SNMP_Version = instance.SNMP_Version,
            SNMP_Community =
                instance.SNMP_OverrideVersionAndAuth && instance.SNMP_Version != SNMPVersion.V3
                    ? instance.SNMP_Community
                    : new SecureString(),
            SNMP_Security = instance.SNMP_Security,
            SNMP_Username = instance.SNMP_Username,
            SNMP_AuthenticationProvider = instance.SNMP_AuthenticationProvider,
            SNMP_Auth = instance.SNMP_OverrideVersionAndAuth &&
                        instance.SNMP_Version == SNMPVersion.V3 &&
                        instance.SNMP_Security != SNMPV3Security.NoAuthNoPriv
                ? instance.SNMP_Auth
                : new SecureString(),
            SNMP_PrivacyProvider = instance.SNMP_PrivacyProvider,
            SNMP_Priv = instance.SNMP_OverrideVersionAndAuth &&
                        instance.SNMP_Version == SNMPVersion.V3 &&
                        instance.SNMP_Security == SNMPV3Security.AuthPriv
                ? instance.SNMP_Priv
                : new SecureString(),

            // Wake on LAN
            WakeOnLAN_Enabled = instance.WakeOnLAN_Enabled,
            WakeOnLAN_MACAddress = instance.WakeOnLAN_MACAddress?.Trim(),
            WakeOnLAN_Broadcast = instance.WakeOnLAN_Broadcast?.Trim(),

            // Whois
            Whois_Enabled = instance.Whois_Enabled,
            Whois_InheritHost = instance.Whois_InheritHost,
            Whois_Domain = instance.Whois_InheritHost ? instance.Host?.Trim() : instance.Whois_Domain?.Trim(),

            // IP Geolocation
            IPGeolocation_Enabled = instance.IPGeolocation_Enabled,
            IPGeolocation_InheritHost = instance.IPGeolocation_InheritHost,
            IPGeolocation_Host = instance.IPGeolocation_InheritHost
                ? instance.Host?.Trim()
                : instance.IPGeolocation_Host?.Trim()
        };
    }

    #endregion

    #region Methods to add and remove group

    private static GroupInfo ParseGroupInfo(GroupViewModel instance)
    {
        var profiles = instance.Group.Profiles;

        var name = instance.Name.Trim();

        // Update group in profiles
        if (profiles.Count > 0)
            if (!string.IsNullOrEmpty(instance.Group.Name) &&
                !string.Equals(instance.Group.Name, name, StringComparison.Ordinal))
                foreach (var profile in profiles)
                    profile.Group = name;
        //else
        //    Debug.WriteLine("Cannot update group in profiles");
        return new GroupInfo
        {
            Name = name,
            Description = instance.Description?.Trim(),

            Profiles = profiles,

            // Remote Desktop
            RemoteDesktop_UseCredentials = instance.RemoteDesktop_UseCredentials,
            RemoteDesktop_Username =
                instance.RemoteDesktop_UseCredentials
                    ? instance.RemoteDesktop_Username
                    : string.Empty, // Remove sensitive info on disable
            RemoteDesktop_Domain =
                instance.RemoteDesktop_UseCredentials
                    ? instance.RemoteDesktop_Domain
                    : string.Empty, // Remove sensitive info on disable
            RemoteDesktop_Password = instance.RemoteDesktop_UseCredentials
                ? instance.RemoteDesktop_Password
                : new SecureString(), // Remove sensitive info on disable
            RemoteDesktop_OverrideDisplay = instance.RemoteDesktop_OverrideDisplay,
            RemoteDesktop_AdjustScreenAutomatically = instance.RemoteDesktop_AdjustScreenAutomatically,
            RemoteDesktop_UseCurrentViewSize = instance.RemoteDesktop_UseCurrentViewSize,
            RemoteDesktop_UseFixedScreenSize = instance.RemoteDesktop_UseFixedScreenSize,
            RemoteDesktop_ScreenWidth = instance.RemoteDesktop_ScreenWidth,
            RemoteDesktop_ScreenHeight = instance.RemoteDesktop_ScreenHeight,
            RemoteDesktop_UseCustomScreenSize = instance.RemoteDesktop_UseCustomScreenSize,
            RemoteDesktop_CustomScreenWidth = int.Parse(instance.RemoteDesktop_CustomScreenWidth),
            RemoteDesktop_CustomScreenHeight = int.Parse(instance.RemoteDesktop_CustomScreenHeight),
            RemoteDesktop_OverrideColorDepth = instance.RemoteDesktop_OverrideColorDepth,
            RemoteDesktop_ColorDepth = instance.RemoteDesktop_SelectedColorDepth,
            RemoteDesktop_OverridePort = instance.RemoteDesktop_OverridePort,
            RemoteDesktop_Port = instance.RemoteDesktop_Port,
            RemoteDesktop_OverrideCredSspSupport = instance.RemoteDesktop_OverrideCredSspSupport,
            RemoteDesktop_EnableCredSspSupport = instance.RemoteDesktop_EnableCredSspSupport,
            RemoteDesktop_OverrideAuthenticationLevel = instance.RemoteDesktop_OverrideAuthenticationLevel,
            RemoteDesktop_AuthenticationLevel = instance.RemoteDesktop_AuthenticationLevel,
            RemoteDesktop_OverrideGatewayServer = instance.RemoteDesktop_OverrideGatewayServer,
            RemoteDesktop_EnableGatewayServer = instance.RemoteDesktop_EnableGatewayServer,
            RemoteDesktop_GatewayServerHostname = instance.RemoteDesktop_GatewayServerHostname,
            RemoteDesktop_GatewayServerBypassLocalAddresses = instance.RemoteDesktop_GatewayServerBypassLocalAddresses,
            RemoteDesktop_GatewayServerLogonMethod = instance.RemoteDesktop_GatewayServerLogonMethod,
            RemoteDesktop_GatewayServerShareCredentialsWithRemoteComputer =
                instance.RemoteDesktop_GatewayServerShareCredentialsWithRemoteComputer,
            RemoteDesktop_UseGatewayServerCredentials = instance.RemoteDesktop_UseGatewayServerCredentials,
            RemoteDesktop_GatewayServerUsername = instance.RemoteDesktop_EnableGatewayServer &&
                                                  Equals(instance.RemoteDesktop_GatewayServerLogonMethod,
                                                      GatewayUserSelectedCredsSource.Userpass) &&
                                                  instance.RemoteDesktop_UseGatewayServerCredentials
                ? instance.RemoteDesktop_GatewayServerUsername
                : string.Empty, // Remove sensitive info on disable
            RemoteDesktop_GatewayServerDomain = instance.RemoteDesktop_EnableGatewayServer &&
                                                Equals(instance.RemoteDesktop_GatewayServerLogonMethod,
                                                    GatewayUserSelectedCredsSource.Userpass) &&
                                                instance.RemoteDesktop_UseGatewayServerCredentials
                ? instance.RemoteDesktop_GatewayServerDomain
                : string.Empty, // Remove sensitive info on disable
            RemoteDesktop_GatewayServerPassword = instance.RemoteDesktop_EnableGatewayServer &&
                                                  Equals(instance.RemoteDesktop_GatewayServerLogonMethod,
                                                      GatewayUserSelectedCredsSource.Userpass) &&
                                                  instance.RemoteDesktop_UseGatewayServerCredentials
                ? instance.RemoteDesktop_GatewayServerPassword
                : new SecureString(), // Remove sensitive info on disable
            RemoteDesktop_OverrideAudioRedirectionMode = instance.RemoteDesktop_OverrideAudioRedirectionMode,
            RemoteDesktop_AudioRedirectionMode = instance.RemoteDesktop_AudioRedirectionMode,
            RemoteDesktop_OverrideAudioCaptureRedirectionMode =
                instance.RemoteDesktop_OverrideAudioCaptureRedirectionMode,
            RemoteDesktop_AudioCaptureRedirectionMode = instance.RemoteDesktop_AudioCaptureRedirectionMode,
            RemoteDesktop_OverrideApplyWindowsKeyCombinations =
                instance.RemoteDesktop_OverrideApplyWindowsKeyCombinations,
            RemoteDesktop_KeyboardHookMode = instance.RemoteDesktop_KeyboardHookMode,
            RemoteDesktop_OverrideRedirectClipboard = instance.RemoteDesktop_OverrideRedirectClipboard,
            RemoteDesktop_RedirectClipboard = instance.RemoteDesktop_RedirectClipboard,
            RemoteDesktop_OverrideRedirectDevices = instance.RemoteDesktop_OverrideRedirectDevices,
            RemoteDesktop_RedirectDevices = instance.RemoteDesktop_RedirectDevices,
            RemoteDesktop_OverrideRedirectDrives = instance.RemoteDesktop_OverrideRedirectDrives,
            RemoteDesktop_RedirectDrives = instance.RemoteDesktop_RedirectDrives,
            RemoteDesktop_OverrideRedirectPorts = instance.RemoteDesktop_OverrideRedirectPorts,
            RemoteDesktop_RedirectPorts = instance.RemoteDesktop_RedirectPorts,
            RemoteDesktop_OverrideRedirectSmartcards = instance.RemoteDesktop_OverrideRedirectSmartcards,
            RemoteDesktop_RedirectSmartCards = instance.RemoteDesktop_RedirectSmartCards,
            RemoteDesktop_OverrideRedirectPrinters = instance.RemoteDesktop_OverrideRedirectPrinters,
            RemoteDesktop_RedirectPrinters = instance.RemoteDesktop_RedirectPrinters,
            RemoteDesktop_OverridePersistentBitmapCaching = instance.RemoteDesktop_OverridePersistentBitmapCaching,
            RemoteDesktop_PersistentBitmapCaching = instance.RemoteDesktop_PersistentBitmapCaching,
            RemoteDesktop_OverrideReconnectIfTheConnectionIsDropped =
                instance.RemoteDesktop_OverrideReconnectIfTheConnectionIsDropped,
            RemoteDesktop_ReconnectIfTheConnectionIsDropped = instance.RemoteDesktop_ReconnectIfTheConnectionIsDropped,
            RemoteDesktop_OverrideNetworkConnectionType = instance.RemoteDesktop_OverrideNetworkConnectionType,
            RemoteDesktop_NetworkConnectionType = instance.RemoteDesktop_NetworkConnectionType,
            RemoteDesktop_DesktopBackground = instance.RemoteDesktop_DesktopBackground,
            RemoteDesktop_FontSmoothing = instance.RemoteDesktop_FontSmoothing,
            RemoteDesktop_DesktopComposition = instance.RemoteDesktop_DesktopComposition,
            RemoteDesktop_ShowWindowContentsWhileDragging = instance.RemoteDesktop_ShowWindowContentsWhileDragging,
            RemoteDesktop_MenuAndWindowAnimation = instance.RemoteDesktop_MenuAndWindowAnimation,
            RemoteDesktop_VisualStyles = instance.RemoteDesktop_VisualStyles,

            // PowerShell
            PowerShell_OverrideCommand = instance.PowerShell_OverrideCommand,
            PowerShell_Command = instance.PowerShell_Command,
            PowerShell_OverrideAdditionalCommandLine = instance.PowerShell_OverrideAdditionalCommandLine,
            PowerShell_AdditionalCommandLine = instance.PowerShell_AdditionalCommandLine,
            PowerShell_OverrideExecutionPolicy = instance.PowerShell_OverrideExecutionPolicy,
            PowerShell_ExecutionPolicy = instance.PowerShell_ExecutionPolicy,

            // PuTTY
            PuTTY_OverrideUsername = instance.PuTTY_OverrideUsername,
            PuTTY_Username = instance.PuTTY_Username?.Trim(),
            PuTTY_OverridePrivateKeyFile = instance.PuTTY_OverridePrivateKeyFile,
            PuTTY_PrivateKeyFile = instance.PuTTY_PrivateKeyFile,
            PuTTY_OverrideProfile = instance.PuTTY_OverrideProfile,
            PuTTY_Profile = instance.PuTTY_Profile?.Trim(),
            PuTTY_OverrideEnableLog = instance.PuTTY_OverrideEnableLog,
            PuTTY_EnableLog = instance.PuTTY_EnableLog,
            PuTTY_OverrideLogMode = instance.PuTTY_OverrideLogMode,
            PuTTY_LogMode = instance.PuTTY_LogMode,
            PuTTY_OverrideLogPath = instance.PuTTY_OverrideLogPath,
            PuTTY_LogPath = instance.PuTTY_LogPath,
            PuTTY_OverrideLogFileName = instance.PuTTY_OverrideLogFileName,
            PuTTY_LogFileName = instance.PuTTY_LogFileName,
            PuTTY_OverrideAdditionalCommandLine = instance.PuTTY_OverrideAdditionalCommandLine,
            PuTTY_AdditionalCommandLine = instance.PuTTY_AdditionalCommandLine?.Trim(),

            // AWS Session Manager
            AWSSessionManager_OverrideProfile = instance.AWSSessionManager_OverrideProfile,
            AWSSessionManager_Profile = instance.AWSSessionManager_Profile,
            AWSSessionManager_OverrideRegion = instance.AWSSessionManager_OverrideRegion,
            AWSSessionManager_Region = instance.AWSSessionManager_Region,

            // TigerVNC
            TigerVNC_OverridePort = instance.TigerVNC_OverridePort,
            TigerVNC_Port = instance.TigerVNC_Port,

            // SNMP
            SNMP_OverrideOIDAndMode = instance.SNMP_OverrideOIDAndMode,
            SNMP_OID = instance.SNMP_OID,
            SNMP_Mode = instance.SNMP_Mode,
            SNMP_OverrideVersionAndAuth = instance.SNMP_OverrideVersionAndAuth,
            SNMP_Version = instance.SNMP_Version,
            SNMP_Community =
                instance.SNMP_OverrideVersionAndAuth && instance.SNMP_Version != SNMPVersion.V3
                    ? instance.SNMP_Community
                    : new SecureString(),
            SNMP_Security = instance.SNMP_Security,
            SNMP_Username = instance.SNMP_Username,
            SNMP_AuthenticationProvider = instance.SNMP_AuthenticationProvider,
            SNMP_Auth = instance.SNMP_OverrideVersionAndAuth &&
                        instance.SNMP_Version == SNMPVersion.V3 &&
                        instance.SNMP_Security != SNMPV3Security.NoAuthNoPriv
                ? instance.SNMP_Auth
                : new SecureString(),
            SNMP_PrivacyProvider = instance.SNMP_PrivacyProvider,
            SNMP_Priv = instance.SNMP_OverrideVersionAndAuth &&
                        instance.SNMP_Version == SNMPVersion.V3 &&
                        instance.SNMP_Security == SNMPV3Security.AuthPriv
                ? instance.SNMP_Priv
                : new SecureString()
        };
    }

    #endregion

    #region Dialog to add, edit, copy as and delete profile

    public static Task ShowAddProfileDialog(Window parentWindow, IProfileManagerMinimal viewModel,
        ProfileInfo profile = null, string group = null,
        ApplicationName applicationName = ApplicationName.None)
    {
        var childWindow = new ProfileChildWindow(parentWindow);

        ProfileViewModel childWindowViewModel = new(instance =>
        {
            childWindow.IsOpen = false;
            Settings.ConfigurationManager.Current.IsChildWindowOpen = false;

            viewModel.OnProfileManagerDialogClose();

            ProfileManager.AddProfile(ParseProfileInfo(instance));
        }, _ =>
        {
            childWindow.IsOpen = false;
            Settings.ConfigurationManager.Current.IsChildWindowOpen = false;

            viewModel.OnProfileManagerDialogClose();
        }, ProfileManager.GetGroupNames(), group, ProfileEditMode.Add, profile, applicationName);

        childWindow.Title = Strings.AddProfile;

        childWindow.DataContext = childWindowViewModel;

        viewModel.OnProfileManagerDialogOpen();
        
        Settings.ConfigurationManager.Current.IsChildWindowOpen = true;
        
        return parentWindow.ShowChildWindowAsync(childWindow);
    }

    public static Task ShowEditProfileDialog(Window parentWindow, IProfileManagerMinimal viewModel,
        ProfileInfo profile)
    {
        var childWindow = new ProfileChildWindow(parentWindow);

        ProfileViewModel childWindowViewModel = new(instance =>
        {
            childWindow.IsOpen = false;
            Settings.ConfigurationManager.Current.IsChildWindowOpen = false;

            viewModel.OnProfileManagerDialogClose();

            ProfileManager.ReplaceProfile(profile, ParseProfileInfo(instance));
        }, _ =>
        {
            childWindow.IsOpen = false;
            Settings.ConfigurationManager.Current.IsChildWindowOpen = false;

            viewModel.OnProfileManagerDialogClose();
        }, ProfileManager.GetGroupNames(), profile.Group, ProfileEditMode.Edit, profile);

        childWindow.Title = Strings.EditProfile;

        childWindow.DataContext = childWindowViewModel;

        viewModel.OnProfileManagerDialogOpen();
        
        Settings.ConfigurationManager.Current.IsChildWindowOpen = true;
        
        return parentWindow.ShowChildWindowAsync(childWindow);
    }

    public static Task ShowCopyAsProfileDialog(Window parentWindow, IProfileManagerMinimal viewModel,
        ProfileInfo profile)
    {
        var childWindow = new ProfileChildWindow(parentWindow);

        ProfileViewModel childWindowViewModel = new(instance =>
        {
            childWindow.IsOpen = false;
            Settings.ConfigurationManager.Current.IsChildWindowOpen = false;
            
            viewModel.OnProfileManagerDialogClose();

            ProfileManager.AddProfile(ParseProfileInfo(instance));
        }, _ =>
        {
            childWindow.IsOpen = false;
            Settings.ConfigurationManager.Current.IsChildWindowOpen = false;
            
            viewModel.OnProfileManagerDialogClose();
        }, ProfileManager.GetGroupNames(), profile.Group, ProfileEditMode.Copy, profile);

        childWindow.Title = Strings.CopyProfile;
        
        childWindow.DataContext = childWindowViewModel;
        
        viewModel.OnProfileManagerDialogOpen();
        
        Settings.ConfigurationManager.Current.IsChildWindowOpen = true;

        return parentWindow.ShowChildWindowAsync(childWindow);
    }

    public static Task ShowDeleteProfileDialog(Window parentWindow, IProfileManagerMinimal viewModel,
        IList<ProfileInfo> profiles)
    {
        var childWindow = new OKCancelInfoMessageChildWindow();
        
        OKCancelInfoMessageViewModel childWindowViewModel = new(_ =>
            {
                childWindow.IsOpen = false;
                Settings.ConfigurationManager.Current.IsChildWindowOpen = false;
                
                viewModel.OnProfileManagerDialogClose();

                ProfileManager.RemoveProfiles(profiles);
            }, _ =>
            {
                childWindow.IsOpen = false;
                Settings.ConfigurationManager.Current.IsChildWindowOpen = false;
                
                viewModel.OnProfileManagerDialogClose();
            },
            profiles.Count == 1
                ? Strings.DeleteProfileMessage
                : Strings.DeleteProfilesMessage);

        childWindow.Title = Strings.DeleteProfile;
        
        childWindow.DataContext = childWindowViewModel;

        viewModel.OnProfileManagerDialogOpen();
        
        Settings.ConfigurationManager.Current.IsChildWindowOpen = true;

        return parentWindow.ShowChildWindowAsync(childWindow);
    }

    #endregion

    #region Dialog to add, edit and delete group

    public static Task ShowAddGroupDialog(Window parentWindow, IProfileManagerMinimal viewModel)
    {
        var childWindow = new GroupChildWindow(parentWindow);
        
        GroupViewModel childWindowViewModel = new(instance =>
        {
            childWindow.IsOpen = false;
            Settings.ConfigurationManager.Current.IsChildWindowOpen = false;
            
            viewModel.OnProfileManagerDialogClose();
            
            ProfileManager.AddGroup(ParseGroupInfo(instance));
        }, _ =>
        {
            childWindow.IsOpen = false;
            Settings.ConfigurationManager.Current.IsChildWindowOpen = false;
            
            viewModel.OnProfileManagerDialogClose();
        }, ProfileManager.GetGroupNames());
        
        childWindow.Title = Strings.AddGroup;
        
        childWindow.DataContext = childWindowViewModel;
        
        viewModel.OnProfileManagerDialogOpen();
        
        Settings.ConfigurationManager.Current.IsChildWindowOpen = true;
        
        return parentWindow.ShowChildWindowAsync(childWindow);
    }

    public static Task ShowEditGroupDialog(Window parentWindow, IProfileManagerMinimal viewModel,
        GroupInfo group)
    {
        var childWindow = new GroupChildWindow(parentWindow);
        
        GroupViewModel childWindowViewModel = new(instance =>
        {
            childWindow.IsOpen = false;
            Settings.ConfigurationManager.Current.IsChildWindowOpen = false;
            
            viewModel.OnProfileManagerDialogClose();
            
            ProfileManager.ReplaceGroup(group, ParseGroupInfo(instance));
        }, _ =>
        {
            childWindow.IsOpen = false;
            Settings.ConfigurationManager.Current.IsChildWindowOpen = false;
            
            viewModel.OnProfileManagerDialogClose();
        }, ProfileManager.GetGroupNames(), GroupEditMode.Edit, group);
        
        childWindow.Title = Strings.EditGroup;
        
        childWindow.DataContext = childWindowViewModel;
        
        viewModel.OnProfileManagerDialogOpen();
        
        Settings.ConfigurationManager.Current.IsChildWindowOpen = true;
        
        return parentWindow.ShowChildWindowAsync(childWindow);
    }

    public static Task ShowDeleteGroupDialog(Window parentWindow, IProfileManagerMinimal viewModel,
        GroupInfo group)
    {
        var childWindow = new OKCancelInfoMessageChildWindow();
        
        OKCancelInfoMessageViewModel childWindowViewModel = new(_ =>
        {
            childWindow.IsOpen = false;
            Settings.ConfigurationManager.Current.IsChildWindowOpen = false;
            
            viewModel.OnProfileManagerDialogClose();

            ProfileManager.RemoveGroup(group);
        }, _ =>
        {
            childWindow.IsOpen = false;
            Settings.ConfigurationManager.Current.IsChildWindowOpen = false;
            
            viewModel.OnProfileManagerDialogClose();
        }, Strings.DeleteGroupMessage);
        
        childWindow.Title = Strings.DeleteGroup;
        
        childWindow.DataContext = childWindowViewModel;
        
        viewModel.OnProfileManagerDialogOpen();
        
        Settings.ConfigurationManager.Current.IsChildWindowOpen = true;
        
        return parentWindow.ShowChildWindowAsync(childWindow);
    }

    #endregion
}