namespace NETworkManager.Models.Network;

/// <summary>
///     Class contains the options for the traceroute.
/// </summary>
public class IPScannerOptions
{
    /// <summary>
    ///     Create an instance of <see cref="IPScannerOptions" /> with parameters.
    /// </summary>
    /// <param name="maxHostThreads">Max concurrent threads for the host scan.</param>
    /// <param name="maxPortThreads">Max concurrent threads for the port scan of each host.</param>
    /// <param name="icmpAttempts">Number of attempts to ping a host.</param>
    /// <param name="icmpTimeout">Timeout in milliseconds after which a ping is considered lost.</param>
    /// <param name="icmpBuffer">Size of the buffer used in the ping in bytes.</param>
    /// <param name="portScanEnabled">Scan the ports of the host.</param>
    /// <param name="portScanPorts">List of ports to scan.</param>
    /// <param name="portScanTimeout">Timeout in milliseconds after which a port is considered closed.</param>
    /// <param name="resolveHostname">Resolve the hostname for an IP address.</param>
    /// <param name="netBIOSEnabled">Resolve the NetBIOS name for an IP address.</param>
    /// <param name="netBIOSTimeout">Timeout in milliseconds after which a NetBIOS request is considered lost.</param>
    /// <param name="resolveMACAddress">Resolve the MAC address for the host from ARP.</param>
    /// <param name="showAllResults">Include unreachable IP addresses in the result.</param>
    public IPScannerOptions(int maxHostThreads, int maxPortThreads, int icmpAttempts, int icmpTimeout,
        byte[] icmpBuffer, bool resolveHostname, bool portScanEnabled, int[] portScanPorts, int portScanTimeout,
        bool netBIOSEnabled, int netBIOSTimeout, bool resolveMACAddress, bool showAllResults)
    {
        MaxHostThreads = maxHostThreads;
        MaxPortThreads = maxPortThreads;
        ICMPAttempts = icmpAttempts;
        ICMPTimeout = icmpTimeout;
        ICMPBuffer = icmpBuffer;
        ResolveHostname = resolveHostname;
        PortScanEnabled = portScanEnabled;
        PortScanPorts = portScanPorts;
        PortScanTimeout = portScanTimeout;
        NetBIOSEnabled = netBIOSEnabled;
        NetBIOSTimeout = netBIOSTimeout;
        ResolveMACAddress = resolveMACAddress;
        ShowAllResults = showAllResults;
    }

    /// <summary>
    ///     Max concurrent threads for the host scan.
    /// </summary>
    public int MaxHostThreads { get; }

    /// <summary>
    ///     Max concurrent threads for the port scan of each host.
    /// </summary>
    public int MaxPortThreads { get; }

    /// <summary>
    ///     Number of attempts to ping a host.
    /// </summary>
    public int ICMPAttempts { get; }

    /// <summary>
    ///     Timeout in milliseconds after which a ping is considered lost.
    /// </summary>
    public int ICMPTimeout { get; }

    /// <summary>
    ///     Size of the buffer used in the ping in bytes.
    /// </summary>
    public byte[] ICMPBuffer { get; }

    /// <summary>
    ///     Resolve the hostname for an IP address.
    /// </summary>
    public bool ResolveHostname { get; }

    /// <summary>
    ///     Scan the ports of the host.
    /// </summary>
    public bool PortScanEnabled { get; }

    /// <summary>
    ///     Ports to scan.
    /// </summary>
    public int[] PortScanPorts { get; }

    /// <summary>
    ///     Timeout in milliseconds after which a port is considered closed.
    /// </summary>
    public int PortScanTimeout { get; }

    /// <summary>
    ///     Resolve the NetBIOS name for an IP address.
    /// </summary>
    public bool NetBIOSEnabled { get; }

    /// <summary>
    ///     Timeout in milliseconds after which a NetBIOS request is considered lost.
    /// </summary>
    public int NetBIOSTimeout { get; }

    /// <summary>
    ///     Resolve the MAC address for the host from ARP.
    /// </summary>
    public bool ResolveMACAddress { get; }

    /// <summary>
    ///     Include unreachable IP addresses in the result.
    /// </summary>
    public bool ShowAllResults { get; }
}