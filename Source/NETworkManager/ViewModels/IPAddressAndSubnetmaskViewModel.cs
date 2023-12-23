using System;
using System.Windows.Input;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

public class IPAddressAndSubnetmaskViewModel : ViewModelBase
{
    private string _ipAddress;

    private string _subnetmask;

    public IPAddressAndSubnetmaskViewModel(Action<IPAddressAndSubnetmaskViewModel> okCommand,
        Action<IPAddressAndSubnetmaskViewModel> cancelHandler)
    {
        OKCommand = new RelayCommand(_ => okCommand(this));
        CancelCommand = new RelayCommand(_ => cancelHandler(this));
    }

    public ICommand OKCommand { get; }

    public ICommand CancelCommand { get; }

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

    public string Subnetmask
    {
        get => _subnetmask;
        set
        {
            if (value == _subnetmask)
                return;

            _subnetmask = value;
            OnPropertyChanged();
        }
    }
}