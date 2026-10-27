using System;
using System.Collections.Generic;
using NETworkManager.Utilities;

namespace NETworkManager.Models.Network;

public static class IPScannerCustomCommand
{
    public static List<CustomCommandInfo> GetDefaultList()
    {
        return new List<CustomCommandInfo>
        {
            new(Guid.NewGuid(), "Edge", "cmd.exe", @"/c start microsoft-edge:http://{{IPAddress}}/"),
            new(Guid.NewGuid(), "Edge (https)", "cmd.exe",
                @"/c start microsoft-edge:https://{{IPAddress}}/"),
            new(Guid.NewGuid(), "Windows Explorer (c$)", "explorer.exe", @"\\{{IPAddress}}\c$"),
            new(Guid.NewGuid(), "Windows Explorer (shares)", "explorer.exe", @"\\{{IPAddress}}"),
            new(Guid.NewGuid(), "Computer Management", "mmc.exe", @"compmgmt.msc /computer:\\{{IPAddress}}"),
            new(Guid.NewGuid(), "Event Viewer", "eventvwr.exe", @"\\{{IPAddress}}"),
            new(Guid.NewGuid(), "System Information", "msinfo32.exe", "/computer {{IPAddress}}")
        };
    }
}