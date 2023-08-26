using System.Net;
using System.Net.NetworkInformation;
using NETworkManager.Models.IPApi;

namespace NETworkManager.Models.Network;

public class TracerouteHopInfo
{
    public int Hop { get; set; }
    public IPStatus Status1 { get; set; }
    public long Time1 { get; set; }
    public IPStatus Status2 { get; set; }
    public long Time2 { get; set; }
    public IPStatus Status3 { get; set; }
    public long Time3 { get; set; }
    public IPAddress IPAddress { get; set; }
    public string Hostname { get; set; }
    public IPGeolocationResult IPGeolocationResult { get; set; }

    public TracerouteHopInfo(int hop, IPStatus status1, long time1, IPStatus status2, long time2, IPStatus status3,
        long time3, IPAddress ipAddress, string hostname, IPGeolocationResult ipGeolocationResult)
    {
        Hop = hop;
        Status1 = status1;
        Time1 = time1;
        Status2 = status2;
        Time2 = time2;
        Status3 = status3;
        Time3 = time3;
        IPAddress = ipAddress;
        Hostname = hostname;
        IPGeolocationResult = ipGeolocationResult;
    }
}