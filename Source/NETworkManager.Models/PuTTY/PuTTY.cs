using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;

namespace NETworkManager.Models.PuTTY
{
    /// <summary>
    /// Class control PuTTY.
    /// </summary>
    public partial class PuTTY
    {
        private static string _puttyFolder => "PuTTY";
        private static string _puttyFile => "putty.exe";

        public static readonly List<string> GetDefaultInstallationPaths = new()
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), _puttyFolder, _puttyFile),
            Path.Combine(Environment.GetFolderPath( Environment.SpecialFolder.ProgramFilesX86), _puttyFolder, _puttyFile)
        };

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

        private static List<Tuple<string, string>> GetProfileRegkeysSZDark()
        {
            return DefaultProfileRegkeysSZBase.Concat(
                new[] {
                    // new Tuple<string, string>("Colour0", "255,255,255"),
                    new Tuple<string, string>("Colour0", "187,187,187"),    // Foreground
                    new Tuple<string, string>("Colour2", "37,37,37")        // Background
                }).ToList();
        }

        private static List<Tuple<string, string>> GetProfileRegkeysSZWhite()
        {
            return DefaultProfileRegkeysSZBase.Concat(
                new[] {
                    // new Tuple<string, string>("Colour0", "68,68,68"),
                    new Tuple<string, string>("Colour0", "0,0,0"),          // Foreground
                    new Tuple<string, string>("Colour2", "255,255,255")     // Background
                }).ToList();
        }

        private static readonly List<Tuple<string, int>> DefaultProfileRegkeysDword = new()
        {
            new Tuple<string, int>("CurType", 2),
            new Tuple<string, int>("FontHeight", 12),
            new Tuple<string, int>("BlinkCur", 1),
            new Tuple<string, int>("ScrollBar", 0)
        };

        /// <summary>
        /// Build command line arguments based on a <see cref="PuTTYSessionInfo"/>.
        /// </summary>
        /// <param name="sessionInfo">Instance of <see cref="PuTTYSessionInfo"/>.</param>
        /// <returns>Command line arguments like "-ssh -l root -i C:\data\key.ppk"</returns>
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

        /// <summary>
        /// 
        /// </summary>
        public static void WriteDefaultProfileToRegistry(string AccentName)
        {
            string profilePath = @"Software\SimonTatham\PuTTY\Sessions\NETworkManager";

            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(profilePath, true);

            if (registryKey == null)
                registryKey = Registry.CurrentUser.CreateSubKey(profilePath);

            if (registryKey != null)
            {
                foreach (Tuple<string, string> key in AccentName == "Dark" ? GetProfileRegkeysSZDark() : GetProfileRegkeysSZWhite())
                    registryKey.SetValue(key.Item1, key.Item2);

                foreach (var key in DefaultProfileRegkeysDword)
                    registryKey.SetValue(key.Item1, key.Item2);
            }

            registryKey.Close();
        }
    }
}
