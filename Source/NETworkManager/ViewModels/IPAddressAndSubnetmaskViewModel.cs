using System;
using System.Windows.Input;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

/// <summary>
/// View model for an IP address and a subnet mask.
/// </summary>
public class IPAddressAndSubnetmaskViewModel : ViewModelBase
{
    /// <summary>
    /// Backing field for <see cref="IPAddress"/>.
    /// </summary>
    private string _ipAddress;

    /// <summary>
    /// Backing field for <see cref="Subnetmask"/>.
    /// </summary>
    private string _subnetmask;

    /// <summary>
    /// Initializes a new instance of the <see cref="IPAddressAndSubnetmaskViewModel"/> class.
    /// </summary>
    /// <param name="okCommand">The action to execute when OK is clicked.</param>
    /// <param name="cancelHandler">The action to execute when Cancel is clicked.</param>
    public IPAddressAndSubnetmaskViewModel(Action<IPAddressAndSubnetmaskViewModel> okCommand,
        Action<IPAddressAndSubnetmaskViewModel> cancelHandler)
    {
        OKCommand = new RelayCommand(_ => okCommand(this));
        CancelCommand = new RelayCommand(_ => cancelHandler(this));
    }

    /// <summary>
    /// Gets the command to confirm the operation.
    /// </summary>
    public ICommand OKCommand { get; }

    /// <summary>
    /// Gets the command to cancel the operation.
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// Gets or sets the IP address.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the subnet mask.
    /// </summary>
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