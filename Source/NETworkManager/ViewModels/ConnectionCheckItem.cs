using NETworkManager.Models.Network;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

/// <summary>
/// Represents a single "is checking / value / state" row (e.g. Computer IPv4, Router DNS, ...)
/// shown by <see cref="NetworkConnectionWidgetViewModel"/>.
/// </summary>
public class ConnectionCheckItem : PropertyChangedBase
{
    /// <summary>
    /// Gets or sets a value indicating whether this item is currently being checked.
    /// </summary>
    public bool IsChecking
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
    /// Gets or sets the checked value (e.g. an IP address or hostname).
    /// </summary>
    public string Value
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = "";

    /// <summary>
    /// Gets or sets the connection state of this item.
    /// </summary>
    public ConnectionState State
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = ConnectionState.None;

    /// <summary>
    /// Resets the item to its initial "checking" state.
    /// </summary>
    public void Reset()
    {
        IsChecking = true;
        Value = "";
        State = ConnectionState.None;
    }

    /// <summary>
    /// Completes the check with the given value and state.
    /// </summary>
    public void Complete(string value, ConnectionState state)
    {
        Value = value;
        State = state;
        IsChecking = false;
    }
}
