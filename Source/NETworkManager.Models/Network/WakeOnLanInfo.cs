using System.Net;

namespace NETworkManager.Models.Network
{
    public class WakeOnLANInfo
    {
        public byte[] MagicPacket;
        public IPAddress Broadcast;
        public int Port;

        public WakeOnLANInfo()
        {

        }

        public WakeOnLANInfo(byte[] magicPacket, IPAddress broadcast, int port)
        {
            MagicPacket = magicPacket;
            Broadcast = broadcast;
            Port = port;
        }
    }
}
