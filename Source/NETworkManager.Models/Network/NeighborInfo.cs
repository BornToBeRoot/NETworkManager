using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace NETworkManager.Models.Network;

/// <summary>
///     Class to store information about an IP neighbor entry (IPv4 ARP / IPv6 NDP).
/// </summary>
public class NeighborInfo
{
    /// <summary>
    ///     Creates a new instance of <see cref="NeighborInfo" /> with the given parameters.
    /// </summary>
    public NeighborInfo(IPAddress ipAddress, PhysicalAddress macAddress, bool isMulticast, int interfaceIndex,
        string interfaceAlias, NeighborState state, AddressFamily addressFamily)
    {
        IPAddress = ipAddress;
        MACAddress = macAddress;
        IsMulticast = isMulticast;
        InterfaceIndex = interfaceIndex;
        InterfaceAlias = interfaceAlias;
        State = state;
        AddressFamily = addressFamily;
    }

    /// <summary>
    ///     IP address of the neighbor entry (IPv4 or IPv6).
    /// </summary>
    public IPAddress IPAddress { get; set; }

    /// <summary>
    ///     Physical (link-layer) address of the neighbor entry. May be empty for
    ///     <see cref="NeighborState.Incomplete" /> / <see cref="NeighborState.Unreachable" /> entries.
    /// </summary>
    public PhysicalAddress MACAddress { get; set; }

    /// <summary>
    ///     Indicates whether the IP address is a multicast address.
    /// </summary>
    public bool IsMulticast { get; set; }

    /// <summary>
    ///     Index of the network interface this neighbor entry belongs to.
    /// </summary>
    public int InterfaceIndex { get; set; }

    /// <summary>
    ///     Human-readable name of the network interface (e.g. <c>"Ethernet"</c>, <c>"Wi-Fi"</c>).
    /// </summary>
    public string InterfaceAlias { get; set; }

    /// <summary>
    ///     Reachability state of the neighbor entry.
    /// </summary>
    public NeighborState State { get; set; }

    /// <summary>
    ///     Address family (<see cref="AddressFamily.InterNetwork" /> for IPv4,
    ///     <see cref="AddressFamily.InterNetworkV6" /> for IPv6).
    /// </summary>
    public AddressFamily AddressFamily { get; set; }
}
