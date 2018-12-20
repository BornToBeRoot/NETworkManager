using NETworkManager.Models.Settings;

namespace NETworkManager.Models.TightVNC
{
    public class TightVNCSessionInfo
    {
        public string ApplicationFilePath { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        
        public TightVNCSessionInfo()
        {

        }

        public static TightVNCSessionInfo Parse(ProfileInfo profileInfo)
        {
            var info = new TightVNCSessionInfo
            {
                Host = profileInfo.TightVNC_Host,
                Port = profileInfo.TightVNC_Port
            };

            return info;
        }
    }
}
