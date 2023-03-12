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
    public bool PortScanEnabled { get; set; }

    /// <summary>
    /// Ports to scan.
    /// </summary>
    public int[] PortScanPorts { get; set; }

    /// <summary>
    /// Timeout in milliseconds after which a port is considered closed.
    /// </summary>
    public int PortScanTimeout { get; set; }

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
    /// Create an instance of <see cref="IPScannerOptions"/> with parameters.
    /// </summary>
    /// <param name="maxHostThreads">Max cuncurrent threads for the host scan.</param>
    /// <param name="maxPortThreads">Max cuncurrent threads for the port scan of each host.</param>
    /// <param name="icmpAttempts">Number of attempts to ping a host.</param>
    /// <param name="icmpTimeout">Timeout in milliseconds after which a ping is considered lost.</param>
    /// <param name="icmpBuffer">Size of the buffer used in the ping in bytes.</param>
    /// <param name="portScanEnabled">Scan the ports of the host.</param>
    /// <param name="portScanPorts">List of ports to scan.</param>
    /// <param name="portScanTimeout">Timeout in milliseconds after which a port is considered closed.</param>
    /// <param name="resolveHostname">Resolve the hostname for an IP address.</param>
    /// <param name="dnsShowErrorMessage">Show the error message if the hostname could not be resolved.</param>
    /// <param name="resolveMACAddress">Resolve the MAC address for the host from ARP.</param>
    /// <param name="showAllResults">Include unreachable IP addresses in the result.</param>
    public IPScannerOptions(int maxHostThreads, int maxPortThreads, int icmpAttempts, int icmpTimeout, byte[] icmpBuffer, bool portScanEnabled, int[] portScanPorts, int portScanTimeout, bool resolveHostname, bool dnsShowErrorMessage, bool resolveMACAddress, bool showAllResults)
    {
        MaxHostThreads = maxHostThreads;
        MaxPortThreads = maxPortThreads;
        ICMPAttempts = icmpAttempts;
        ICMPTimeout = icmpTimeout;
        ICMPBuffer = icmpBuffer;
        PortScanEnabled = portScanEnabled;
        PortScanPorts = portScanPorts;
        PortScanTimeout = portScanTimeout;
        ResolveHostname = resolveHostname;
        DNSShowErrorMessage = dnsShowErrorMessage;
        ResolveMACAddress = resolveMACAddress;
        ShowAllResults = showAllResults;
    }
}
