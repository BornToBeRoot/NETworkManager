using MahApps.Metro.IconPacks;
using NETworkManager.Models;
using System.Collections.Generic;

namespace NETworkManager.Profiles
{
    public static class GroupViewManager
    {
        // List of all applications
        public static List<GroupViewInfo> List => new List<GroupViewInfo>
        {
            // General
            new GroupViewInfo(GroupViewName.General, new PackIconModern{ Kind = PackIconModernKind.Box }),

            // Applications
            new GroupViewInfo(GroupViewName.RemoteDesktop, ApplicationManager.GetIcon(ApplicationName.RemoteDesktop)),
            new GroupViewInfo(GroupViewName.PowerShell, ApplicationManager.GetIcon(ApplicationName.PowerShell)),
            new GroupViewInfo(GroupViewName.PuTTY, ApplicationManager.GetIcon(ApplicationName.PuTTY)),
            new GroupViewInfo(GroupViewName.TigerVNC, ApplicationManager.GetIcon(ApplicationName.TigerVNC))
        };              
    }
}
