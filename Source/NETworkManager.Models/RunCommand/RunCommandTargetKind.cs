namespace NETworkManager.Models.RunCommand;

/// <summary>
///     Kind of a target that is typed without a command (e.g. "10.0.0.1"), see <see cref="RunCommandTarget" />.
/// </summary>
public enum RunCommandTargetKind
{
    /// <summary>
    ///     Not a target.
    /// </summary>
    None,

    /// <summary>
    ///     IP address or host name with a domain (e.g. "10.0.0.1", "server-01.borntoberoot.net").
    /// </summary>
    Host,

    /// <summary>
    ///     Host name without a domain (e.g. "server-01" in a company network). It can also be a mistyped command, so the
    ///     commands are only suggested if no command starts with it, and not preselected.
    /// </summary>
    Hostname,

    /// <summary>
    ///     Several hosts separated by ";", at least one is an IP address or a host with a domain.
    /// </summary>
    Hosts,

    /// <summary>
    ///     IPv4 range (e.g. "10.0.0.1-10.0.0.100", "10.0.[0-1].1"), or several hosts with at least one range or subnet.
    /// </summary>
    IPRange,

    /// <summary>
    ///     Subnet (e.g. "10.0.0.0/24").
    /// </summary>
    Subnet,

    /// <summary>
    ///     MAC address (e.g. "00:11:22:33:44:55").
    /// </summary>
    MACAddress,

    /// <summary>
    ///     First 3 bytes of a MAC address (e.g. "00:11:22").
    /// </summary>
    OUI,

    /// <summary>
    ///     Port(s) or port range(s), optional with protocol (e.g. "22", "80-90/tcp").
    /// </summary>
    Ports,

    /// <summary>
    ///     AS number (e.g. "AS13335").
    /// </summary>
    ASN,

    /// <summary>
    ///     HTTP or HTTPS URL.
    /// </summary>
    URL
}
