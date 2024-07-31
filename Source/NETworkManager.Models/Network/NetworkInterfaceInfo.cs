using System;
using System.Net;
using System.Net.NetworkInformation;

namespace NETworkManager.Models.Network;

/// <summary>
///     Represents a network interface with all its properties.
/// </summary>
public class NetworkInterfaceInfo
{
    /// <summary>
    ///     Identifier of the network adapter.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    ///     Name of the network adapter.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Description of the network adapter.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    ///     Type of the network adapter (e.g. Ethernet, Wireless).
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    ///     Physical address (MAC) of the network adapter.
    /// </summary>
    public PhysicalAddress PhysicalAddress { get; set; }

    /// <summary>
    ///     Status of the network adapter.
    /// </summary>
    public OperationalStatus Status { get; set; }

    /// <summary>
    ///     Is the network adapter operational.
    /// </summary>
    public bool IsOperational { get; set; }

    /// <summary>
    ///     Speed of the network adapter.
    /// </summary>
    public long Speed { get; set; }

    /// <summary>
    ///     Is the IPv4 protocol available.
    /// </summary>
    public bool IPv4ProtocolAvailable { get; set; }

    /// <summary>
    ///     IPv4 address(es) with subnet mask.
    /// </summary>
    public Tuple<IPAddress, IPAddress>[] IPv4Address { get; set; }

    /// <summary>
    ///     IPv4 gateway(s)
    /// </summary>
    public IPAddress[] IPv4Gateway { get; set; }

    /// <summary>
    ///     Is DHCP enabled.
    /// </summary>
    public bool DhcpEnabled { get; set; }

    /// <summary>
    ///     DHCP server(s).
    /// </summary>
    public IPAddress[] DhcpServer { get; set; }

    /// <summary>
    ///     Time when the DHCP lease was obtained.
    /// </summary>
    public DateTime DhcpLeaseObtained { get; set; }

    /// <summary>
    ///     Time when the DHCP lease expires.
    /// </summary>
    public DateTime DhcpLeaseExpires { get; set; }

    /// <summary>
    ///     Is the IPv6 protocol available.
    /// </summary>
    public bool IPv6ProtocolAvailable { get; set; }

    /// <summary>
    ///     IPv6 address(es).
    /// </summary>
    public IPAddress[] IPv6Address { get; set; }

    /// <summary>
    ///     IPv6 link-local address(es).
    /// </summary>
    public IPAddress[] IPv6AddressLinkLocal { get; set; }

    /// <summary>
    ///     IPv6 gateway(s).
    /// </summary>
    public IPAddress[] IPv6Gateway { get; set; }

    /// <summary>
    ///     Is DNS autoconfiguration enabled.
    /// </summary>
    public bool DNSAutoconfigurationEnabled { get; set; }

    /// <summary>
    ///     DNS suffix like "example.com".
    /// </summary>
    public string DNSSuffix { get; set; }

    /// <summary>
    ///     DNS server(s).
    /// </summary>
    public IPAddress[] DNSServer { get; set; }
}