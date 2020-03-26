using NETworkManager.Models.Profile;
using NETworkManager.Models.Settings;
using static NETworkManager.Models.PuTTY.PuTTY;

namespace NETworkManager.Models.PuTTYTMP
{
    public class PuTTYSessionInfo
    {
        public string ApplicationFilePath { get; set; }
        public ConnectionMode Mode { get; set; }
        public string HostOrSerialLine { get; set; }
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
                Mode = profileInfo.PuTTY_ConnectionMode,
                HostOrSerialLine = profileInfo.PuTTY_HostOrSerialLine,
              // TODO:  PortOrBaud = profileInfo.PuTTY_OverridePortOrBaud ? profileInfo.PuTTY_PortOrBaud : GetPortOrBaudByConnectionMode(profileInfo.PuTTY_ConnectionMode),
                Username = profileInfo.PuTTY_OverrideUsername ? profileInfo.PuTTY_Username : SettingsManager.Current.PuTTY_Username,
                Profile = profileInfo.PuTTY_OverrideProfile ? profileInfo.PuTTY_Profile : SettingsManager.Current.PuTTY_Profile,
                AdditionalCommandLine = profileInfo.PuTTY_OverrideAdditionalCommandLine ? profileInfo.PuTTY_AdditionalCommandLine : SettingsManager.Current.PuTTY_AdditionalCommandLine
            };

            return info;
        }
    }
}
