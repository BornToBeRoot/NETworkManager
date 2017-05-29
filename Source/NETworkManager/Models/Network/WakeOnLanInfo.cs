using System.Net;

namespace NETworkManager.Models.Network
{
    public class WakeOnLanInfo
    {
        public byte[] MagicPacket;
        public IPAddress Broadcast;
        public int Port;

        public WakeOnLanInfo()
        {

        }

        public WakeOnLanInfo(byte[] magicPacket, IPAddress broadcast, int port)
        {
            MagicPacket = magicPacket;
            Broadcast = broadcast;
            Port = port;
        }
    }
}
