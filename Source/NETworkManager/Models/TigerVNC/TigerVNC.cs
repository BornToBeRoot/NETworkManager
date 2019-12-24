using NETworkManager.Models.Profile;
using NETworkManager.Models.Settings;

namespace NETworkManager.Models.TigerVNC
{
    public class TigerVNC
    {
        public static string BuildCommandLine(TigerVNCSessionInfo sessionInfo)
        {
            return $"{sessionInfo.Host}::{sessionInfo.Port}";
        }

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
