using NETworkManager.Utilities;
using System;
using System.Windows.Input;

namespace NETworkManager.ViewModels
{
    public class NetworkInterfaceAddIPAddressViewModel : ViewModelBase
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

        private string _subnetmaskOrCidr;
        public string SubnetmaskOrCidr
        {
            get => _subnetmaskOrCidr;
            set
            {
                if (value == _subnetmaskOrCidr)
                    return;

                _subnetmaskOrCidr = value;
                OnPropertyChanged();
            }
        }

        public NetworkInterfaceAddIPAddressViewModel(Action<NetworkInterfaceAddIPAddressViewModel> addCommand, Action<NetworkInterfaceAddIPAddressViewModel> cancelHandler)
        {
            AddCommand = new RelayCommand(p => addCommand(this));
            CancelCommand = new RelayCommand(p => cancelHandler(this));
        }        
    }
}