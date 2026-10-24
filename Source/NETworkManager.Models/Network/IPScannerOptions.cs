namespace NETworkManager.Models.Network;

/// <summary>
///     Class contains the options for the IP scanner.
/// </summary>
public class IPScannerOptions
{
    /// <summary>
    ///     Max concurrent threads for the host scan.
    /// </summary>
    public int MaxHostThreads { get; init; }

    /// <summary>
    ///     Max concurrent threads for the port scan of each host.
    /// </summary>
    public int MaxPortThreads { get; init; }

    /// <summary>
    ///     Number of attempts to ping a host.
    /// </summary>
    public int ICMPAttempts { get; init; }

    /// <summary>
    ///     Timeout in milliseconds after which a ping is considered lost.
    /// </summary>
    public int ICMPTimeout { get; init; }

    /// <summary>
    ///     Size of the buffer used in the ping in bytes.
    /// </summary>
    public byte[] ICMPBuffer { get; init; } = [];

    /// <summary>
    ///     Resolve the hostname for an IP address.
    /// </summary>
    public bool ResolveHostname { get; init; }

    /// <summary>
    ///     Scan the ports of the host.
    /// </summary>
    public bool PortScanEnabled { get; init; }

    /// <summary>
    ///     Ports to scan.
    /// </summary>
    public int[] PortScanPorts { get; init; } = [];

    /// <summary>
    ///     Timeout in milliseconds after which a port is considered closed.
    /// </summary>
    public int PortScanTimeout { get; init; }

    /// <summary>
    ///     Number of attempts to connect to a port if there was no answer (timed out).
    /// </summary>
    public int PortScanAttempts { get; init; }

    /// <summary>
    ///     Resolve the NetBIOS name for an IP address.
    /// </summary>
    public bool NetBIOSEnabled { get; init; }

    /// <summary>
    ///     Timeout in milliseconds after which a NetBIOS request is considered lost.
    /// </summary>
    public int NetBIOSTimeout { get; init; }

    /// <summary>
    ///     Resolve the MAC address for the host from ARP.
    /// </summary>
    public bool ResolveMACAddress { get; init; }

    /// <summary>
    ///     Include unreachable IP addresses in the result.
    /// </summary>
    public bool ShowAllResults { get; init; }
}
