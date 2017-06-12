
using System.Net;
using System.Net.NetworkInformation;

namespace NETworkManager.Models.Network
{
    public class TracerouteHopInfo
    {
        public int Hop { get; set; }
        public double Time1 { get; set; }
        public double Time2 { get; set; }
        public double Time3 { get; set; }
        public IPAddress IPAddress { get; set; }
        public string Hostname { get; set; }
        public IPStatus Status1 { get; set; }
        public IPStatus Status2 { get; set; }
        public IPStatus Status3 { get; set; }

        public TracerouteHopInfo()
        {

        }

        public TracerouteHopInfo(int hop, long time1, long time2, long time3, IPAddress ipAddress, string hostname, IPStatus status1, IPStatus status2, IPStatus status3)
        {
            Hop = hop;
            Time1 = time1;
            Time2 = time2;
            Time3 = time3;
            IPAddress = ipAddress;
            Hostname = hostname;
            Status1 = status1;
            Status2 = status2;
            Status3 = status3;
        }

        public static TracerouteHopInfo Parse(TracerouteHopReceivedArgs e)
        {
            return new TracerouteHopInfo(e.Hop, e.Time1, e.Time2, e.Time3, e.IPAddress, e.Hostname, e.Status1,e.Status2,e.Status3);
        }
    }
}
