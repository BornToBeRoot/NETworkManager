using System;
using System.Windows.Input;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

/// <summary>
/// View model for adding an ARP table entry.
/// </summary>
public class ARPTableAddEntryViewModel : ViewModelBase
{
    /// <summary>
    /// Backing field for <see cref="IPAddress"/>.
    /// </summary>
    private string _ipAddress;

    /// <summary>
    /// Backing field for <see cref="MACAddress"/>.
    /// </summary>
    private string _macAddress;

    /// <summary>
    /// Initializes a new instance of the <see cref="ARPTableAddEntryViewModel"/> class.
    /// </summary>
    /// <param name="addCommand">The action to execute when the add command is invoked.</param>
    /// <param name="cancelHandler">The action to execute when the cancel command is invoked.</param>
    public ARPTableAddEntryViewModel(Action<ARPTableAddEntryViewModel> addCommand,
        Action<ARPTableAddEntryViewModel> cancelHandler)
    {
        AddCommand = new RelayCommand(_ => addCommand(this));
        CancelCommand = new RelayCommand(_ => cancelHandler(this));
    }

    /// <summary>
    /// Gets the command to add the entry.
    /// </summary>
    public ICommand AddCommand { get; }

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
    /// Gets or sets the MAC address.
    /// </summary>
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
}