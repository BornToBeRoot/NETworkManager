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

        var subnetmask = enableStaticIPAddress ? ConvertSubnetmaskOrCidr(subnetmaskOrCidr) : null;

        // DNS servers can only be obtained automatically with DHCP (older profiles may contain a static IP address
        // with DHCP for the DNS servers, the UI selects static DNS servers in that case)
        if (enableStaticIPAddress)
            enableStaticDNS = true;

        // If primary and secondary DNS are empty --> autoconfiguration
        if (enableStaticDNS && string.IsNullOrEmpty(primaryDNSServer) && string.IsNullOrEmpty(secondaryDNSServer))
            enableStaticDNS = false;

        // When primary DNS is empty, swap it with secondary (if not empty)
        if (enableStaticDNS && string.IsNullOrEmpty(primaryDNSServer) && !string.IsNullOrEmpty(secondaryDNSServer))
        {
            primaryDNSServer = secondaryDNSServer;
            secondaryDNSServer = string.Empty;
        }

        // Values of the option that is not used (e.g. a static IP address stored in a profile that uses DHCP) are not
        // applied and not validated.
        return new NetworkInterfaceConfig
        {
            Name = name,
            EnableStaticIPAddress = enableStaticIPAddress,
            IPAddress = enableStaticIPAddress ? ipAddress : null,
            Subnetmask = enableStaticIPAddress ? subnetmask : null,
            Gateway = enableStaticIPAddress ? gateway : null,
            EnableStaticDNS = enableStaticDNS,
            PrimaryDNSServer = enableStaticDNS ? primaryDNSServer : null,
            SecondaryDNSServer = enableStaticDNS ? secondaryDNSServer : null
        };
    }

    /// <summary>
    ///     Gets the first value that is not a valid IPv4 address (IP address, gateway, DNS servers) or subnet mask.
    ///     The values are inserted into a command line that runs elevated, so they must be checked again before they
    ///     are applied: profile files can be edited or shared without the validation of the UI.
    /// </summary>
    /// <returns>The invalid value, or <see langword="null" /> if all values are empty or valid.</returns>
    public string GetInvalidValue()
    {
        foreach (var value in new[] { IPAddress, Gateway, PrimaryDNSServer, SecondaryDNSServer })
        {
            if (!string.IsNullOrEmpty(value) && !RegexHelper.IPv4AddressRegex().IsMatch(value))
                return value;
        }

        if (!string.IsNullOrEmpty(Subnetmask) && !RegexHelper.SubnetmaskRegex().IsMatch(Subnetmask))
            return Subnetmask;

        return null;
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

        // Not Subnetmask.ConvertSubnetmaskOrCidrToCidr: it throws for invalid input, which is reported by
        // GetInvalidValue instead
        return int.TryParse(value.TrimStart('/'), out var cidr) && cidr is >= 0 and <= 32
            ? Network.Subnetmask.ConvertCidrToSubnetmask(cidr)
            : value;
    }
}
