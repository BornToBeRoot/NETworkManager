using NETworkManager.Models.RemoteDesktop;
using System.Windows.Controls;

namespace NETworkManager.Controls
{
    public partial class RemoteDesktopControl : UserControl
    {
        public RemoteDesktopControl(RemoteDesktopSessionInfo info)
        {
            InitializeComponent();

            rdpClient.Server = info.Hostname;

            // AdvancedSettings
            rdpClient.AdvancedSettings9.AuthenticationLevel = 2;
            rdpClient.AdvancedSettings9.EnableCredSspSupport = true;
            rdpClient.AdvancedSettings9.RedirectClipboard = false;
            rdpClient.AdvancedSettings9.RedirectDevices = false;
            rdpClient.AdvancedSettings9.RedirectDrives = false;
            rdpClient.AdvancedSettings9.RedirectPorts = false;
            rdpClient.AdvancedSettings9.RedirectSmartCards = false;
            rdpClient.AdvancedSettings9.RedirectPrinters = false;

            // Display
            rdpClient.ColorDepth = 24;      // 8, 15, 16, 24
            rdpClient.DesktopHeight = 768;
            rdpClient.DesktopWidth = 1280;

            // Events
            rdpClient.OnDisconnected += Mstsc_OnDisconnected;

            rdpClient.Connect();
        }

        private void Mstsc_OnDisconnected(object sender, AxMSTSCLib.IMsTscAxEvents_OnDisconnectedEvent e)
        {
            
             // Add DC reason
        }
    }
}
