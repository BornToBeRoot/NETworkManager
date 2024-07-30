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
}