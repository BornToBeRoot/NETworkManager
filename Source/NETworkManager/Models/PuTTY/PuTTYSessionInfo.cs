using static NETworkManager.Models.PuTTY.PuTTY;

namespace NETworkManager.Models.PuTTY
{
    public class PuTTYProfileInfo
    {
        public string PuTTYLocation { get; set; }
        public string HostOrSerialLine { get; set; }
        public ConnectionMode Mode { get; set; }
        public int PortOrBaud { get; set; }
        public string Profile { get; set; }
        public string Username { get; set; }
        public string AdditionalCommandLine { get; set; }

        public PuTTYProfileInfo()
        {

        }

        public static PuTTYProfileInfo Parse(Settings.PuTTYProfileInfo ProfileInfo)
        {
            PuTTYProfileInfo info = new PuTTYProfileInfo();

            info.HostOrSerialLine = ProfileInfo.HostOrSerialLine;
            info.PortOrBaud = ProfileInfo.PortOrBaud;
            info.Mode = ProfileInfo.ConnectionMode;
            info.Username = ProfileInfo.Username;
            info.Profile = ProfileInfo.Profile;
            info.AdditionalCommandLine = ProfileInfo.AdditionalCommandLine;

            return info;
        }
    }
}
