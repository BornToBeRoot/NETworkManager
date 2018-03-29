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
    }
}
