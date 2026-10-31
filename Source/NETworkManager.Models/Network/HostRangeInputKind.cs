namespace NETworkManager.Models.Network;

/// <summary>
///     Format of a host input, see <see cref="HostRangeHelper.Parse" />.
/// </summary>
public enum HostRangeInputKind
{
    /// <summary>
    ///     Not a valid input (rejected by the validation of the input).
    /// </summary>
    Invalid,

    /// <summary>
    ///     IPv4 or IPv6 address, e.g. "10.0.0.1" or "2001:db8::1".
    /// </summary>
    IPAddress,

    /// <summary>
    ///     IPv4 range, e.g. "10.0.0.0/24", "10.0.0.0/255.255.255.0", "10.0.0.1-100" or "10.0.0.1-10.0.0.100".
    /// </summary>
    IPv4Range,

    /// <summary>
    ///     IPv4 range per octet, e.g. "10.0.[0-10,20].1".
    /// </summary>
    IPv4SpecialRange,

    /// <summary>
    ///     Hostname, e.g. "server.example.com".
    /// </summary>
    Hostname,

    /// <summary>
    ///     Subnet of a hostname (IPv4), e.g. "server.example.com/24" or "server.example.com/255.255.255.0".
    /// </summary>
    HostnameWithSubnet
}
