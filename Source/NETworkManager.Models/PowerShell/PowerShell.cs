namespace NETworkManager.Models.PowerShell
{
    public static partial class PowerShell
    {
        public static string BuildCommandLine(PowerShellSessionInfo sessionInfo)
        {
            var command = $"-NoExit -ExecutionPolicy {sessionInfo.ExecutionPolicy} {sessionInfo.AdditionalCommandLine}";

            if (sessionInfo.EnableRemoteConsole)
                command += $" -Command \"Enter-PSSession -ComputerName {sessionInfo.Host}\"";

            return command;
        }
    }
}
