using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace NETworkManager.Models.Network;

/// <summary>
///     Class containing information about a host in a <see cref="IPScanner" />.
/// </summary>
public class IPScannerHostInfo
{
    /// <summary>
    ///     Creates a new instance of <see cref="IPScannerHostInfo" /> with the specified parameters.
    /// </summary>
    /// <param name="isReachable">Indicates whether the host is reachable.</param>
    /// <param name="pingInfo">Information about the ping to the host.</param>
    /// <param name="isAnyPortOpen">Indicates whether any port is open.</param>
    /// <param name="ports">List of open ports.</param>
    /// <param name="hostname">Hostname of the host.</param>
    /// <param name="macAddress">MAC address of the host.</param>
    /// <param name="vendor">Vendor of the host based on the MAC address.</param>
    public IPScannerHostInfo(bool isReachable, PingInfo pingInfo, bool isAnyPortOpen, List<PortInfo> ports,
        string hostname, PhysicalAddress macAddress, string vendor)
    {
        IsReachable = isReachable;
        PingInfo = pingInfo;
        IsAnyPortOpen = isAnyPortOpen;
        Ports = ports;
        Hostname = hostname;
        MACAddress = macAddress;
        Vendor = vendor;
    }

    /// <summary>
    ///     Indicates whether the host is reachable.
    /// </summary>
    public bool IsReachable { get; set; }

    /// <summary>
    ///     Information about the ping to the host.
    /// </summary>
    public PingInfo PingInfo { get; set; }

    /// <summary>
    ///     Indicates whether any port is open.
    /// </summary>
    public bool IsAnyPortOpen { get; set; }

    /// <summary>
    ///     List of open ports.
    /// </summary>
    public List<PortInfo> Ports { get; set; }

    /// <summary>
    ///     Hostname of the host.
    /// </summary>
    public string Hostname { get; set; }

    /// <summary>
    ///     MAC address of the host.
    /// </summary>
    public PhysicalAddress MACAddress { get; set; }

    /// <summary>
    ///     Vendor of the host based on the MAC address.
    /// </summary>
    public string Vendor { get; set; }

    /// <summary>
    ///     MAC address of the host as a string.
    /// </summary>
    public string MACAddressString => MACAddress?.ToString();
}