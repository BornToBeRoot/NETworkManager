using NETworkManager.Utilities;
using System;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

public abstract class IPAddressViewModel : ViewModelBase
{
    public ICommand OKCommand { get; }

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

    protected IPAddressViewModel(Action<IPAddressViewModel> okCommand, Action<IPAddressViewModel> cancelHandler)
    {
        OKCommand = new RelayCommand(_ => okCommand(this));
        CancelCommand = new RelayCommand(_ => cancelHandler(this));
    }        
}