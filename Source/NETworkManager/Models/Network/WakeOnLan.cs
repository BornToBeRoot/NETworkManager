using NETworkManager.Utilities;
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

        public static byte[] CreateMagicPacket(byte[] mac)
        {
            byte[] packet = new byte[17 * 6];

            for (int i = 0; i < 6; i++)
                packet[i] = 0xFF;

            for (int i = 1; i <= 16; i++)
            {
                for (int j = 0; j < 6; j++)
                    packet[i * 6 + j] = mac[j];
            }

            return packet;
        }

        public static byte[] CreateMagicPacket(string mac)
        {
            byte[] macBytes = MACAddressHelper.ConvertStringToByteArray(mac);

            return CreateMagicPacket(macBytes);
        }
        #endregion
    }
}
