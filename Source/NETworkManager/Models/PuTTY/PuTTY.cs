namespace NETworkManager.Models.PuTTY
{
    public class PuTTY
    {
        public static string BuildCommandLine(PuTTYSessionInfo sessionInfo)
        {
            string command = string.Empty;

            // Protocol
            switch (sessionInfo.Mode)
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
            if (!string.IsNullOrEmpty(sessionInfo.Profile))
                command += string.Format(" -load {0}{1}{0}", '"', sessionInfo.Profile);

            // Username
            if (!string.IsNullOrEmpty(sessionInfo.Username))
                command += string.Format(" -l {0}", sessionInfo.Username);

            // Additional commands
            if (!string.IsNullOrEmpty(sessionInfo.AdditionalCommandLine))
                command += string.Format(" {0}", sessionInfo.AdditionalCommandLine);

            // SerialLine, Baud
            if (sessionInfo.Mode == ConnectionMode.Serial)
                command += string.Format(" {0} -sercfg {1}", sessionInfo.HostOrSerialLine, sessionInfo.PortOrBaud);

            // Port, Host
            if (sessionInfo.Mode != ConnectionMode.Serial)
                command += string.Format(" -P {0} {1}", sessionInfo.PortOrBaud, sessionInfo.HostOrSerialLine);

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
