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
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the subnet mask.
    /// </summary>
    public string Subnetmask
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }
}