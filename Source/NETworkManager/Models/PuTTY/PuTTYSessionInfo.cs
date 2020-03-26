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

 
    }
}
