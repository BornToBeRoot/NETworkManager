using System.Collections.Generic;
using System.Net;

namespace NETworkManager.Models.Network;

/// <summary>
///     Host input in its parts, see <see cref="HostRangeHelper.Parse" />. Only the values of the
///     <paramref name="Kind" /> are set.
/// </summary>
/// <param name="Kind">Format of the input.</param>
/// <param name="IPAddress">IP address (<see cref="HostRangeInputKind.IPAddress" />).</param>
/// <param name="Start">First IPv4 address (<see cref="HostRangeInputKind.IPv4Range" />).</param>
/// <param name="End">Last IPv4 address (<see cref="HostRangeInputKind.IPv4Range" />).</param>
/// <param name="Octets">Values of the four octets (<see cref="HostRangeInputKind.IPv4SpecialRange" />).</param>
/// <param name="Hostname">
///     Hostname (<see cref="HostRangeInputKind.Hostname" />, <see cref="HostRangeInputKind.HostnameWithSubnet" />).
/// </param>
/// <param name="Subnet">
///     CIDR or subnet mask after the hostname, e.g. "24" (<see cref="HostRangeInputKind.HostnameWithSubnet" />).
/// </param>
public sealed record HostRangeInput(
    HostRangeInputKind Kind,
    IPAddress IPAddress = null,
    IPAddress Start = null,
    IPAddress End = null,
    List<List<byte>> Octets = null,
    string Hostname = null,
    string Subnet = null);
