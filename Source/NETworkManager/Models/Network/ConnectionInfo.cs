using NETworkManager.Utilities;
using System.Net;
using System.Net.NetworkInformation;
using static NETworkManager.Models.Network.Connection;

namespace NETworkManager.Models.Network
{
    public class ConnectionInfo
    {
        public Protocol Protocol { get; set; }
        public IPAddress LocalIPAddress { get; set; }
        public int LocalPort { get; set; }
        public IPAddress RemoteIPAddress { get; set; }
        public int RemotePort { get; set; }
        public TcpState TcpState { get; set; }

        public int LocalIPAddressInt32 => LocalIPAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? IPv4AddressHelper.ConvertToInt32(LocalIPAddress) : 0;

        public int RemoteIPAddressInt32 => LocalIPAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? IPv4AddressHelper.ConvertToInt32(RemoteIPAddress) : 0;

        public ConnectionInfo()
        {

        }

        public ConnectionInfo(Protocol protocol, IPAddress localIPAddress, int localPort, IPAddress remoteIPAddress, int remotePort, TcpState tcpState)
        {
            Protocol = protocol;
            LocalIPAddress = localIPAddress;
            LocalPort = localPort;
            RemoteIPAddress = remoteIPAddress;
            RemotePort = remotePort;
            TcpState = tcpState;
        }
    }
}