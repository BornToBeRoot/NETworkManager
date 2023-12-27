using System.Collections.Generic;

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
    /// <param name="hostname">Hostname of the host (dns or netbios).</param>
    /// <param name="dnsHostname">DNS hostname of the host.</param>
    /// <param name="isAnyPortOpen">Indicates whether any port is open.</param>
    /// <param name="ports">List of open ports.</param>
    /// <param name="netBIOSInfo">NetBIOS information about the host.</param>
    /// <param name="macAddress">MAC address of the host (ARP or netbios).</param>
    /// <param name="vendor">Vendor of the host based on the MAC address.</param>
    /// <param name="arpMacAddress">MAC address of the host received from ARP.</param>
    /// <param name="arpVendor">Vendor of the host based on the MAC address received from ARP.</param>
    public IPScannerHostInfo(bool isReachable, PingInfo pingInfo, string hostname, string dnsHostname,
        bool isAnyPortOpen, List<PortInfo> ports,
        NetBIOSInfo netBIOSInfo, string macAddress, string vendor, string arpMacAddress, string arpVendor)
    {
        IsReachable = isReachable;
        PingInfo = pingInfo;
        Hostname = hostname;
        DNSHostname = dnsHostname;
        IsAnyPortOpen = isAnyPortOpen;
        Ports = ports;
        NetBIOSInfo = netBIOSInfo;
        MACAddress = macAddress;
        Vendor = vendor;
        ARPMACAddress = arpMacAddress;
        ARPVendor = arpVendor;
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
    ///     Hostname of the host (dns or netbios).
    /// </summary>
    public string Hostname { get; set; }

    /// <summary>
    ///     DNS hostname of the host.
    /// </summary>
    public string DNSHostname { get; set; }

    /// <summary>
    ///     Indicates whether any port is open.
    /// </summary>
    public bool IsAnyPortOpen { get; set; }

    /// <summary>
    ///     List of open ports.
    /// </summary>
    public List<PortInfo> Ports { get; set; }

    /// <summary>
    ///     NetBIOS information about the host.
    /// </summary>
    public NetBIOSInfo NetBIOSInfo { get; set; }

    /// <summary>
    ///     MAC address of the host (ARP or netbios).
    /// </summary>
    public string MACAddress { get; set; }

    /// <summary>
    ///     Vendor of the host based on the MAC address.
    /// </summary>
    public string Vendor { get; set; }

    /// <summary>
    ///     MAC address of the host received from ARP.
    /// </summary>
    public string ARPMACAddress { get; set; }

    /// <summary>
    ///     Vendor of the host based on the MAC address received from ARP.
    /// </summary>
    public string ARPVendor { get; set; }
}