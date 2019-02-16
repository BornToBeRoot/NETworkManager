using NETworkManager.Models.Settings;

namespace NETworkManager.Models.TightVNC
{
    public class TightVNC
    {
        public static string BuildCommandLine(TightVNCSessionInfo sessionInfo)
        {
            return $"{sessionInfo.Host}::{sessionInfo.Port}";
        }

        public static TightVNCSessionInfo CreateSessionInfo(ProfileInfo profileInfo)
        {
            var info = new TightVNCSessionInfo
            {
                Host = profileInfo.TightVNC_Host,
                Port = profileInfo.TightVNC_OverridePort ? profileInfo.TightVNC_Port : SettingsManager.Current.TightVNC_Port
            };

            return info;
        }
    }
}
