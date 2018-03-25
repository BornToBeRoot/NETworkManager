using NETworkManager.Utilities;

namespace NETworkManager.Controls
{
    public class DragablzPuTTYTabItem : ViewModelBase
    {
        public string Header { get; set; }
        public PuTTYControl View { get; set; }

        public DragablzPuTTYTabItem(string header, PuTTYControl view)
        {
            Header = header;
            View = view;
        }
    }
}
