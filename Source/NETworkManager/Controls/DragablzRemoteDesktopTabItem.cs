using NETworkManager.ViewModels;

namespace NETworkManager.Controls
{
    public class DragablzRemoteDesktopTabItem : ViewModelBase
    {
        public string Header { get; set; }
        public RemoteDesktopControl View { get; set; }

        public DragablzRemoteDesktopTabItem(string header, RemoteDesktopControl view)
        {
            Header = header;
            View = view;
        }
    }
}
