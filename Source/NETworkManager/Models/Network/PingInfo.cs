
using System.Net;
using System.Net.NetworkInformation;

namespace NETworkManager.Models.Network
{
    public class PingInfo
    {
        public IPAddress IPAddress { get; set; }
        public int Bytes { get; set; }
        public long Time { get; set; }
        public int TTL { get; set; }
        public IPStatus Status { get; set; }

        public PingInfo()
        {

        }

        public PingInfo(IPAddress ipAddress, IPStatus status)
        {
            IPAddress = ipAddress;
            Status = status;
        }

        public PingInfo(IPAddress ipAddress, int bytes, long time, IPStatus status)
        {
            IPAddress = ipAddress;
            Bytes = bytes;
            Time = time;
            Status = status;
        }

        public PingInfo(IPAddress ipAddress, int bytes, long time, int ttl, IPStatus status)
        {
            IPAddress = ipAddress;
            Bytes = bytes;
            Time = time;
            TTL = ttl;
            Status = status;
        }

        public static PingInfo Parse(PingArgs e)
        {
            return new PingInfo(e.IPAddress, e.Bytes, e.Time, e.TTL, e.Status);
        }
    }
}