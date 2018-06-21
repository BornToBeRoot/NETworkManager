using NETworkManager.Models.Settings;
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

        public static PuTTYProfileInfo Parse(ProfileInfo profileInfo)
        {
            PuTTYProfileInfo info = new PuTTYProfileInfo
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
