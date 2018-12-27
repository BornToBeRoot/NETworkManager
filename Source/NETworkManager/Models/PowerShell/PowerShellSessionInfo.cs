using NETworkManager.Models.Settings;
using static NETworkManager.Models.PowerShell.PowerShell;

namespace NETworkManager.Models.PowerShell
{
    public class PowerShellSessionInfo
    {
        public string ApplicationFilePath { get; set; }
        public bool EnableRemoteConsole { get; set; }
        public string Host { get; set; }
        public string AdditionalCommandLine { get; set; }
        public ExecutionPolicy ExecutionPolicy { get; set; }

        public PowerShellSessionInfo()
        {

        }

        public static PowerShellSessionInfo Parse(ProfileInfo profileInfo)
        {
            var info = new PowerShellSessionInfo
            {
                EnableRemoteConsole = profileInfo.PowerShell_EnableRemoteConsole,
                Host = profileInfo.Host,
                AdditionalCommandLine = profileInfo.PowerShell_AdditionalCommandLine,
                ExecutionPolicy = profileInfo.PowerShell_ExecutionPolicy
            };

            return info;
        }
    }
}
