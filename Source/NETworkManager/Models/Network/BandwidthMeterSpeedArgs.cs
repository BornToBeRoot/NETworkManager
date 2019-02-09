namespace NETworkManager.Models.Network
{
    public class BandwidthMeterSpeedArgs : System.EventArgs
    {
        public long TotalBytesReceived { get; }
        public long TotalBytesSent { get; }
        public long ByteReceivedSpeed { get; }
        public long ByteSentSpeed { get; }

        public BandwidthMeterSpeedArgs(long totoTotalBytesReceived, long totalBytesSent, long byteReceivedSpeed, long byteSentSpeed)
        {
            TotalBytesReceived = totoTotalBytesReceived;
            TotalBytesSent = totalBytesSent;
            ByteReceivedSpeed = byteReceivedSpeed;
            ByteSentSpeed = byteSentSpeed;
        }
    }
}
