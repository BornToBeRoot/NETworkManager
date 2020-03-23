using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NETworkManager.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public static class CustomCommand
    {
        /// <summary>
        /// 
        /// </summary>
        public static List<CustomCommandInfo> GetDefaults() => new List<CustomCommandInfo>
        {
            new CustomCommandInfo(Guid.NewGuid(), "Internet Explorer", "iexplore.exe", @"http://$$ipaddress$$/"),
            new CustomCommandInfo(Guid.NewGuid(), "Internet Explorer (https)", "iexplore.exe", @"https://$$ipaddress$$/"),
            new CustomCommandInfo(Guid.NewGuid(), "Windows Explorer (c$)", "explorer.exe", @"\\$$ipaddress$$\c$"),
        };

        /// <summary>
        /// 
        /// </summary>
        public static void Run(CustomCommandInfo info)
        {
            if (string.IsNullOrEmpty(info.Arguments))
                Process.Start(info.FilePath);
            else
                Process.Start(info.FilePath, info.Arguments);
        }
    }
}