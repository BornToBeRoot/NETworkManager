using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NETworkManager.Models.PuTTY
{
    /// <summary>
    /// Class control PuTTY.
    /// </summary>
    public partial class PuTTY
    {
        /// <summary>
        /// Name of the PuTTY folder.
        /// </summary>
        private static string _puttyFolder => "PuTTY";

        /// <summary>
        /// Name of the PuTTY executable.
        /// </summary>
        private static string _puttyFile => "putty.exe";

        /// <summary>
        /// Default PuTTY installation paths.
        /// </summary>
        public static readonly List<string> GetDefaultInstallationPaths = new()
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), _puttyFolder, _puttyFile),
            Path.Combine(Environment.GetFolderPath( Environment.SpecialFolder.ProgramFilesX86), _puttyFolder, _puttyFile)
        };

        /// <summary>
        /// Default SZ registry keys for PuTTY profile NETworkManager.
        /// </summary>
        private static readonly List<Tuple<string, string>> DefaultProfileRegkeysSZBase = new()
        {
            new Tuple<string, string>("Colour1", "255,255,255"),
            new Tuple<string, string>("Colour3", "85,85,85"),
            new Tuple<string, string>("Colour4", "0,0,0"),
            new Tuple<string, string>("Colour5", "0,255,0"),
            new Tuple<string, string>("Colour6", "0,0,0"),
            new Tuple<string, string>("Colour7", "85,85,85"),
            new Tuple<string, string>("Colour8", "187,0,0"),
            new Tuple<string, string>("Colour9", "255,85,85"),
            new Tuple<string, string>("Colour10", "0,187,0"),
            new Tuple<string, string>("Colour11", "85,255,85"),
            new Tuple<string, string>("Colour12", "187,187,0"),
            new Tuple<string, string>("Colour13", "255,255,85"),
            new Tuple<string, string>("Colour14", "0,0,187"),
            new Tuple<string, string>("Colour15", "85,85,255"),
            new Tuple<string, string>("Colour16", "187,0,187"),
            new Tuple<string, string>("Colour17", "255,85,255"),
            new Tuple<string, string>("Colour18", "0,187,187"),
            new Tuple<string, string>("Colour19", "85,255,255"),
            new Tuple<string, string>("Colour20", "187,187,187"),
            new Tuple<string, string>("Colour21", "255,255,255"),
            new Tuple<string, string>("LineCodePage", "UTF-8"),
            new Tuple<string, string>("Font", "Consolas")
        };

        /// <summary>
        /// SZ registry keys for PuTTY profile NETworkManager if app theme is dark.
        /// </summary>
        /// <returns>List with SZ registry keys.</returns>
        private static List<Tuple<string, string>> GetProfileRegkeysSZDark()
        {
            return DefaultProfileRegkeysSZBase.Concat(
                new[] {
                    // new Tuple<string, string>("Colour0", "255,255,255"),
                    new Tuple<string, string>("Colour0", "187,187,187"),    // Foreground
                    new Tuple<string, string>("Colour2", "37,37,37")        // Background
                }).ToList();
        }

        /// <summary>
        /// SZ registry keys for PuTTY profile NETworkManager if app theme is white.
        /// </summary>
        /// <returns>List with DWORD registry keys.</returns>
        private static List<Tuple<string, string>> GetProfileRegkeysSZWhite()
        {
            return DefaultProfileRegkeysSZBase.Concat(
                new[] {
                    // new Tuple<string, string>("Colour0", "68,68,68"),
                    new Tuple<string, string>("Colour0", "0,0,0"),          // Foreground
                    new Tuple<string, string>("Colour2", "255,255,255")     // Background
                }).ToList();
        }

        /// <summary>
        /// Default DWORD registry keys for PuTTY profile NETworkManager.
        /// </summary>
        private static readonly List<Tuple<string, int>> DefaultProfileRegkeysDwordBase = new()
        {
            new Tuple<string, int>("CurType", 2),
            new Tuple<string, int>("FontHeight", 12),
            new Tuple<string, int>("BlinkCur", 1),
            new Tuple<string, int>("ScrollBar", 0)
        };

        /// <summary>
        /// Write the default PuTTY profile NETworkManager to the registry.
        /// HKCU\Software\SimonTatham\PuTTY\Sessions\NETworkManager
        /// </summary>
        /// <param name="theme">Current application theme to adjust the PuTTY colors</param>
        public static void WriteDefaultProfileToRegistry(string theme)
        {
            string profilePath = @"Software\SimonTatham\PuTTY\Sessions\NETworkManager";

            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(profilePath, true);

            registryKey ??= Registry.CurrentUser.CreateSubKey(profilePath);

            if (registryKey != null)
            {
                foreach (var item in theme == "Dark" ? GetProfileRegkeysSZDark() : GetProfileRegkeysSZWhite())
                    registryKey.SetValue(item.Item1, item.Item2);

                foreach (var item in DefaultProfileRegkeysDwordBase)
                    registryKey.SetValue(item.Item1, item.Item2);
            }

            registryKey.Close();
        }

        /// <summary>
        /// Build command line arguments based on a <see cref="PuTTYSessionInfo"/>.
        /// </summary>
        /// <param name="sessionInfo">Instance of <see cref="PuTTYSessionInfo"/>.</param>
        /// <returns>Command line arguments like "-ssh -l root -i C:\data\key.ppk".</returns>
        public static string BuildCommandLine(PuTTYSessionInfo sessionInfo)
        {
            var command = string.Empty;

            // Protocol
            switch (sessionInfo.Mode)
            {
                case ConnectionMode.SSH:
                    command += "-ssh";
                    break;
                case ConnectionMode.Telnet:
                    command += "-telnet";
                    break;
                case ConnectionMode.Serial:
                    command += "-serial";
                    break;
                case ConnectionMode.Rlogin:
                    command += "-rlogin";
                    break;
                case ConnectionMode.RAW:
                    command += "-raw";
                    break;
            }

            // Username
            if (!string.IsNullOrEmpty(sessionInfo.Username))
                command += $" -l {sessionInfo.Username}";

            // Private key
            if (!string.IsNullOrEmpty(sessionInfo.PrivateKey))
                command += $" -i {'"'}{sessionInfo.PrivateKey}{'"'}";

            // Profile
            if (!string.IsNullOrEmpty(sessionInfo.Profile))
                command += $" -load {'"'}{sessionInfo.Profile}{'"'}";

            // Log
            if (sessionInfo.EnableLog)
            {
                switch (sessionInfo.LogMode)
                {
                    case LogMode.SessionLog:
                        command += $" -sessionlog";
                        break;
                    case LogMode.SSHLog:
                        command += $" -sshlog";
                        break;
                    case LogMode.SSHRawLog:
                        command += $" -sshrawlog";
                        break;
                }

                command += $" {'"'}{Environment.ExpandEnvironmentVariables(Path.Combine(sessionInfo.LogPath, sessionInfo.LogFileName))}{'"'}";
            }

            // Additional commands
            if (!string.IsNullOrEmpty(sessionInfo.AdditionalCommandLine))
                command += $" {sessionInfo.AdditionalCommandLine}";

            // SerialLine, Baud
            if (sessionInfo.Mode == ConnectionMode.Serial)
                command += $" {sessionInfo.HostOrSerialLine} -sercfg {sessionInfo.PortOrBaud}";

            // Port, Host
            if (sessionInfo.Mode != ConnectionMode.Serial)
                command += $" -P {sessionInfo.PortOrBaud} {sessionInfo.HostOrSerialLine}";

            return command;
        }
    }
}
