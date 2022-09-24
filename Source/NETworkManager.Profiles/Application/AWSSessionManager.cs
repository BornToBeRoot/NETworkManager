using NETworkManager.Models.AWS;

namespace NETworkManager.Profiles.Application
{
    public class AWSSessionManager
    {        
        public static AWSSessionManagerSessionInfo CreateSessionInfo(ProfileInfo profile)
        {
            return new AWSSessionManagerSessionInfo
            {       
                InstanceID = profile.AWSSessionManager_InstanceID,
                Profile = profile.AWSSessionManager_Profile,
                Region = profile.AWSSessionManager_Region
            };
        }
    }
}
