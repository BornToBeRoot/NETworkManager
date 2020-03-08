using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NETworkManager.Utilities
{
    public static class CustomCommand
    {
        public static List<CustomCommandInfo> DefaultList()
        {
            return new List<CustomCommandInfo>
            {
                new CustomCommandInfo(Guid.NewGuid(), "Internet Explorer", "iexplore.exe", @"http://$$ipaddress$$/"),
                new CustomCommandInfo(Guid.NewGuid(), "Internet Explorer (https)", "iexplore.exe", @"https://$$ipaddress$$/"),
                new CustomCommandInfo(Guid.NewGuid(), "Windows Explorer (c$)", "explorer.exe", @"\\$$ipaddress$$\c$"),
            };
        }

        public static void Run(CustomCommandInfo info)
        {
            if (string.IsNullOrEmpty(info.Arguments))
                Process.Start(info.FilePath);
            else
                Process.Start(info.FilePath, info.Arguments);
        }
    }
}