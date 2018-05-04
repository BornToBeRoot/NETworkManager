using NETworkManager.Utilities;
using NETworkManager.Views;

namespace NETworkManager.Controls
{
    public class DragablzHTTPHeadersTabItem : ViewModelBase
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

        public HTTPHeadersView View { get; set; }
        public int ID { get; set; }

        public DragablzHTTPHeadersTabItem(string header, HTTPHeadersView view, int id)
        {
            Header = header;
            View = view;
            ID = id;
        }
    }
}
