using System.Net;
using System.Net.NetworkInformation;

namespace NETworkManager.Models.Network
{
    public class PingReceivedArgs : System.EventArgs
    {
        public IPAddress IPAddress { get; set; }
        public int Bytes { get; set; }
        public long Time { get; set; }
        public int TTL { get; set; }
        public IPStatus Status { get; set; }
    
        public PingReceivedArgs(IPAddress ipAddress, IPStatus status)
        {
            IPAddress = ipAddress;
            Status = status;
        }

        public PingReceivedArgs(IPAddress ipAddress, int bytes, long time, IPStatus status)
        {
            IPAddress = ipAddress;
            Bytes = bytes;
            Time = time;
            Status = status;
        }

        public PingReceivedArgs(IPAddress ipAddress, int bytes, long time, int ttl, IPStatus status)
        {
            IPAddress = ipAddress;
            Bytes = bytes;
            Time = time;
            TTL = ttl;
            Status = status;
        }
    }
}