// ReSharper disable InconsistentNaming
namespace NETworkManager.Models.Firewall;

/// <summary>
/// Specifies the network protocols supported by the firewall configuration.
/// Each protocol is represented by its respective protocol number as defined in
/// the Internet Assigned Numbers Authority (IANA) protocol registry.
/// This enumeration is used to identify traffic based on its protocol type
/// for filtering or access control purposes in the firewall.
/// </summary>
public enum FirewallProtocol
{
    /// <summary>
    /// Denotes the Transmission Control Protocol (TCP) used in firewall configurations.
    /// TCP is a fundamental protocol within the Internet Protocol Suite, ensuring reliable
    /// communication by delivering a stream of data packets in sequence with error checking
    /// between networked devices.
    /// </summary>
    TCP = 6,

    /// <summary>
    /// Represents the User Datagram Protocol (UDP) in the context of firewall rules.
    /// UDP is a connectionless protocol within the Internet Protocol (IP) suite that
    /// allows for minimal latency by transmitting datagrams without guaranteeing delivery,
    /// order, or error recovery.
    /// </summary>
    UDP = 17,

    /// <summary>
    /// Represents the Internet Control Message Protocol (ICMPv4) in the context of firewall rules.
    /// ICMP is used by network devices, such as routers, to send error messages and operational
    /// information, indicating issues like unreachable network destinations.
    /// </summary>
    ICMPv4 = 1,

    /// <summary>
    /// Represents the Internet Control Message Protocol for IPv6 (ICMPv6) in the context of firewall rules.
    /// ICMPv6 is a supporting protocol in the Internet Protocol version 6 (IPv6) suite and is used for
    /// diagnostic and error-reporting purposes, as well as for functions such as Neighbor Discovery Protocol (NDP).
    /// </summary>
    ICMPv6 = 58,

    /// <summary>
    /// Represents the IPv6 Hop-by-Hop Option (HOPOPT) protocol in the context of firewall rules.
    /// HOPOPT is a special protocol used in IPv6 for carrying optional information that must be examined
    /// by each node along the packet's delivery path.
    /// </summary>
    HOPOPT = 0,

    /// <summary>
    /// Represents the Generic Routing Encapsulation (GRE) protocol in the context of firewall rules.
    /// GRE is a tunneling protocol developed to encapsulate a wide variety of network layer protocols
    /// inside virtual point-to-point links. It is commonly used in creating VPNs and enabling the
    /// transport of multicast traffic and non-IP protocols across IP networks.
    /// </summary>
    GRE = 47,

    /// <summary>
    /// Represents the Internet Protocol Version 6 (IPv6) in the context of firewall rules.
    /// IPv6 is the most recent version of the Internet Protocol (IP) and provides identification
    /// and location addressing for devices across networks, enabling communication over the internet.
    /// </summary>
    IPv6 = 41,

    /// <summary>
    /// Represents the IPv6-Route protocol in the context of firewall rules.
    /// IPv6-Route is used for routing header information in IPv6 packets, which
    /// specifies the list of one or more intermediate nodes a packet should pass
    /// through before reaching its destination.
    /// </summary>
    IPv6_Route = 43,

    /// <summary>
    /// Represents the IPv6 Fragmentation Header (IPv6_Frag) in the context of firewall rules.
    /// The IPv6 Fragmentation Header is used to support fragmentation and reassembly of
    /// packets in IPv6 networks. It facilitates handling packets that are too large to
    /// fit in the path MTU (Maximum Transmission Unit) of the network segment.
    /// </summary>
    IPv6_Frag = 44,

    /// <summary>
    /// Represents the IPv6 No Next Header protocol in the context of firewall rules.
    /// This protocol indicates that there is no next header following the current header in the IPv6 packet.
    /// It is primarily used in cases where the payload does not require a specific transport protocol header.
    /// </summary>
    IPv6_NoNxt = 59,

    /// <summary>
    /// Represents the IPv6 Options (IPv6_Opts) protocol in the context of firewall rules.
    /// IPv6 Options is a part of the IPv6 suite used for carrying optional internet-layer information
    /// and additional headers for specific purposes, providing extensibility in IPv6 communication.
    /// </summary>
    IPv6_Opts = 60,

    /// <summary>
    /// Represents the Virtual Router Redundancy Protocol (VRRP) in the context of firewall rules.
    /// VRRP is a network protocol that provides automatic assignment of available routers to
    /// participating hosts, ensuring redundancy and high availability of router services.
    /// </summary>
    VRRP = 112,

    /// <summary>
    /// Represents the Pragmatic General Multicast (PGM) protocol in the context of firewall rules.
    /// PGM is a reliable multicast transport protocol that ensures ordered, duplicate-free,
    /// and scalable delivery of data in multicast-enabled networks.
    /// </summary>
    PGM = 113,

    /// <summary>
    /// Represents the Layer 2 Tunneling Protocol (L2TP) in the context of firewall rules.
    /// L2TP is a tunneling protocol used to support virtual private networks (VPNs) or
    /// as part of the delivery of services by Internet Service Providers (ISPs).
    /// </summary>
    L2TP = 115,

    /// <summary>
    /// Represents a wildcard protocol option to match any protocol in the context of firewall rules.
    /// The "Any" value can be used to specify that the rule applies to all network protocols
    /// without restriction or specificity.
    /// </summary>
    Any = 255
}