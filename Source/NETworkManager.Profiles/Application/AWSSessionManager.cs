using NETworkManager.Models.AWS;
using NETworkManager.Settings;

namespace NETworkManager.Profiles.Application
{
    public class AWSSessionManager
    {        
        public static AWSSessionManagerSessionInfo CreateSessionInfo(ProfileInfo profile)
        {
            // Get group info
            GroupInfo group = ProfileManager.GetGroup(profile.Group);

            return new AWSSessionManagerSessionInfo
            {       
                InstanceID = profile.AWSSessionManager_InstanceID,
                Profile = profile.AWSSessionManager_OverrideProfile ? profile.AWSSessionManager_Profile : (group.AWSSessionManager_OverrideProfile ? group.AWSSessionManager_Profile : SettingsManager.Current.AWSSessionManager_DefaultProfile),
                Region = profile.AWSSessionManager_OverrideRegion ? profile.AWSSessionManager_Region : (group.AWSSessionManager_OverrideRegion ? group.AWSSessionManager_Region : SettingsManager.Current.AWSSessionManager_DefaultRegion)
            };
        }
    }
}
