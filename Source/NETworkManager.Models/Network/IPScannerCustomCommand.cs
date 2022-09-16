using NETworkManager.Utilities;
using System;
using System.Collections.Generic;

namespace NETworkManager.Models.Network
{
    public static class IPScannerCustomCommand
    {
        public static List<CustomCommandInfo> GetDefaultList()
        {
            return new List<CustomCommandInfo>
            {
                new CustomCommandInfo(Guid.NewGuid(), "Internet Explorer", "iexplore.exe", @"http://$$ipaddress$$/"),
                new CustomCommandInfo(Guid.NewGuid(), "Internet Explorer (https)", "iexplore.exe", @"https://$$ipaddress$$/"),
                new CustomCommandInfo(Guid.NewGuid(), "Windows Explorer (c$)", "explorer.exe", @"\\$$ipaddress$$\c$")
            };
        }
    }
}
