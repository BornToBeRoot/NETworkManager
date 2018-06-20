namespace NETworkManager.Models.PuTTY
{
    public class PuTTY
    {
        public static string BuildCommandLine(PuTTYProfileInfo ProfileInfo)
        {
            string command = string.Empty;

            // Protocol
            switch (ProfileInfo.Mode)
            {
                case ConnectionMode.SSH:
                    command += string.Format("-ssh");
                    break;
                case ConnectionMode.Telnet:
                    command += string.Format("-telnet");
                    break;
                case ConnectionMode.Serial:
                    command += string.Format("-serial");
                    break;
                case ConnectionMode.Rlogin:
                    command += string.Format("-rlogin");
                    break;
                case ConnectionMode.RAW:
                    command += string.Format("-raw");
                    break;
            }

            // Profile
            if (!string.IsNullOrEmpty(ProfileInfo.Profile))
                command += string.Format(" -load {0}{1}{0}", '"', ProfileInfo.Profile);

            // Username
            if (!string.IsNullOrEmpty(ProfileInfo.Username))
                command += string.Format(" -l {0}", ProfileInfo.Username);

            // Additional commands
            if (!string.IsNullOrEmpty(ProfileInfo.AdditionalCommandLine))
                command += string.Format(" {0}", ProfileInfo.AdditionalCommandLine);

            // SerialLine, Baud
            if (ProfileInfo.Mode == ConnectionMode.Serial)
                command += string.Format(" {0} -sercfg {1}", ProfileInfo.HostOrSerialLine, ProfileInfo.PortOrBaud);

            // Port, Host
            if (ProfileInfo.Mode != ConnectionMode.Serial)
                command += string.Format(" -P {0} {1}", ProfileInfo.PortOrBaud, ProfileInfo.HostOrSerialLine);

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
