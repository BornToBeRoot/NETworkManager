using NETworkManager.ViewModels;
using NETworkManager.Views.Applications;

namespace NETworkManager.Controls
{
    public class DragablzPingTabItem : ViewModelBase
    {
        private string _header;
        public string Header
        {
            get { return _header; }
            set
            {
                if (value == _header)
                    return;

                _header = value;
                OnPropertyChanged();
            }
        }

        public PingView View { get; set; }
        public int ID { get; set; }

        public DragablzPingTabItem(string header, PingView view, int id)
        {
            Header = header;
            View = view;
            ID = id;
        }
    }
}
