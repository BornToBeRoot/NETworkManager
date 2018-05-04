using NETworkManager.Utilities;
using System.Windows.Controls;

namespace NETworkManager.Controls
{
    public class DragablzTabItem : ViewModelBase
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

        public UserControl View { get; set; }
        public int ID { get; set; }

        public DragablzTabItem(string header, UserControl view)
        {
            Header = header;
            View = view;         
        }

        public DragablzTabItem(string header, UserControl view, int id)
        {
            Header = header;
            View = view;
            ID = id;
        }
    }
}
