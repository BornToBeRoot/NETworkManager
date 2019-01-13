using NETworkManager.Models.Settings;

namespace NETworkManager.Models.TightVNC
{
    public class TightVNCSessionInfo
    {
        public string ApplicationFilePath { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        
        public TightVNCSessionInfo()
        {

        }
    }
}
