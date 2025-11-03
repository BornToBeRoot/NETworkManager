using System;

namespace NETworkManager.ViewModels;

public class IPAddressAndSubnetmaskViewModel : DialogViewModelBase<IPAddressAndSubnetmaskViewModel>
{
    private string _ipAddress;

    private string _subnetmask;

    public IPAddressAndSubnetmaskViewModel(Action<IPAddressAndSubnetmaskViewModel> okCommand,
        Action<IPAddressAndSubnetmaskViewModel> cancelHandler)
        : base(okCommand, cancelHandler)
    {
    }

    public string IPAddress
    {
        get => _ipAddress;
        set => SetProperty(ref _ipAddress, value);
    }

    public string Subnetmask
    {
        get => _subnetmask;
        set => SetProperty(ref _subnetmask, value);
    }
}