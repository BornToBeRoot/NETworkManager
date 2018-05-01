using NETworkManager.Utilities;
using NETworkManager.Views;

namespace NETworkManager.Controls
{
    public class DragablzPortScannerTabItem : ViewModelBase
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

        public PortScannerView View { get; set; }
        public int ID { get; set; }

        public DragablzPortScannerTabItem(string header, PortScannerView view, int id)
        {
            Header = header;
            View = view;
            ID = id;
        }
    }
}
