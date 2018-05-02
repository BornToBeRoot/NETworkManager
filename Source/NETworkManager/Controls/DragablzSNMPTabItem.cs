using NETworkManager.Utilities;
using NETworkManager.Views;

namespace NETworkManager.Controls
{
    public class DragablzSNMPTabItem : ViewModelBase
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

        public SNMPView View { get; set; }
        public int ID { get; set; }

        public DragablzSNMPTabItem(string header, SNMPView view, int id)
        {
            Header = header;
            View = view;
            ID = id;
        }
    }
}
