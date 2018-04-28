using NETworkManager.Utilities;
using NETworkManager.Views;

namespace NETworkManager.Controls
{
    public class DragablzTracerouteTabItem : ViewModelBase
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

        public TracerouteView View { get; set; }
        public int ID { get; set; }

        public DragablzTracerouteTabItem(string header, TracerouteView view, int id)
        {
            Header = header;
            View = view;
            ID = id;
        }
    }
}
