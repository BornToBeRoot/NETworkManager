using System;

namespace NETworkManager.ViewModels;

public abstract class IPAddressViewModel : DialogViewModelBase<IPAddressViewModel>
{
    private string _ipAddress;

    protected IPAddressViewModel(Action<IPAddressViewModel> okCommand, Action<IPAddressViewModel> cancelHandler)
        : base(okCommand, cancelHandler)
    {
    }

    public string IPAddress
    {
        get => _ipAddress;
        set => SetProperty(ref _ipAddress, value);
    }
}