using System.Collections.Generic;

namespace NETworkManager.Models.Network;

/// <summary>
/// Class contains the options for the traceroute.
/// </summary>
public class IPScannerOptions
{
    /// <summary>
    /// Max cuncurrent threads for the host scan.
    /// </summary>
    public int MaxHostThreads { get; set; }

    /// <summary>
    /// Max cuncurrent threads for the port scan of each host.
    /// </summary>
    public int MaxPortThreads { get; set; }

    /// <summary>
    /// Number of attempts to ping a host.
    /// </summary>
    public int ICMPAttempts { get; set; }

    /// <summary>
    /// Timeout in milliseconds after which a ping is considered lost.
    /// </summary>
    public int ICMPTimeout { get; set; }

    /// <summary>
    /// Size of the buffer used in the ping in bytes.
    /// </summary>
    public byte[] ICMPBuffer { get; set; }

    /// <summary>
    /// Scan the ports of the host.
    /// </summary>
    public bool ScanPorts { get; set; }

    /// <summary>
    /// List of ports to scan.
    /// </summary>
    public List<int> Ports { get; set; }

    /// <summary>
    /// Resolve the hostname for an IP address.
    /// </summary>
    public bool ResolveHostname { get; set; }

    /// <summary>
    /// Show the error message if the hostname could not be resolved.
    /// </summary>
    public bool DNSShowErrorMessage { get; set; }

    /// <summary>
    /// Resolve the MAC address for the host from ARP.
    /// </summary>
    public bool ResolveMACAddress { get; set; }

    /// <summary>
    /// Include unreachable IP addresses in the result.
    /// </summary>
    public bool ShowAllResults { get; set; }

    /// <summary>
    /// Create an instance of <see cref="IPScannerOptions"/>.
    /// </summary>
    public IPScannerOptions()
    {

    }

    /// <summary>
    /// Create an instance of <see cref="IPScannerOptions"/> with parameters.
    /// </summary>
    /// <param name="maxHostThreads">Max cuncurrent threads for the host scan.</param>
    /// <param name="maxPortThreads">Max cuncurrent threads for the port scan of each host.</param>
    /// <param name="icmpAttempts">Number of attempts to ping a host.</param>
    /// <param name="icmpTimeout">Timeout in milliseconds after which a ping is considered lost.</param>
    /// <param name="icmpBuffer">Size of the buffer used in the ping in bytes.</param>
    /// <param name="scanPorts">Scan the ports of the host.</param>
    /// <param name="ports">List of ports to scan.</param>
    /// <param name="resolveHostname">Resolve the hostname for an IP address.</param>
    /// <param name="dnsShowErrorMessage">Show the error message if the hostname could not be resolved.</param>
    /// <param name="resolveMACAddress">Resolve the MAC address for the host from ARP.</param>
    /// <param name="showAllResults">Include unreachable IP addresses in the result.</param>
    public IPScannerOptions(int maxHostThreads, int maxPortThreads, int icmpAttempts, int icmpTimeout, byte[] icmpBuffer, bool scanPorts, List<int> ports, bool resolveHostname, bool dnsShowErrorMessage, bool resolveMACAddress, bool showAllResults)
    {
        MaxHostThreads = maxHostThreads;
        MaxPortThreads = maxPortThreads;
        ICMPAttempts = icmpAttempts;
        ICMPTimeout = icmpTimeout;
        ICMPBuffer = icmpBuffer;
        ScanPorts = scanPorts;
        Ports = ports;
        ResolveHostname = resolveHostname;
        DNSShowErrorMessage = dnsShowErrorMessage;
        ResolveMACAddress = resolveMACAddress;
        ShowAllResults = showAllResults;
    }
}
