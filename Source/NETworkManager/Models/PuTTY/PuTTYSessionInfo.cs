using NETworkManager.Models.Settings;
using static NETworkManager.Models.PuTTY.PuTTY;

namespace NETworkManager.Models.PuTTY
{
    public class PuTTYSessionInfo
    {
        public string ApplicationFilePath { get; set; }
        public string HostOrSerialLine { get; set; }
        public ConnectionMode Mode { get; set; }
        public int PortOrBaud { get; set; }
        public string Profile { get; set; }
        public string Username { get; set; }
        public string AdditionalCommandLine { get; set; }

        public PuTTYSessionInfo()
        {

        }

        public static PuTTYSessionInfo Parse(ProfileInfo profileInfo)
        {
            var info = new PuTTYSessionInfo
            {
                HostOrSerialLine = profileInfo.PuTTY_HostOrSerialLine,
                PortOrBaud = profileInfo.PuTTY_PortOrBaud,
                Mode = profileInfo.PuTTY_ConnectionMode,
                Username = profileInfo.PuTTY_Username,
                Profile = profileInfo.PuTTY_Profile,
                AdditionalCommandLine = profileInfo.PuTTY_AdditionalCommandLine
            };

            return info;
        }
    }
}
