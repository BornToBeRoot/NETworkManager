namespace NETworkManager.Models.PuTTY
{
    public class PuTTY
    {
        public static string BuildCommandLine(PuTTYProfileInfo ProfileInfo)
        {
            var command = string.Empty;

            // Protocol
            switch (ProfileInfo.Mode)
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
            if (!string.IsNullOrEmpty(ProfileInfo.Profile))
                command += $" -load {'"'}{ProfileInfo.Profile}{'"'}";

            // Username
            if (!string.IsNullOrEmpty(ProfileInfo.Username))
                command += $" -l {ProfileInfo.Username}";

            // Additional commands
            if (!string.IsNullOrEmpty(ProfileInfo.AdditionalCommandLine))
                command += $" {ProfileInfo.AdditionalCommandLine}";

            // SerialLine, Baud
            if (ProfileInfo.Mode == ConnectionMode.Serial)
                command += $" {ProfileInfo.HostOrSerialLine} -sercfg {ProfileInfo.PortOrBaud}";

            // Port, Host
            if (ProfileInfo.Mode != ConnectionMode.Serial)
                command += $" -P {ProfileInfo.PortOrBaud} {ProfileInfo.HostOrSerialLine}";

            return command;
        }

        #region enum
        public enum ConnectionMode
        {
            SSH,
            Telnet,
            Serial,
            Rlogin,
            RAW
        }
        #endregion
    }
}
