using System.Net;
using System.Net.NetworkInformation;

namespace NETworkManager.Models.Network
{
    public class HopReceivedArgs : System.EventArgs
    {
        public int Hop { get; set; }
        public double Time { get; set; }
        public IPAddress IPAddress { get; set; }
        public string Hostname { get; set; }
        public IPStatus Status { get; set; }

        public HopReceivedArgs()
        {

        }

        public HopReceivedArgs(int hop, double time, IPAddress ipAddress, string hostname, IPStatus status)
        {
            Hop = hop;
            Time = time;
            IPAddress = ipAddress;
            Hostname = hostname;
            Status = status;
        }
    }
}
