namespace NETworkManager.Models.Network;

/// <summary>
///     Reason of the state of a network connection check (e.g. Computer > IPv4), shown as tooltip of the state icon.
///     Localized via <c>ResourceIdentifier.ConnectionCheckReason</c>.
/// </summary>
public enum ConnectionCheckReason
{
    /// <summary>
    ///     No reason (e.g. the check was successful).
    /// </summary>
    None,

    /// <summary>
    ///     The computer has no IPv4 address.
    /// </summary>
    NoIPv4Address,

    /// <summary>
    ///     The computer has only a link-local IPv4 address (169.254.x.x): no address from a DHCP server.
    /// </summary>
    IPv4LinkLocal,

    /// <summary>
    ///     There is no route to the public IP address (no default gateway); the address of the first active network
    ///     adapter is shown.
    /// </summary>
    NoRoute,

    /// <summary>
    ///     The computer has no routed IPv6 address (only link-local or none), e.g. the network doesn't support IPv6.
    /// </summary>
    IPv6NotAvailable
}
