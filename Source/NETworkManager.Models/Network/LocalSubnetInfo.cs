namespace NETworkManager.Models.Network;

/// <summary>
///     IPv4 subnet of a local network interface, e.g. to scan the subnet of a specific network interface.
/// </summary>
/// <param name="InterfaceName">Name of the network interface (e.g. "Ethernet").</param>
/// <param name="Subnet">Subnet in CIDR notation (network address, e.g. "192.168.1.0/24").</param>
/// <param name="IsLinkLocal">The subnet is the link-local subnet (169.254.0.0/16, no DHCP server answered).</param>
public sealed record LocalSubnetInfo(string InterfaceName, string Subnet, bool IsLinkLocal)
{
    /// <summary>
    ///     Text to show the subnet in a list, e.g. "192.168.1.0/24 (Ethernet)".
    /// </summary>
    public string DisplayName => $"{Subnet} ({InterfaceName})";
}
