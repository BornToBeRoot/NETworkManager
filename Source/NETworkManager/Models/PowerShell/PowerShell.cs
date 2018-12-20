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
