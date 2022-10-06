namespace NETworkManager.Models.PowerShell
{
    public static partial class PowerShell
    {
        public static string BuildCommandLine(PowerShellSessionInfo sessionInfo)
        {
            var command = $"-NoExit -ExecutionPolicy {sessionInfo.ExecutionPolicy} {sessionInfo.AdditionalCommandLine}";

            // Connect to remote host or execute local command if configured
            if (sessionInfo.EnableRemoteConsole)
                command += $" -Command \"& {{Enter-PSSession -ComputerName {sessionInfo.Host}}}\"";
            else if(!string.IsNullOrEmpty(sessionInfo.Command))
                command += $" -Command \"& {{{sessionInfo.Command}}}\"";

            return command;
        }
    }
}
