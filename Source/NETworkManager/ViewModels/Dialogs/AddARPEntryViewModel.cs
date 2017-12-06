using System;
using System.Windows.Input;

namespace NETworkManager.ViewModels.Dialogs
{
    public class AddARPEntryViewModel : ViewModelBase
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

        public AddARPEntryViewModel(Action<AddARPEntryViewModel> saveCommand, Action<AddARPEntryViewModel> cancelHandler)
        {
            _addCommand = new RelayCommand(p => saveCommand(this));
            _cancelCommand = new RelayCommand(p => cancelHandler(this));
        }        
    }
}