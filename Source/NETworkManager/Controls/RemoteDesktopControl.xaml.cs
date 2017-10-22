using NETworkManager.Models.RemoteDesktop;
using System.Windows.Controls;

namespace NETworkManager.Controls
{
    public partial class RemoteDesktopControl : UserControl
    {
        public RemoteDesktopControl(RemoteDesktopSessionInfo info)
        {
            InitializeComponent();
            
            mstsc.Server = info.Hostname;

            // Auth
            mstsc.AdvancedSettings9.EnableCredSspSupport = true;

            // Display
            mstsc.DesktopHeight = 768;
            mstsc.DesktopWidth = 1280;


            mstsc.Connect();
        }
    }
}
