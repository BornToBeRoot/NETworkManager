using NETworkManager.Utilities;
using System;
using System.Net;
using System.Net.NetworkInformation;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NETworkManager.Models.Network
{
    public class PingInfo
    {
        public DateTime Timestamp { get; set; }

        [JsonConverter(typeof(JsonIPAddressConverter))]
        public IPAddress IPAddress { get; set; }

        public string Hostname { get; set; }
        public int Bytes { get; set; }
        public long Time { get; set; }
        public int TTL { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public IPStatus Status { get; set; }

        public int IPAddressInt32 => IPAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? IPv4AddressHelper.ConvertToInt32(IPAddress) : 0;

        public PingInfo()
        {

        }

        public PingInfo(IPAddress ipAddress, IPStatus ipStatus)
        {
            IPAddress = ipAddress;
            Status = ipStatus;
        }

        public PingInfo(DateTime timestamp, IPAddress ipAddress, IPStatus status)
        {
            Timestamp = timestamp;
            IPAddress = ipAddress;
            Status = status;
        }

        public PingInfo(DateTime timestamp, IPAddress ipAddress, int bytes, long time, IPStatus status)
        {
            Timestamp = timestamp;
            IPAddress = ipAddress;
            Bytes = bytes;
            Time = time;
            Status = status;
        }

        public PingInfo(DateTime timestamp, IPAddress ipAddress, int bytes, long time, int ttl, IPStatus status)
        {
            Timestamp = timestamp;
            IPAddress = ipAddress;
            Bytes = bytes;
            Time = time;
            TTL = ttl;
            Status = status;
        }

        public PingInfo(DateTime timestamp, IPAddress ipAddress, string hostname, int bytes, long time, int ttl, IPStatus status)
        {
            Timestamp = timestamp;
            IPAddress = ipAddress;
            Hostname = hostname;
            Bytes = bytes;
            Time = time;
            TTL = ttl;
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

        public static PingInfo Parse(PingReceivedArgs e)
        {
            return new PingInfo(e.Timestamp, e.IPAddress, e.Hostname, e.Bytes, e.Time, e.TTL, e.Status);
        }
    }
}