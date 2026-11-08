using NETworkManager.Utilities;

namespace NETworkManager.Models.Network;

public class NetworkInterfaceConfig
{
    /// <summary>
    ///     Name of the network adapter to configure (e.g. "Ethernet", "Wi-Fi", "Local Area Connection")
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    ///     Enable or disable static IP address configuration (otherwise DHCP is used).
    /// </summary>
    public bool EnableStaticIPAddress { get; init; }

    /// <summary>
    ///     Enable or disable DHCP static IP coexistence.
    /// </summary>
    public bool EnableDhcpStaticIpCoexistence { get; init; }

    /// <summary>
    ///     Static or additional IP address to configure.
    /// </summary>
    public string IPAddress { get; init; }

    /// <summary>
    ///     Subnet mask to use for the static or additional IP address configuration.
    /// </summary>
    public string Subnetmask { get; init; }

    /// <summary>
    ///     Gateway to use for the static IP address configuration.
    /// </summary>
    public string Gateway { get; init; }

    /// <summary>
    ///     Enable or disable static DNS server configuration (otherwise DHCP is used).
    /// </summary>
    public bool EnableStaticDNS { get; init; }

    /// <summary>
    ///     Primary DNS server to use for the static DNS server configuration.
    /// </summary>
    public string PrimaryDNSServer { get; init; }

    /// <summary>
    ///     Secondary DNS server to use for the static DNS server configuration.
    /// </summary>
    public string SecondaryDNSServer { get; init; }

    /// <summary>
    ///     Creates the configuration for a network interface from the values entered by the user (Configure tab or
    ///     profile). Values are trimmed, a subnet mask in CIDR notation (e.g. "/24" or "24") is converted to a subnet mask, static DNS servers
    ///     without any address fall back to DHCP and a secondary DNS server without a primary one becomes the primary.
    /// </summary>
    /// <param name="name">Name of the network interface.</param>
    /// <param name="enableStaticIPAddress">Use a static IP address (otherwise DHCP).</param>
    /// <param name="ipAddress">Static IPv4 address.</param>
    /// <param name="subnetmaskOrCidr">Subnet mask (e.g. "255.255.255.0") or CIDR (e.g. "/24").</param>
    /// <param name="gateway">Default gateway (optional).</param>
    /// <param name="enableStaticDNS">Use static DNS servers (otherwise DHCP).</param>
    /// <param name="primaryDNSServer">Primary DNS server.</param>
    /// <param name="secondaryDNSServer">Secondary DNS server.</param>
    /// <returns>Configuration to apply.</returns>
    public static NetworkInterfaceConfig Create(string name, bool enableStaticIPAddress, string ipAddress,
        string subnetmaskOrCidr, string gateway, bool enableStaticDNS, string primaryDNSServer,
        string secondaryDNSServer)
    {
        // Values from the UI are validated with trimmed input (and profile values are trimmed when saved).
        ipAddress = ipAddress?.Trim();
        gateway = gateway?.Trim();
        primaryDNSServer = primaryDNSServer?.Trim();
        secondaryDNSServer = secondaryDNSServer?.Trim();

        var subnetmask = enableStaticIPAddress ? ConvertSubnetmaskOrCidr(subnetmaskOrCidr) : subnetmaskOrCidr?.Trim();

        // If primary and secondary DNS are empty --> autoconfiguration
        if (enableStaticDNS && string.IsNullOrEmpty(primaryDNSServer) && string.IsNullOrEmpty(secondaryDNSServer))
            enableStaticDNS = false;

        // When primary DNS is empty, swap it with secondary (if not empty)
        if (enableStaticDNS && string.IsNullOrEmpty(primaryDNSServer) && !string.IsNullOrEmpty(secondaryDNSServer))
        {
            primaryDNSServer = secondaryDNSServer;
            secondaryDNSServer = string.Empty;
        }

        return new NetworkInterfaceConfig
        {
            Name = name,
            EnableStaticIPAddress = enableStaticIPAddress,
            IPAddress = ipAddress,
            Subnetmask = subnetmask,
            Gateway = gateway,
            EnableStaticDNS = enableStaticDNS,
            PrimaryDNSServer = primaryDNSServer,
            SecondaryDNSServer = secondaryDNSServer
        };
    }

    /// <summary>
    ///     Converts a subnet mask or CIDR as accepted by the IPv4SubnetmaskOrCIDRValidator (e.g. "255.255.255.0",
    ///     "/24" or "24") to a subnet mask, which is required by <c>netsh</c>.
    /// </summary>
    /// <param name="subnetmaskOrCidr">Subnet mask or CIDR.</param>
    /// <returns>Subnet mask, or the trimmed input if it is neither a subnet mask nor a CIDR.</returns>
    public static string ConvertSubnetmaskOrCidr(string subnetmaskOrCidr)
    {
        var value = subnetmaskOrCidr?.Trim();

        if (string.IsNullOrEmpty(value) || RegexHelper.SubnetmaskRegex().IsMatch(value))
            return value;

        return int.TryParse(value.TrimStart('/'), out var cidr) && cidr is >= 0 and <= 32
            ? Network.Subnetmask.GetFromCidr(cidr).Subnetmask
            : value;
    }
}
