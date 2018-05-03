using NETworkManager.Utilities;
using NETworkManager.Views;

namespace NETworkManager.Controls
{
    public class DragablzDNSLookupTabItem : ViewModelBase
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

        public DNSLookupView View { get; set; }
        public int ID { get; set; }

        public DragablzDNSLookupTabItem(string header, DNSLookupView view, int id)
        {
            Header = header;
            View = view;
            ID = id;
        }
    }
}
