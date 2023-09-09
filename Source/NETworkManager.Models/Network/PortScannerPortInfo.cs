using NETworkManager.Models.Lookup;
using System.Net;

namespace NETworkManager.Models.Network;

/// <summary>
/// Class containing information about a port in a <see cref="PortScanner"/>.
/// </summary>
public class PortScannerPortInfo : PortInfo
{
    /// <summary>
    /// IP address of the host.
    /// </summary>
    public IPAddress IPAddress { get; set; }
    
    /// <summary>
    /// Hostname of the host.
    /// </summary>
    public string Hostname { get; set; }
    
    /// <summary>
    /// IP address of the host as a integer.
    /// </summary>
    public int IPAddressInt32 => IPAddress is { AddressFamily: System.Net.Sockets.AddressFamily.InterNetwork } ? IPv4Address.ToInt32(IPAddress) : 0;

    /// <summary>
    /// Creates a new instance of <see cref="PortScannerPortInfo"/> with the specified parameters.
    /// </summary>
    /// <param name="ipAddress">IP address of the host.</param>
    /// <param name="hostname">Hostname of the host.</param>
    /// <param name="port">Port number.</param>
    /// <param name="lookupInfo">Port lookup info like service and description.</param>
    /// <param name="status">State of the port.</param>
    public PortScannerPortInfo(IPAddress ipAddress, string hostname, int port, PortLookupInfo lookupInfo,
        PortState status) : base(port, lookupInfo, status)
    {
        IPAddress = ipAddress;
        Hostname = hostname;
    }
}
