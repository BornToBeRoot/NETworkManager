using NETworkManager.ViewModels;

namespace NETworkManager.Controls
{
    public class DragablzRemoteDesktopTabItem : ViewModelBase
    {
        public string Header { get; set; }
        public RemoteDesktopControl Control { get; set; }

        public DragablzRemoteDesktopTabItem(string header, RemoteDesktopControl control)
        {
            Header = header;
            Control = control;
        }
    }
}
