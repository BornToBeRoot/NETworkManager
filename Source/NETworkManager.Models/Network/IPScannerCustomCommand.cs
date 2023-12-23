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
            new(Guid.NewGuid(), "Edge", "cmd.exe", @"/c start microsoft-edge:http://$$ipaddress$$/"),
            new(Guid.NewGuid(), "Edge (https)", "cmd.exe",
                @"/c start microsoft-edge:https://$$ipaddress$$/"),
            new(Guid.NewGuid(), "Windows Explorer (c$)", "explorer.exe", @"\\$$ipaddress$$\c$")
        };
    }
}