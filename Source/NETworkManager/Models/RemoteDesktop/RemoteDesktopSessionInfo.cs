using System.Security;

namespace NETworkManager.Models.RemoteDesktop
{
    public class RemoteDesktopSessionInfo
    {
        public string Hostname { get; set; }
        public bool CustomCredentials { get; set; }
        public string Username { get; set; }
        public SecureString Password { get; set; }
        public bool AdjustScreenAutomatically { get; set; }
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

        public RemoteDesktopSessionInfo(string hostname, bool adjustScreenAutomatically, int desktopWidth, int desktopHeight, int colorDepth, bool redirectClipboard, bool redirectDevices, bool redirectDrives, bool redirectPorts, bool redirectSmartCards, bool redirectPrinters)
        {
            Hostname = hostname;
            AdjustScreenAutomatically = adjustScreenAutomatically;
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

        public RemoteDesktopSessionInfo(string hostname, bool customCredentials, string username, SecureString password, bool adjustScreenAutomatically, int desktopWidth, int desktopHeight, int colorDepth, bool redirectClipboard, bool redirectDevices, bool redirectDrives, bool redirectPorts, bool redirectSmartCards, bool redirectPrinters)
        {
            Hostname = hostname;
            CustomCredentials = customCredentials;
            Username = username;
            Password = password;
            AdjustScreenAutomatically = adjustScreenAutomatically;
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
