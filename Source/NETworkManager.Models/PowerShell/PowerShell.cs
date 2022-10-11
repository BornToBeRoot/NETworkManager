using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NETworkManager.Models.PowerShell
{
    public static partial class PowerShell
    {

        /// <summary>
        /// Default SZ registry keys for the global PowerShell profile.
        /// </summary>
        private static readonly List<Tuple<string, string>> DefaultProfileRegkeysSZBase = new()
        {
            new Tuple<string, string>("FaceName", "Consolas"),

        };

        private static readonly List<Tuple<string, int>> DefaultProfileRegkeysDwordBase = new()
        {
            new Tuple<string, int>("CursorType", 1),
            new Tuple<string, int>("FontFamiliy", 54), // 36
            new Tuple<string, int>("FontSize", 1179648), // 120000
            new Tuple<string, int>("FontWeight", 400) // 190
        };

        private static List<Tuple<string, int>> GetProfileRegkeysDwordDark()
        {
            return DefaultProfileRegkeysDwordBase.Concat(
                new[] {
                    new Tuple<string, int>("DefaultBackground", 2434341 ), // HEX: 252525
                    new Tuple<string, int>("ColorTable00", 2434341), // HEX: 252525
                    new Tuple<string, int>("ColorTable07", 13421772), // HEX: cccccc
                }).ToList();
        }

        /*
        private static List<Tuple<string, int>> GetProfileRegkeysDwordWhite()
        {
            return DefaultProfileRegkeysDword.Concat(
                new[] {
                    // new Tuple<string, string>("Colour0", "68,68,68"),
                    //new Tuple<string, int>("Colour0", "0,0,0"),          // Foreground
                    //new Tuple<string, int>("Colour2", "255,255,255")     // Background
                }).ToList();
        }
        */

        public static void WriteDefaultProfileToRegistry(string theme, string powerShellPath)
        {            
            //string profilePath = @"Console\%SystemRoot%_System32_WindowsPowerShell_v1.0_powershell.exe";
            string profilePath = @"Console\C:_Program Files_PowerShell_7_pwsh.exe";

            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(profilePath, true);

            registryKey ??= Registry.CurrentUser.CreateSubKey(profilePath);

            if (registryKey != null)
            {
                //foreach (Tuple<string, string> key in theme == "Dark" ? GetProfileRegkeysDwordDark() : GetProfileRegkeysDwordWhite())
                foreach (var key in GetProfileRegkeysDwordDark())
                    registryKey.SetValue(key.Item1, key.Item2);

                foreach (var key in DefaultProfileRegkeysSZBase)
                    registryKey.SetValue(key.Item1, key.Item2);
            }

            registryKey.Close();
        }

        /// <summary>
        /// Build command line arguments based on a <see cref="PowerShellSessionInfo"/>.
        /// </summary>
        /// <param name="sessionInfo">Instance of <see cref="PowerShellSessionInfo"/>.</param>
        /// <returns>Command line arguments like "-NoExit -ExecutionPolicy RemoteSigned ...".</returns>
        public static string BuildCommandLine(PowerShellSessionInfo sessionInfo)
        {
            var command = $"-NoExit -ExecutionPolicy {sessionInfo.ExecutionPolicy} {sessionInfo.AdditionalCommandLine}";

            // Connect to remote host or execute local command if configured
            if (sessionInfo.EnableRemoteConsole)
                command += $" -Command \"& {{Enter-PSSession -ComputerName {sessionInfo.Host}}}\"";
            else if (!string.IsNullOrEmpty(sessionInfo.Command))
                command += $" -Command \"& {{{sessionInfo.Command}}}\"";

            return command;
        }
    }
}
