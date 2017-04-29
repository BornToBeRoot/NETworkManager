using System;
using System.Net;
using System.Net.NetworkInformation;

namespace NETworkManager.Model.Network
{
    public class PingArgs : EventArgs
    {
        public IPAddress IPAddress { get; set; }
        public int Bytes { get; set; }
        public long Time { get; set; }
        public int TTL { get; set; }
        public IPStatus Status { get; set; }
    
        public PingArgs(IPAddress ipAddress, IPStatus status)
        {
            IPAddress = ipAddress;
            Status = status;
        }

        public PingArgs(IPAddress ipAddress, int bytes, long time, IPStatus status)
        {
            IPAddress = ipAddress;
            Bytes = bytes;
            Time = time;
            Status = status;
        }

        public PingArgs(IPAddress ipAddress, int bytes, long time, int ttl, IPStatus status)
        {
            IPAddress = ipAddress;
            Bytes = bytes;
            Time = time;
            TTL = ttl;
            Status = status;
        }
    }
}