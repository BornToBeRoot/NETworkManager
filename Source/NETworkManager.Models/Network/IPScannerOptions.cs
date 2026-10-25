namespace NETworkManager.Models.Network;

/// <summary>
///     Class contains the options for the IP scanner.
/// </summary>
public class IPScannerOptions
{
    /// <summary>
    ///     Max concurrent threads for the host scan.
    /// </summary>
    public required int MaxHostThreads { get; init; }

    /// <summary>
    ///     Max concurrent threads for the port scan of each host.
    /// </summary>
    public required int MaxPortThreads { get; init; }

    /// <summary>
    ///     Number of attempts to ping a host.
    /// </summary>
    public required int ICMPAttempts { get; init; }

    /// <summary>
    ///     Timeout in milliseconds after which a ping is considered lost.
    /// </summary>
    public required int ICMPTimeout { get; init; }

    /// <summary>
    ///     Size of the buffer used in the ping in bytes.
    /// </summary>
    public required byte[] ICMPBuffer { get; init; }

    /// <summary>
    ///     Resolve the hostname for an IP address.
    /// </summary>
    public required bool ResolveHostname { get; init; }

    /// <summary>
    ///     Scan the ports of the host.
    /// </summary>
    public required bool PortScanEnabled { get; init; }

    /// <summary>
    ///     Ports to scan.
    /// </summary>
    public required int[] PortScanPorts { get; init; }

    /// <summary>
    ///     Timeout in milliseconds after which a port is considered closed.
    /// </summary>
    public required int PortScanTimeout { get; init; }

    /// <summary>
    ///     Number of attempts to connect to a port if there was no answer (timed out).
    /// </summary>
    public required int PortScanAttempts { get; init; }

    /// <summary>
    ///     Resolve the NetBIOS name for an IP address.
    /// </summary>
    public required bool NetBIOSEnabled { get; init; }

    /// <summary>
    ///     Timeout in milliseconds after which a NetBIOS request is considered lost.
    /// </summary>
    public required int NetBIOSTimeout { get; init; }

    /// <summary>
    ///     Resolve the MAC address for the host from ARP.
    /// </summary>
    public required bool ResolveMACAddress { get; init; }

    /// <summary>
    ///     Include unreachable IP addresses in the result.
    /// </summary>
    public required bool ShowAllResults { get; init; }

    /// <summary>
    ///     Raise every host in <see cref="IPScanner.HostFound" />, also unreachable ones (e.g. for a rescan of shown
    ///     hosts), without showing all ports like <see cref="ShowAllResults" />.
    /// </summary>
    public required bool ReportAllHosts { get; init; }
}
