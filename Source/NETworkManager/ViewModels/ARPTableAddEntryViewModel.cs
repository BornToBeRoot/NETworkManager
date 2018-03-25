using NETworkManager.Utilities;
using System;
using System.Windows.Input;

namespace NETworkManager.ViewModels
{
    public class ARPTableAddEntryViewModel : ViewModelBase
    {
        private readonly ICommand _addCommand;
        public ICommand AddCommand
        {
            get { return _addCommand; }
        }

        private readonly ICommand _cancelCommand;
        public ICommand CancelCommand
        {
            get { return _cancelCommand; }
        }

        private string _ipAddress;
        public string IPAddress
        {
            get { return _ipAddress; }
            set
            {
                if (value == _ipAddress)
                    return;

                _ipAddress = value;
                OnPropertyChanged();
            }
        }

        private string _macAddress;
        public string MACAddress
        {
            get { return _macAddress; }
            set
            {
                if (value == _macAddress)
                    return;

                _macAddress = value;
                OnPropertyChanged();
            }
        }

        public ARPTableAddEntryViewModel(Action<ARPTableAddEntryViewModel> addCommand, Action<ARPTableAddEntryViewModel> cancelHandler)
        {
            _addCommand = new RelayCommand(p => addCommand(this));
            _cancelCommand = new RelayCommand(p => cancelHandler(this));
        }        
    }
}