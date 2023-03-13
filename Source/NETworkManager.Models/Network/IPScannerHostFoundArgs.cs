using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace NETworkManager.Models.Network;

public class IPScannerHostFoundArgs : System.EventArgs
{
    public bool IsReachable { get; set; }

    public PingInfo PingInfo { get; set; }

    public bool IsAnyPortOpen { get; set; }

    public List<PortInfo> Ports { get; set; }

    public string Hostname { get; set; }

    public PhysicalAddress MACAddress { get; set; }

    public string Vendor { get; set; }

    public IPScannerHostFoundArgs(bool isReachable, PingInfo pingInfo, bool isAnyPortOpen, List<PortInfo> ports, string hostname, PhysicalAddress macAddress, string vendor)
    {
        IsReachable = isReachable;
        PingInfo = pingInfo;
        IsAnyPortOpen = isAnyPortOpen;
        Ports = ports;
        Hostname = hostname;
        MACAddress = macAddress;
        Vendor = vendor;
    }
}
