using System;
using System.IO;

namespace NETworkManager.Models.PuTTY
{
    /// <summary>
    /// Class control PuTTY.
    /// </summary>
    public partial class PuTTY
    {
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

                command += $" {'"'}{ Environment.ExpandEnvironmentVariables(Path.Combine(sessionInfo.LogPath, sessionInfo.LogFileName))}{'"'}";
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
