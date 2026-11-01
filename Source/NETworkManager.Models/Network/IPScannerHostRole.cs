using System;

namespace NETworkManager.Models.Network;

/// <summary>
///     Roles of a host for the local computer (e.g. its default gateway), see <see cref="IPScannerHostRoleHelper" />.
/// </summary>
[Flags]
public enum IPScannerHostRole
{
    /// <summary>
    ///     No role.
    /// </summary>
    None = 0,

    /// <summary>
    ///     IP address of the local computer.
    /// </summary>
    LocalComputer = 1,

    /// <summary>
    ///     Default gateway of a local network interface.
    /// </summary>
    DefaultGateway = 2,

    /// <summary>
    ///     DNS server of a local network interface.
    /// </summary>
    DNSServer = 4,

    /// <summary>
    ///     DHCP server of a local network interface.
    /// </summary>
    DHCPServer = 8
}
