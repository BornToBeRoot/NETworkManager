using static NETworkManager.Models.PuTTY.PuTTY;

namespace NETworkManager.Models.PuTTY
{
    public class PuTTYSessionInfo
    {
        public string PuTTYLocation { get; set; }
        public string HostOrSerialLine { get; set; }
        public ConnectionMode Mode { get; set; }
        public int PortOrBaud { get; set; }
        public string Profile { get; set; }
        public string Username { get; set; }
        public string AdditionalCommandLine { get; set; }

        public PuTTYSessionInfo()
        {

        }

        public static PuTTYSessionInfo Parse(Settings.PuTTYSessionInfo sessionInfo)
        {
            PuTTYSessionInfo info = new PuTTYSessionInfo();

            info.HostOrSerialLine = sessionInfo.HostOrSerialLine;
            info.PortOrBaud = sessionInfo.PortOrBaud;
            info.Mode = sessionInfo.ConnectionMode;
            info.Username = sessionInfo.Username;
            info.Profile = sessionInfo.Profile;
            info.AdditionalCommandLine = sessionInfo.AdditionalCommandLine;

            return info;
        }
    }
}
