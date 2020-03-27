using NETworkManager.Models.PowerShell;
using NETworkManager.Models.Profile;
using NETworkManager.Settings;

namespace NETworkManager.Models.PowerShellTmp
{
    public class PowerShell
    {        
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
    }
}
