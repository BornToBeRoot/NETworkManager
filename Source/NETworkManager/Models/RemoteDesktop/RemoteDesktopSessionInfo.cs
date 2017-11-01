namespace NETworkManager.Models.RemoteDesktop
{
    public class RemoteDesktopSessionInfo
    {
        public string Hostname { get; set; }
        public bool RedirectClipboard { get; set; }
        public bool RedirectDevices { get; set; }
        public bool RedirectDrives { get; set; }
        public bool RedirectPorts { get; set; }
        public bool RedirectSmartCards { get; set; }
        public bool RedirectPrinters { get; set; }

        public RemoteDesktopSessionInfo()
        {

        }

        public RemoteDesktopSessionInfo(string hostname, bool redirectClipboard, bool redirectDevices, bool redirectDrives, bool redirectPorts, bool redirectSmartCards, bool redirectPrinters)
        {
            Hostname = hostname;
            RedirectClipboard = redirectClipboard;
            RedirectDevices = redirectDevices;
            RedirectDrives = redirectDrives;
            RedirectPorts = redirectPorts;
            RedirectSmartCards = redirectSmartCards;
            RedirectPrinters = redirectPrinters;
        }
    }
}
