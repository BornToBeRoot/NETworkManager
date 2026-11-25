using NETworkManager.Localization;
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
    /// Gets or sets the reason of the state (e.g. why it is a warning), shown as tooltip of the state icon. <c>null</c>
    /// if there is nothing to explain.
    /// </summary>
    public string Message
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
    /// Resets the item to its initial "checking" state.
    /// </summary>
    public void Reset()
    {
        IsChecking = true;
        Value = "";
        State = ConnectionState.None;
        Message = null;
    }

    /// <summary>
    /// Completes the check with the given value and state.
    /// </summary>
    public void Complete(string value, ConnectionState state, string message = null)
    {
        Value = value;
        State = state;
        Message = message;
        IsChecking = false;
    }

    /// <summary>
    /// Completes the check with the given value and the state and (localized) reason of the result.
    /// </summary>
    public void Complete(string value, ConnectionCheckResult result)
    {
        Complete(value, result.State, result.Reason == ConnectionCheckReason.None
            ? null
            : ResourceTranslator.Translate(ResourceIdentifier.ConnectionCheckReason, result.Reason));
    }
}
