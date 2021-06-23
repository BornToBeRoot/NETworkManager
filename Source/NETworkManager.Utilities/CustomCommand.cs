using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NETworkManager.Utilities
{
    /// <summary>
    /// Class provides static methods to manage custom commands.
    /// </summary>
    public static class CustomCommand
    {
        /// <summary>
        /// Method returns a list of default custom commands (see <see cref="CustomCommandInfo"/>.
        /// </summary>
        /// <returns>List with <see cref="CustomCommandInfo"/>s.</returns>
        public static List<CustomCommandInfo> GetDefaults() => new List<CustomCommandInfo>
        {
            new CustomCommandInfo(Guid.NewGuid(), "Internet Explorer", "iexplore.exe", @"http://$$ipaddress$$/"),
            new CustomCommandInfo(Guid.NewGuid(), "Internet Explorer (https)", "iexplore.exe", @"https://$$ipaddress$$/"),
            new CustomCommandInfo(Guid.NewGuid(), "Windows Explorer (c$)", "explorer.exe", @"\\$$ipaddress$$\c$"),
        };

        /// <summary>
        /// Method to execute a <see cref="CustomCommandInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="CustomCommandInfo"/> which is executed.</param>
        public static void Run(CustomCommandInfo info)
        {
            var processStartInfo = new ProcessStartInfo()
            {
                FileName = info.FilePath,
                UseShellExecute = true
            };

            if (!string.IsNullOrEmpty(info.Arguments))
                processStartInfo.Arguments = info.Arguments;

            Process.Start(processStartInfo);
        }
    }
}