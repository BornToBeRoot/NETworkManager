using System;

namespace NETworkManager.Models.Network
{
    public class BandwidthInfo
    {
        public DateTime DateTime { get; set; }
        public long Value { get; set; }

        public BandwidthInfo(DateTime dateTime, long value)
        {
            DateTime = dateTime;
            Value = value;
        }
    }
}
