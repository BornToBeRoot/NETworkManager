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
        private static List<Tuple<string, int>> GetProfileRegkeysDwordWhite()
        {
            return DefaultProfileRegkeysDwordBase.Concat(
                new[] {
                    new Tuple<string, int>("DefaultBackground", 16777215 ), // HEX: FFFFFF
                    new Tuple<string, int>("ColorTable00", 16777215), // HEX: FFFFFF
                    new Tuple<string, int>("ColorTable07", 2434341), // HEX: 252525
                }).ToList();
        }

        private static List<string> DefaultProfileRegkeysDwordDelete = new()
        {
            "ScreenColors"
        };

        public static void WriteDefaultProfileToRegistry(string theme, string powerShellPath)
        {
            var registryPath = @"Console\";

            var systemRoot = Environment.ExpandEnvironmentVariables("%SystemRoot%");

            // Windows PowerShell --> HKCU:\Console\%SystemRoot%_System32_WindowsPowerShell_v1.0_powershell.exe
            if (powerShellPath.StartsWith(systemRoot))
                registryPath += "%SystemRoot%" + powerShellPath.Substring(systemRoot.Length, powerShellPath.Length - systemRoot.Length).Replace(@"\", "_");
            // PWSH -->  HKCU:\Console\C:_Program Files_PowerShell_7_pwsh.exe
            else
                registryPath += powerShellPath.Replace(@"\", "_");

            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(registryPath, true);

            registryKey ??= Registry.CurrentUser.CreateSubKey(registryPath);

            if (registryKey != null)
            {
                foreach (var item in theme == "Dark" ? GetProfileRegkeysDwordDark() : GetProfileRegkeysDwordWhite())
                    registryKey.SetValue(item.Item1, item.Item2);

                foreach (var item in DefaultProfileRegkeysSZBase)
                    registryKey.SetValue(item.Item1, item.Item2);

                foreach (var item in DefaultProfileRegkeysDwordDelete)
                {
                    registryKey.DeleteValue(item, false);
                }
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
