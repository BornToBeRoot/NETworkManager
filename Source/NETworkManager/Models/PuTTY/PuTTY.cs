using NETworkManager.Models.Settings;
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
        
    }
}
