using NETworkManager.Models.PuTTY;
using System.IO;

namespace NETworkManager.Settings.Application
{
    public static class PuTTY
    {
        public static string PortableLogPath => Path.Combine(ConfigurationManager.Current.ExecutionPath, "PuTTY_LogFiles");

        public static string LogPath => ConfigurationManager.Current.IsPortable ? PortableLogPath : SettingsManager.Current.PuTTY_LogPath;

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
