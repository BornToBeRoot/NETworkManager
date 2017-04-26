namespace NETworkManager.Utilities.Network
{
    public static class MagicPacketHelper
    {
        public static byte[] Create(byte[] mac)
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

        public static byte[] Create(string mac)
        {
            byte[] macBytes = MACAddressHelper.ConvertStringToByteArray(mac);

            return Create(macBytes);
        }
    }
}