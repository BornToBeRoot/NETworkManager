using System.Net.NetworkInformation;

namespace NETworkManager.Models.Network;

public class HostInfo
{
    public bool IsReachable { get; set; }

    public PingInfo PingInfo { get; set; }

    public string Hostname { get; set; }

    public bool PortStatus { get; set; } = true;

    public PhysicalAddress MACAddress { get; set; }

    public string Vendor { get; set; }

    public string MACAddressString => MACAddress?.ToString();

    public HostInfo()
    {

    }

    public HostInfo(bool isReachable, PingInfo pingInfo, string hostname, PhysicalAddress macAddress, string vendor)
    {
        IsReachable = isReachable;
        PingInfo = pingInfo;
        Hostname = hostname;
        MACAddress = macAddress;
        Vendor = vendor;
    }

    public static HostInfo Parse(HostFoundArgs e)
    {
        return new HostInfo(e.IsReachable, e.PingInfo, e.Hostname, e.MACAddress, e.Vendor);
    }
}
