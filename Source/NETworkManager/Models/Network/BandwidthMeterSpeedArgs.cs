using System;

namespace NETworkManager.Models.Network
{
    public class BandwidthMeterSpeedArgs : EventArgs
    {
        public DateTime DateTime { get; }
        public long TotalBytesReceived { get; }
        public long TotalBytesSent { get; }
        public long ByteReceivedSpeed { get; }
        public long ByteSentSpeed { get; }

        public BandwidthMeterSpeedArgs(DateTime dateTime, long totoTotalBytesReceived, long totalBytesSent, long byteReceivedSpeed, long byteSentSpeed)
        {
            DateTime = dateTime;
            TotalBytesReceived = totoTotalBytesReceived;
            TotalBytesSent = totalBytesSent;
            ByteReceivedSpeed = byteReceivedSpeed;
            ByteSentSpeed = byteSentSpeed;
        }
    }
}
