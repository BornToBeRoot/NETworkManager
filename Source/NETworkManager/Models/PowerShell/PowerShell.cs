using NETworkManager.Models.Profile;
using NETworkManager.Models.Settings;

namespace NETworkManager.Models.PowerShell
{
    public class PowerShell
    {
        public static string BuildCommandLine(PowerShellSessionInfo sessionInfo)
        {
            var command = $"-ExecutionPolicy {sessionInfo.ExecutionPolicy} {sessionInfo.AdditionalCommandLine}";

            if (sessionInfo.EnableRemoteConsole)
                command += $" -NoExit -Command \"Enter-PSSession -ComputerName {sessionInfo.Host}\"";

            return command;
        }

        public static PowerShellSessionInfo CreateSessionInfo(ProfileInfo profileInfo)
        {
            var info = new PowerShellSessionInfo
            {
                EnableRemoteConsole = profileInfo.PowerShell_EnableRemoteConsole,
                Host = profileInfo.Host,
                AdditionalCommandLine = profileInfo.PowerShell_OverrideAdditionalCommandLine ? profileInfo.PowerShell_AdditionalCommandLine : SettingsManager.Current.PowerShell_AdditionalCommandLine,
                ExecutionPolicy = profileInfo.PowerShell_OverrideExecutionPolicy ? profileInfo.PowerShell_ExecutionPolicy : SettingsManager.Current.PowerShell_ExecutionPolicy
            };

            return info;
        }

        public enum ExecutionPolicy
        {
            Restricted,
            AllSigned,
            RemoteSigned,
            Unrestricted,
            Bypass
        }
    }
}
