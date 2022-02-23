
using NETworkManager.Models.TigerVNC;
using NETworkManager.Settings;

namespace NETworkManager.Profiles.Application
{
    public static class TigerVNC
    {
        public static TigerVNCSessionInfo CreateSessionInfo(ProfileInfo profile)
        {
            // Get group info
            GroupInfo group = ProfileManager.GetGroup(profile.Group);

            return new TigerVNCSessionInfo
            {
                Host = profile.TigerVNC_Host,

                Port = profile.TigerVNC_OverridePort ? profile.TigerVNC_Port : (group.TigerVNC_OverridePort ? group.TigerVNC_Port : SettingsManager.Current.TigerVNC_Port)
            };
        }
    }
}
