namespace NETworkManager.Models.AWS
{
    public static partial class AWSSessionManager
    {
        public static string BuildCommandLine(AWSSessionManagerSessionInfo sessionInfo)
        {
            return $"-NoExit -NoLogo -NoProfile -Command \"aws ssm start-session --target {sessionInfo.InstanceID} --profile {sessionInfo.Profile} --region {sessionInfo.Region}\"";           
        }
    }
}
