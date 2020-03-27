using NETworkManager.Models.Profile;
using NETworkManager.Settings;
using static NETworkManager.Models.PuTTY.PuTTY;

namespace NETworkManager.Models.PuTTYTMP
{
    public partial class PuTTY
    {
        public static string BuildCommandLine(PuTTYSessionInfo profileInfo)
        {
            var command = string.Empty;

            // Protocol
            switch (profileInfo.Mode)
            {
                case ConnectionMode.SSH:
                    command += "-ssh";
                    break;
                case ConnectionMode.Telnet:
                    command += "-telnet";
                    break;
                case ConnectionMode.Serial:
                    command += "-serial";
                    break;
                case ConnectionMode.Rlogin:
                    command += "-rlogin";
                    break;
                case ConnectionMode.RAW:
                    command += "-raw";
                    break;
            }

            // Profile
            if (!string.IsNullOrEmpty(profileInfo.Profile))
                command += $" -load {'"'}{profileInfo.Profile}{'"'}";

            // Username
            if (!string.IsNullOrEmpty(profileInfo.Username))
                command += $" -l {profileInfo.Username}";

            // Additional commands
            if (!string.IsNullOrEmpty(profileInfo.AdditionalCommandLine))
                command += $" {profileInfo.AdditionalCommandLine}";

            // SerialLine, Baud
            if (profileInfo.Mode == ConnectionMode.Serial)
                command += $" {profileInfo.HostOrSerialLine} -sercfg {profileInfo.PortOrBaud}";

            // Port, Host
            if (profileInfo.Mode != ConnectionMode.Serial)
                command += $" -P {profileInfo.PortOrBaud} {profileInfo.HostOrSerialLine}";

            return command;
        }

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
