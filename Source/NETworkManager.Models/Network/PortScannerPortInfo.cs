using NETworkManager.Models.Lookup;
using System.Net;

namespace NETworkManager.Models.Network;

public partial class PortScannerPortInfo : PortInfo
{
    public IPAddress IPAddress { get; set; }
    public string Hostname { get; set; }
    
    public int IPAddressInt32 => IPAddress != null && IPAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? IPv4Address.ToInt32(IPAddress) : 0;

    public PortScannerPortInfo(IPAddress ipAddress, string hostname, int port, PortLookupInfo lookupInfo, PortState status) : base(port, lookupInfo, status)
    {
        IPAddress = ipAddress;
        Hostname = hostname;        
    }

    public static PortScannerPortInfo Parse(PortScannerPortScannedArgs e)
    {
        return new PortScannerPortInfo(e.IPAddress, e.Hostname, e.Port, e.LookupInfo, e.State);
    }
}
