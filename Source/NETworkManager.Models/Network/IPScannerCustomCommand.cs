using NETworkManager.Utilities;
using System;
using System.Collections.Generic;

namespace NETworkManager.Models.Network;

public static class IPScannerCustomCommand
{
    public static List<CustomCommandInfo> GetDefaultList()
    {
        return new List<CustomCommandInfo>
        {
            new CustomCommandInfo(Guid.NewGuid(), "Edge", "cmd.exe", @"/c start microsoft-edge:http://$$ipaddress$$/"),
            new CustomCommandInfo(Guid.NewGuid(), "Edge (https)", "cmd.exe", @"/c start microsoft-edge:https://$$ipaddress$$/"),
            new CustomCommandInfo(Guid.NewGuid(), "Windows Explorer (c$)", "explorer.exe", @"\\$$ipaddress$$\c$")
        };
    }
}
