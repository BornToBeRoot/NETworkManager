using System;
using System.Net;
using System.Net.NetworkInformation;

namespace NETworkManager.Models.Network
{
    public class PingReceivedArgs : EventArgs
    {
        public DateTime Timestamp { get; set; }
        public IPAddress IPAddress { get; set; }
        public string Hostname { get; set; }
        public int Bytes { get; set; }
        public long Time { get; set; }
        public int TTL { get; set; }
        public IPStatus Status { get; set; }
    
        public PingReceivedArgs(DateTime timestamp, IPAddress ipAddress, IPStatus status)
        {
            Timestamp = timestamp;
            IPAddress = ipAddress;
            Status = status;
        }

        public PingReceivedArgs(DateTime timestamp, IPAddress ipAddress, string hostname, IPStatus status)
        {
            Timestamp = timestamp;
            IPAddress = ipAddress;
            Hostname = hostname;
            Status = status;
        }

        public PingReceivedArgs(DateTime timestamp, IPAddress ipAddress, int bytes, long time, IPStatus status)
        {
            Timestamp = timestamp;
            IPAddress = ipAddress;
            Bytes = bytes;
            Time = time;
            Status = status;
        }

        public PingReceivedArgs(DateTime timestamp, IPAddress ipAddress, string hostname, int bytes, long time, IPStatus status)
        {
            Timestamp = timestamp;
            IPAddress = ipAddress;
            Hostname = hostname;
            Bytes = bytes;
            Time = time;
            Status = status;
        }

        public PingReceivedArgs(DateTime timestamp, IPAddress ipAddress, int bytes, long time, int ttl, IPStatus status)
        {
            Timestamp = timestamp;
            IPAddress = ipAddress;
            Bytes = bytes;
            Time = time;
            TTL = ttl;
            Status = status;
        }

        public PingReceivedArgs(DateTime timestamp, IPAddress ipAddress, string hostname, int bytes, long time, int ttl, IPStatus status)
        {
            Timestamp = timestamp;
            IPAddress = ipAddress;
            Hostname = hostname;
            Bytes = bytes;
            Time = time;
            TTL = ttl;
            Status = status;
        }
    }
}