using System.Net;
using System.Net.NetworkInformation;

namespace NETworkManager.Model.Network
{
    public class HopInfo
    {
        public int Hop { get; set; }
        public double Time { get; set; }
        public IPAddress IPAddress { get; set; }
        public string Hostname { get; set; }
        public IPStatus Status { get; set; }

        public HopInfo()
        {

        }

        public HopInfo(int hop, double time, IPAddress ipAddress, string hostname, IPStatus status)
        {
            Hop = hop;
            Time = time;
            IPAddress = ipAddress;
            Hostname = hostname;
            Status = status;
        }

        public static HopInfo Parse(HopReceivedArgs e)
        {
            return new HopInfo(e.Hop, e.Time, e.IPAddress, e.Hostname, e.Status);
        }
    }
}
