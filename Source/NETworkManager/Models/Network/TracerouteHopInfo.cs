
using System.Net;
using System.Net.NetworkInformation;

namespace NETworkManager.Models.Network
{
    public class TracerouteHopInfo
    {
        public int Hop { get; set; }
        public double Time { get; set; }
        public IPAddress IPAddress { get; set; }
        public string Hostname { get; set; }
        public IPStatus Status { get; set; }

        public TracerouteHopInfo()
        {

        }

        public TracerouteHopInfo(int hop, double time, IPAddress ipAddress, string hostname, IPStatus status)
        {
            Hop = hop;
            Time = time;
            IPAddress = ipAddress;
            Hostname = hostname;
            Status = status;
        }

        public static TracerouteHopInfo Parse(TracerouteHopReceivedArgs e)
        {
            return new TracerouteHopInfo(e.Hop, e.Time, e.IPAddress, e.Hostname, e.Status);
        }
    }
}
