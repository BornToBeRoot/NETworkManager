using NETworkManager.Utilities;
using System;
using System.Windows.Input;

namespace NETworkManager.ViewModels
{
    public class ArpTableAddEntryViewModel : ViewModelBase
    {
        public ICommand AddCommand { get; }

        public ICommand CancelCommand { get; }

        private string _ipAddress;
        public string IPAddress
        {
            get => _ipAddress;
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
            get => _macAddress;
            set
            {
                if (value == _macAddress)
                    return;

                _macAddress = value;
                OnPropertyChanged();
            }
        }

        public ArpTableAddEntryViewModel(Action<ArpTableAddEntryViewModel> addCommand, Action<ArpTableAddEntryViewModel> cancelHandler)
        {
            AddCommand = new RelayCommand(p => addCommand(this));
            CancelCommand = new RelayCommand(p => cancelHandler(this));
        }        
    }
}