using NETworkManager.Utilities;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network
{
    public static class SNTP
    {
        public static DateTime GetNetworkTimeRfc2030(IPEndPoint server)
        {
            var ntpData = new byte[48]; // RFC 2030
            ntpData[0] = 0x1B; // LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)
            
            var udpClient = new UdpClient(server.AddressFamily);
            udpClient.Client.SendTimeout = 4000;
            udpClient.Client.ReceiveTimeout = 4000;
            udpClient.Connect(server);

            udpClient.Send(ntpData, ntpData.Length);

            ntpData = udpClient.Receive(ref server);
            udpClient.Close();

            ulong intPart = (ulong)ntpData[40] << 24 | (ulong)ntpData[41] << 16 | (ulong)ntpData[42] << 8 | (ulong)ntpData[43];
            ulong fractPart = (ulong)ntpData[44] << 24 | (ulong)ntpData[45] << 16 | (ulong)ntpData[46] << 8 | (ulong)ntpData[47];

            var milliseconds = (intPart * 1000) + (fractPart * 1000 / 0x100000000L);
            var networkDateTime = new DateTime(1900, 1, 1).AddMilliseconds((long)milliseconds);

            return networkDateTime;
        }
    }
}
