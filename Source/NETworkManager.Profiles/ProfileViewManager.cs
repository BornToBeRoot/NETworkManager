using System.Collections.Generic;
using MahApps.Metro.IconPacks;
using NETworkManager.Models;

namespace NETworkManager.Profiles;

/// <summary>
/// Provides centralized access to all available profile views within the application.
/// </summary>
/// <remarks>The ProfileViewManager class exposes a static list of ProfileViewInfo objects, each representing a
/// distinct application or tool, such as network utilities and monitoring features. This enables developers to
/// enumerate and display profile views, each associated with an icon and a profile group, for use in user interfaces or
/// configuration scenarios.</remarks>
public static class ProfileViewManager
{
    /// <summary>
    /// Gets a static collection of predefined profile view information, organized by general and application profiles.
    /// </summary>
    /// <remarks>The collection contains instances of ProfileViewInfo representing various network and
    /// application tools, each associated with an icon and a profile group. Use this property to access commonly used
    /// profiles for display or selection purposes.</remarks>
    public static List<ProfileViewInfo> List =>
    [
        // General
        new(ProfileName.General, new PackIconModern { Kind = PackIconModernKind.Box },
            ProfileGroup.General),

        // Applications
        new(ProfileName.NetworkInterface, ApplicationManager.GetIcon(ApplicationName.NetworkInterface),
            ProfileGroup.Application),
        new(ProfileName.IPScanner, ApplicationManager.GetIcon(ApplicationName.IPScanner),
            ProfileGroup.Application),
        new(ProfileName.PortScanner, ApplicationManager.GetIcon(ApplicationName.PortScanner),
            ProfileGroup.Application),
        new(ProfileName.PingMonitor, ApplicationManager.GetIcon(ApplicationName.PingMonitor),
            ProfileGroup.Application),
        new(ProfileName.Traceroute, ApplicationManager.GetIcon(ApplicationName.Traceroute),
            ProfileGroup.Application),
        new(ProfileName.DNSLookup, ApplicationManager.GetIcon(ApplicationName.DNSLookup),
            ProfileGroup.Application),
        new(ProfileName.RemoteDesktop, ApplicationManager.GetIcon(ApplicationName.RemoteDesktop),
            ProfileGroup.Application),
        new(ProfileName.PowerShell, ApplicationManager.GetIcon(ApplicationName.PowerShell),
            ProfileGroup.Application),
        new(ProfileName.PuTTY, ApplicationManager.GetIcon(ApplicationName.PuTTY),
            ProfileGroup.Application),
        new(ProfileName.TigerVNC, ApplicationManager.GetIcon(ApplicationName.TigerVNC),
            ProfileGroup.Application),
        new(ProfileName.WebConsole, ApplicationManager.GetIcon(ApplicationName.WebConsole),
            ProfileGroup.Application),
        new(ProfileName.SNMP, ApplicationManager.GetIcon(ApplicationName.SNMP),
            ProfileGroup.Application),
        /*
        new(ProfileName.Firewall, ApplicationManager.GetIcon(ApplicationName.Firewall),
            ProfileGroup.Application),
        */
        new(ProfileName.WakeOnLAN, ApplicationManager.GetIcon(ApplicationName.WakeOnLAN),
            ProfileGroup.Application),
        new(ProfileName.Whois, ApplicationManager.GetIcon(ApplicationName.Whois),
            ProfileGroup.Application),
        new(ProfileName.IPGeolocation, ApplicationManager.GetIcon(ApplicationName.IPGeolocation),
            ProfileGroup.Application),      
    ];
}