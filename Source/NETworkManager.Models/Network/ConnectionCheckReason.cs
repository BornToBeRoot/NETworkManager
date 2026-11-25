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
    IPv6NotAvailable,

    /// <summary>
    ///     There is no IP address to resolve the DNS name (PTR) for.
    /// </summary>
    NoIPAddressToResolve,

    /// <summary>
    ///     The DNS server answered, but there is no DNS record (PTR) for the IP address (common for private addresses).
    /// </summary>
    PTRNotFound,

    /// <summary>
    ///     The DNS server did not answer or returned an error.
    /// </summary>
    DNSError,

    /// <summary>
    ///     No default gateway was found on the network adapter of the local IP address (e.g. a VPN connection).
    /// </summary>
    NoGateway,

    /// <summary>
    ///     The gateway did not reply to the ping (it may be offline or block ICMP).
    /// </summary>
    GatewayNoReply,

    /// <summary>
    ///     The API returned a private IP address (e.g. the page of a captive portal or a proxy).
    /// </summary>
    PublicIPAddressPrivate,

    /// <summary>
    ///     The public IP address could not be requested from the API (no answer, an error or no IP address).
    /// </summary>
    PublicIPAddressRequestFailed,

    /// <summary>
    ///     The host name of the public IP address API could not be resolved (DNS).
    /// </summary>
    NameResolutionFailed
}
