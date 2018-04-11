using static NETworkManager.Models.PuTTY.PuTTY;

namespace NETworkManager.Models.PuTTY
{
    public class PuTTYSessionInfo
    {
        public string PuTTYLocation { get; set; }
        public string Host { get; set; }
        public string SerialLine { get; set; }
        public ConnectionMode Mode { get; set; }
        public int Port { get; set; }
        public int Baud { get; set; }
        public string Profile { get; set; }
        public string Username { get; set; }
        public string AdditionalCommandLine { get; set; }

        public PuTTYSessionInfo()
        {

        }
                
        public static PuTTYSessionInfo Parse(Settings.PuTTYSessionInfo sessionInfo)
        {
            PuTTYSessionInfo info = new PuTTYSessionInfo();

            if(sessionInfo.ConnectionMode == ConnectionMode.Serial)
            {
                info.SerialLine = sessionInfo.SerialLine;
                info.Baud = sessionInfo.Baud;
            }
            else
            {
                info.Host = sessionInfo.Host;
                info.Port = sessionInfo.Port;

            }

            info.Mode = sessionInfo.ConnectionMode;
            info.Username = sessionInfo.Username;
            info.Profile = sessionInfo.Profile;
            info.AdditionalCommandLine = sessionInfo.AdditionalCommandLine;

            return info;
        }
    }
}
