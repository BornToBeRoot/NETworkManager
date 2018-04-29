using NETworkManager.Utilities;
using NETworkManager.Views;

namespace NETworkManager.Controls
{
    public class DragablzIPScannerTabItem : ViewModelBase
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

        public IPScannerView View { get; set; }
        public int ID { get; set; }

        public DragablzIPScannerTabItem(string header, IPScannerView view, int id)
        {
            Header = header;
            View = view;
            ID = id;
        }
    }
}
