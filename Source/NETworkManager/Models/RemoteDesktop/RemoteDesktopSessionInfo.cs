namespace NETworkManager.Models.RemoteDesktop
{
    public class RemoteDesktopSessionInfo
    {
        public string Hostname { get; set; }
        public int DesktopWidth { get; set; }
        public int DesktopHeight { get; set; }
        public int ColorDepth { get; set; }
        public bool RedirectClipboard { get; set; }
        public bool RedirectDevices { get; set; }
        public bool RedirectDrives { get; set; }
        public bool RedirectPorts { get; set; }
        public bool RedirectSmartCards { get; set; }
        public bool RedirectPrinters { get; set; }

        public RemoteDesktopSessionInfo()
        {

        }

        public RemoteDesktopSessionInfo(string hostname, int desktopWidth, int desktopHeight, int colorDepth, bool redirectClipboard, bool redirectDevices, bool redirectDrives, bool redirectPorts, bool redirectSmartCards, bool redirectPrinters)
        {
            Hostname = hostname;
            DesktopWidth = desktopWidth;
            DesktopHeight = desktopHeight;
            ColorDepth = colorDepth;
            RedirectClipboard = redirectClipboard;
            RedirectDevices = redirectDevices;
            RedirectDrives = redirectDrives;
            RedirectPorts = redirectPorts;
            RedirectSmartCards = redirectSmartCards;
            RedirectPrinters = redirectPrinters;
        }
    }
}
