namespace NETworkManager.Models.AWS
{
    public static partial class AWSSessionManager
    {
        private static string _encodingCommand = "[console]::InputEncoding = [console]::OutputEncoding = New-Object System.Text.UTF8Encoding";
        private static string _setLocationCommand = "Set-Location -Path ~";
        private static string _clearHostCommand = "Clear-Host";

        public static string BuildCommandLine(AWSSessionManagerSessionInfo sessionInfo)
        {
            var commandLine = $"-NoExit -NoLogo -NoProfile -Command \"{_encodingCommand}; {_setLocationCommand}; {_clearHostCommand}; aws ssm start-session --target {sessionInfo.InstanceID}";

            // Add profile
            if (!string.IsNullOrEmpty(sessionInfo.Profile))
                commandLine += $" --profile {sessionInfo.Profile}";

            // Add region
            if (!string.IsNullOrEmpty(sessionInfo.Region))
                commandLine += $" --region {sessionInfo.Region}";

            return $"{commandLine}\"";
        }
    }
}
