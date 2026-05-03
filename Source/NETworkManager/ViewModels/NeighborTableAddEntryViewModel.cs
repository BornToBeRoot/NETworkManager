using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

/// <summary>
/// View model for adding a neighbor table entry.
/// </summary>
public class NeighborTableAddEntryViewModel : ViewModelBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NeighborTableAddEntryViewModel"/> class.
    /// </summary>
    /// <param name="addCommand">The action to execute when the add command is invoked.</param>
    /// <param name="cancelHandler">The action to execute when the cancel command is invoked.</param>
    /// <param name="interfaces">Available network interfaces to select from.</param>
    public NeighborTableAddEntryViewModel(Action<NeighborTableAddEntryViewModel> addCommand,
        Action<NeighborTableAddEntryViewModel> cancelHandler,
        List<KeyValuePair<int, string>> interfaces)
    {
        AddCommand = new RelayCommand(_ => addCommand(this));
        CancelCommand = new RelayCommand(_ => cancelHandler(this));

        Interfaces = interfaces;

        if (interfaces.Count > 0)
        {
            var lastUsed = interfaces.FirstOrDefault(x =>
                x.Value == SettingsManager.Current.NeighborTable_InterfaceName);

            SelectedInterface = lastUsed.Value != null ? lastUsed : interfaces[0];
        }
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
    /// Gets or sets the MAC address.
    /// </summary>
    public string MACAddress
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
    /// Gets the list of available network interfaces.
    /// </summary>
    public List<KeyValuePair<int, string>> Interfaces { get; }

    /// <summary>
    /// Gets or sets the selected network interface.
    /// </summary>
    public KeyValuePair<int, string> SelectedInterface
    {
        get;
        set
        {
            if (value.Equals(field))
                return;

            field = value;

            SettingsManager.Current.NeighborTable_InterfaceName = value.Value;

            OnPropertyChanged();
        }
    }
}
