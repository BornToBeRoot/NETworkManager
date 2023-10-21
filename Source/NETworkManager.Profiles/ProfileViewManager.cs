using MahApps.Metro.IconPacks;
using NETworkManager.Models;
using System.Collections.Generic;

namespace NETworkManager.Profiles;

public static class ProfileViewManager
{
    // List of all applications
    public static List<ProfileViewInfo> List => new()
    {
        // General
        new ProfileViewInfo(ProfileName.General, new PackIconModern{ Kind = PackIconModernKind.Box }, ProfileGroup.General),

        // Applications
        new ProfileViewInfo(ProfileName.NetworkInterface, ApplicationManager.GetIcon(ApplicationName.NetworkInterface), ProfileGroup.Application),
        new ProfileViewInfo(ProfileName.IPScanner, ApplicationManager.GetIcon(ApplicationName.IPScanner), ProfileGroup.Application),
        new ProfileViewInfo(ProfileName.PortScanner, ApplicationManager.GetIcon(ApplicationName.PortScanner), ProfileGroup.Application),
        new ProfileViewInfo(ProfileName.PingMonitor, ApplicationManager.GetIcon(ApplicationName.PingMonitor), ProfileGroup.Application),
        new ProfileViewInfo(ProfileName.Traceroute, ApplicationManager.GetIcon(ApplicationName.Traceroute), ProfileGroup.Application),
        new ProfileViewInfo(ProfileName.DNSLookup, ApplicationManager.GetIcon(ApplicationName.DNSLookup), ProfileGroup.Application),
        new ProfileViewInfo(ProfileName.RemoteDesktop, ApplicationManager.GetIcon(ApplicationName.RemoteDesktop), ProfileGroup.Application),
        new ProfileViewInfo(ProfileName.PowerShell, ApplicationManager.GetIcon(ApplicationName.PowerShell), ProfileGroup.Application),
        new ProfileViewInfo(ProfileName.PuTTY, ApplicationManager.GetIcon(ApplicationName.PuTTY), ProfileGroup.Application),
        new ProfileViewInfo(ProfileName.AWSSessionManager, ApplicationManager.GetIcon(ApplicationName.AWSSessionManager), ProfileGroup.Application),
        new ProfileViewInfo(ProfileName.TigerVNC, ApplicationManager.GetIcon(ApplicationName.TigerVNC), ProfileGroup.Application),
        new ProfileViewInfo(ProfileName.WebConsole, ApplicationManager.GetIcon(ApplicationName.WebConsole), ProfileGroup.Application),
        new ProfileViewInfo(ProfileName.SNMP, ApplicationManager.GetIcon(ApplicationName.SNMP), ProfileGroup.Application),
        new ProfileViewInfo(ProfileName.WakeOnLAN, ApplicationManager.GetIcon(ApplicationName.WakeOnLAN), ProfileGroup.Application),            
        new ProfileViewInfo(ProfileName.Whois, ApplicationManager.GetIcon(ApplicationName.Whois), ProfileGroup.Application),
        new ProfileViewInfo(ProfileName.IPGeolocation, ApplicationManager.GetIcon(ApplicationName.IPGeolocation), ProfileGroup.Application)
    };              
}
