namespace NETworkManager.Models.AWS
{
    public static partial class AWSSessionManager
    {
        public static string BuildCommandLine(AWSSessionManagerSessionInfo sessionInfo)
        {
            var commandLine = $"-NoExit -NoLogo -NoProfile -Command \"Set-Location -Path ~; Clear-Host; aws ssm start-session --target {sessionInfo.InstanceID}";

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
