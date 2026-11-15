using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Dragablz
{
    /// <summary>
    /// Helper class to create view models, particularly for tool/MDI windows.
    /// </summary>
    public class HeaderedItemViewModel : INotifyPropertyChanged
    {
        private bool _isSelected;
        private object _header;
        private object _content;

        public HeaderedItemViewModel()
        {
        }

        public HeaderedItemViewModel(object header, object content, bool isSelected = false)
        {
            _header = header;
            _content = content;
            _isSelected = isSelected;
        }

        public object Header
        {
            get { return _header; }
            set
            {
                if (_header == value) return;
                _header = value;
#if NET40
                OnPropertyChanged("Header");
#else
                OnPropertyChanged();
#endif
            }
        }

        public object Content
        {
            get { return _content; }
            set
            {
                if (_content == value) return;
                _content = value;
#if NET40
                OnPropertyChanged("Content");
#else
                OnPropertyChanged();
#endif
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
#if NET40
                OnPropertyChanged("IsSelected");
#else
                OnPropertyChanged();
#endif
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

#if NET40
        protected virtual void OnPropertyChanged(string propertyName)
#else
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
#endif
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
