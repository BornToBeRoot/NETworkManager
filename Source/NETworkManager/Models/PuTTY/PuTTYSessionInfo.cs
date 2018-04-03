using static NETworkManager.Models.PuTTY.PuTTY;

namespace NETworkManager.Models.PuTTY
{
    public class PuTTYSessionInfo
    {
        public string PuTTYLocation { get; set; }
        public string Host { get; set; }
        public string SerialLine { get; set; }
        public ConnectionMode Mode { get; set; }
        public int Port { get; set; }
        public int Baud { get; set; }
        public string Profile { get; set; }
        public string Username { get; set; }
        public string AdditionalCommandLine { get; set; }

        public PuTTYSessionInfo()
        {

        }

        public static PuTTYSessionInfo Parse(Settings.PuTTYSessionInfo sessionInfo)
        {            
            return new PuTTYSessionInfo
            {
                Host = sessionInfo.Host,
                SerialLine = sessionInfo.SerialLine,
                Mode = sessionInfo.ConnectionMode,
                Port = sessionInfo.Port,
                Baud = sessionInfo.Baud,
                Username = sessionInfo.Username,
                Profile = sessionInfo.Profile,
                AdditionalCommandLine = sessionInfo.AdditionalCommandLine
            };
        }
    }
}
