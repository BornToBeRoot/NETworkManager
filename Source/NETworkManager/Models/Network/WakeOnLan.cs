using NETworkManager.Utilities;
using System.Net.Sockets;

namespace NETworkManager.Models.Network
{
    public static class WakeOnLAN
    {
        #region Methods
        public static void Send(WakeOnLANInfo info)
        {
            using (var udpClient = new UdpClient())
            {
                udpClient.Connect(info.Broadcast, info.Port);

                udpClient.Send(info.MagicPacket, info.MagicPacket.Length);
            }
        }

        public static byte[] CreateMagicPacket(byte[] mac)
        {
            var packet = new byte[17 * 6];

            for (var i = 0; i < 6; i++)
                packet[i] = 0xFF;

            for (var i = 1; i <= 16; i++)
            {
                for (var j = 0; j < 6; j++)
                    packet[i * 6 + j] = mac[j];
            }

            return packet;
        }

        public static byte[] CreateMagicPacket(string mac)
        {
            var macBytes = MACAddressHelper.ConvertStringToByteArray(mac);

            return CreateMagicPacket(macBytes);
        }
        #endregion
    }
}
