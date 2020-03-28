using NETworkManager.Models.PuTTY;
using NETworkManager.Profiles;
using NETworkManager.Settings;
using static NETworkManager.Models.PuTTY.PuTTY;

namespace NETworkManager.Models.PuTTYTMP
{
    public partial class PuTTY
    {
      
        public static int GetPortOrBaudByConnectionMode(ConnectionMode mode)
        {
            var portOrBaud = 0;

            switch (mode)
            {
                case ConnectionMode.SSH:
                    portOrBaud = SettingsManager.Current.PuTTY_SSHPort;
                    break;
                case ConnectionMode.Telnet:
                    portOrBaud = SettingsManager.Current.PuTTY_TelnetPort;
                    break;
                case ConnectionMode.Rlogin:
                    portOrBaud = SettingsManager.Current.PuTTY_RloginPort;
                    break;
                case ConnectionMode.RAW:
                    portOrBaud = SettingsManager.Current.PuTTY_DefaultRaw;
                    break;
                case ConnectionMode.Serial:
                    portOrBaud = SettingsManager.Current.PuTTY_BaudRate;
                    break;
            }

            return portOrBaud;
        }

        public static PuTTYSessionInfo CreateSessionInfoFromProfile(ProfileInfo profileInfo)
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
