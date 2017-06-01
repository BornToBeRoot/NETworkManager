using System.Net.Sockets;

namespace NETworkManager.Models.Network
{
    public static class WakeOnLAN
    {
        #region Methods
        public static void Send(WakeOnLANInfo info)
        {
            using (UdpClient udpClient = new UdpClient())
            {
                udpClient.Connect(info.Broadcast, info.Port);

                udpClient.Send(info.MagicPacket, info.MagicPacket.Length);
            }
        }
        #endregion
    }
}
