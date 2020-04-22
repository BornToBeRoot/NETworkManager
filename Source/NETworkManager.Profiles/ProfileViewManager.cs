using MahApps.Metro.IconPacks;
using NETworkManager.Models;
using System.Collections.Generic;

namespace NETworkManager.Profiles
{
    public static class ProfileViewManager
    {
        // List of all applications
        public static List<ProfileViewInfo> List => new List<ProfileViewInfo>
        {
            // General
            new ProfileViewInfo(ProfileViewName.General, new PackIconModern{ Kind = PackIconModernKind.Box }),

            // Applications
            new ProfileViewInfo(ProfileViewName.NetworkInterface, ApplicationManager.GetIcon(ApplicationName.NetworkInterface)),
            new ProfileViewInfo(ProfileViewName.IPScanner, ApplicationManager.GetIcon(ApplicationName.IPScanner)),
            new ProfileViewInfo(ProfileViewName.PortScanner, ApplicationManager.GetIcon(ApplicationName.PortScanner)),
            new ProfileViewInfo(ProfileViewName.PingMonitor, ApplicationManager.GetIcon(ApplicationName.PingMonitor)),
            new ProfileViewInfo(ProfileViewName.Traceroute, ApplicationManager.GetIcon(ApplicationName.Traceroute)),
            new ProfileViewInfo(ProfileViewName.DNSLookup, ApplicationManager.GetIcon(ApplicationName.DNSLookup)),
            new ProfileViewInfo(ProfileViewName.RemoteDesktop, ApplicationManager.GetIcon(ApplicationName.RemoteDesktop)),
            new ProfileViewInfo(ProfileViewName.PowerShell, ApplicationManager.GetIcon(ApplicationName.PowerShell)),
            new ProfileViewInfo(ProfileViewName.PuTTY, ApplicationManager.GetIcon(ApplicationName.PuTTY)),
            new ProfileViewInfo(ProfileViewName.TigerVNC, ApplicationManager.GetIcon(ApplicationName.TigerVNC)),
            new ProfileViewInfo(ProfileViewName.WebConsole, ApplicationManager.GetIcon(ApplicationName.WebConsole)),
            new ProfileViewInfo(ProfileViewName.WakeOnLAN, ApplicationManager.GetIcon(ApplicationName.WakeOnLAN)),            
            new ProfileViewInfo(ProfileViewName.Whois, ApplicationManager.GetIcon(ApplicationName.Whois))
        };              
    }
}
