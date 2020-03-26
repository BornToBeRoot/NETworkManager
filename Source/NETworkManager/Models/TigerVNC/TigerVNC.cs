using NETworkManager.Models.Profile;
using NETworkManager.Models.Settings;
using NETworkManager.Models.TigerVNC;

namespace NETworkManager.Models.TigerVNCTMP
{
    public class TigerVNC
    {       
        public static TigerVNCSessionInfo CreateSessionInfo(ProfileInfo profileInfo)
        {
            var info = new TigerVNCSessionInfo
            {
                Host = profileInfo.TigerVNC_Host,
                Port = profileInfo.TigerVNC_OverridePort ? profileInfo.TigerVNC_Port : SettingsManager.Current.TigerVNC_Port
            };

            return info;
        }
    }
}
