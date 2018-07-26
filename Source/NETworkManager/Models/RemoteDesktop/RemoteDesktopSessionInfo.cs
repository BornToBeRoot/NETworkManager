using System.Security;

namespace NETworkManager.Models.RemoteDesktop
{
    public class RemoteDesktopSessionInfo
    {
        public string Hostname { get; set; }
        public int Port { get; set; }
        public bool CustomCredentials { get; set; }
        public string Username { get; set; }
        public SecureString Password { get; set; }
        public bool AdjustScreenAutomatically { get; set; }
        public bool UseCurrentViewSize { get; set; }
        public int DesktopWidth { get; set; }
        public int DesktopHeight { get; set; }
        public int ColorDepth { get; set; }
        public bool EnableCredSspSupport { get; set; }
        public uint AuthenticationLevel { get; set; }
        public int KeyboardHookMode { get; set; }
        public bool RedirectClipboard { get; set; }
        public bool RedirectDevices { get; set; }
        public bool RedirectDrives { get; set; }
        public bool RedirectPorts { get; set; }
        public bool RedirectSmartCards { get; set; }
        public bool RedirectPrinters { get; set; }        

        public RemoteDesktopSessionInfo()
        {

        }
    }
}
