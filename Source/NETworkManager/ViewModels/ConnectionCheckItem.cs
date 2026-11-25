using System.Linq;
using System.Net;
using System.Windows.Input;
using NETworkManager.Localization;
using NETworkManager.Models;
using NETworkManager.Models.Network;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

/// <summary>
/// Kind of a <see cref="ConnectionCheckItem"/>: decides which other tools can be opened with its value.
/// </summary>
public enum ConnectionCheckItemKind
{
    /// <summary>
    /// IP address of the computer.
    /// </summary>
    ComputerIPAddress,

    /// <summary>
    /// IP address of the gateway / router.
    /// </summary>
    RouterIPAddress,

    /// <summary>
    /// Public IP address (internet).
    /// </summary>
    PublicIPAddress,

    /// <summary>
    /// DNS name (PTR).
    /// </summary>
    DNSName
}

/// <summary>
/// Represents a single "is checking / value / state" row (e.g. Computer IPv4, Router DNS, ...)
/// shown by <see cref="NetworkConnectionWidgetViewModel"/>.
/// </summary>
public class ConnectionCheckItem : PropertyChangedBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionCheckItem"/> class.
    /// </summary>
    /// <param name="kind">Kind of the item (which tools can be opened with its value).</param>
    public ConnectionCheckItem(ConnectionCheckItemKind kind)
    {
        Kind = kind;
    }

    /// <summary>
    /// Gets the kind of the item.
    /// </summary>
    public ConnectionCheckItemKind Kind { get; }

    /// <summary>
    /// Gets a value indicating whether the value is an IP address (not a DNS name).
    /// </summary>
    public bool IsIPAddress => Kind != ConnectionCheckItemKind.DNSName;

    /// <summary>
    /// Gets a value indicating whether the value is the IP address of the gateway / router (e.g. web interface).
    /// </summary>
    public bool IsRouterIPAddress => Kind == ConnectionCheckItemKind.RouterIPAddress;

    /// <summary>
    /// Gets a value indicating whether the target can be opened in the Web Console: the gateway / router, but not a
    /// link-local IPv6 address (the scope id, e.g. <c>fe80::1%12</c>, is not valid in a URL).
    /// </summary>
    public bool IsWebConsoleAvailable => IsRouterIPAddress &&
                                         !(IPAddress.TryParse(Target, out var ipAddress) && ipAddress.IsIPv6LinkLocal);

    /// <summary>
    /// Gets a value indicating whether the value is the public IP address (e.g. IP geolocation, RDAP).
    /// </summary>
    public bool IsPublicIPAddress => Kind == ConnectionCheckItemKind.PublicIPAddress;

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
    /// Gets or sets the IP address or DNS name that is opened in other tools (e.g. without the round-trip time of the
    /// value), <c>null</c> if there is none.
    /// </summary>
    public string Target
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsWebConsoleAvailable));
        }
    }

    /// <summary>
    /// Resets the item to its initial "checking" state.
    /// </summary>
    public void Reset()
    {
        IsChecking = true;
        Value = "";
        Target = null;
        State = ConnectionState.None;
        Message = null;
    }

    /// <summary>
    /// Completes the check with the given value and state.
    /// </summary>
    /// <param name="value">Value shown (<c>-/-</c> if there is none).</param>
    /// <param name="state">State of the check.</param>
    /// <param name="message">Reason of the state (tooltip), <c>null</c> if none.</param>
    /// <param name="target">IP address or DNS name for other tools, by default the value (if there is one), an empty
    /// string for none.</param>
    public void Complete(string value, ConnectionState state, string message = null, string target = null)
    {
        Value = value;
        Target = target != null
            ? target.Length == 0 ? null : target
            : string.IsNullOrEmpty(value) || value == "-/-" ? null : value;
        State = state;
        Message = message;
        IsChecking = false;
    }

    /// <summary>
    /// Completes the check with the given value and the state and (localized) reason of the result.
    /// </summary>
    /// <param name="value">Value shown (<c>-/-</c> if there is none).</param>
    /// <param name="result">State and reason.</param>
    /// <param name="target">IP address or DNS name for other tools, by default the value (if there is one), an empty
    /// string for none.</param>
    public void Complete(string value, ConnectionCheckResult result, string target = null)
    {
        Complete(value, result.State, result.Reason == ConnectionCheckReason.None
            ? null
            : ResourceTranslator.Translate(ResourceIdentifier.ConnectionCheckReason, result.Reason), target);
    }

    /// <summary>
    /// Gets the command to copy the value.
    /// </summary>
    public ICommand CopyValueCommand => new RelayCommand(_ => ClipboardHelper.SetClipboard(Value),
        _ => !string.IsNullOrEmpty(Value) && Value != "-/-");

    /// <summary>
    /// Gets the command to open the target in another tool (parameter: <see cref="ApplicationName"/>).
    /// </summary>
    public ICommand OpenApplicationCommand => new RelayCommand(OpenApplicationAction, _ => Target != null);

    private void OpenApplicationAction(object parameter)
    {
        if (parameter is not ApplicationName applicationName || Target == null)
            return;

        NavigationManager.Open(applicationName switch
        {
            ApplicationName.DNSLookup => new DNSLookupRequest { Target = Target },
            ApplicationName.RDAP => new RDAPRequest { Target = Target },
            ApplicationName.WebConsole => new ApplicationRequest(applicationName)
                { Target = WebConsoleUrlHelper.CreateUrl(Target, Enumerable.Empty<int>()) },
            _ => new ApplicationRequest(applicationName) { Target = Target }
        });
    }
}
