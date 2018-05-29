using NETworkManager.Utilities;
using System.Net;
using System.Net.NetworkInformation;

namespace NETworkManager.Models.Network
{
    public class ConnectionInfo
    {
        public IPAddress LocalIPAddress { get; set; }
        public int LocalPort { get; set; }
        public IPAddress RemoteIPAddress { get; set; }
        public int RemotePort { get; set; }
        public TcpState State { get; set; }

        public int IPAddressInt32
        {
            get { return LocalIPAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? IPv4AddressHelper.ConvertToInt32(LocalIPAddress) : 0; }
        }

        public ConnectionInfo()
        {

        }

        public ConnectionInfo(IPAddress localIPAddress, int localPort, IPAddress remoteIPAddress, int remotePort, TcpState state)
        {
            LocalIPAddress = localIPAddress;
            LocalPort = localPort;
            RemoteIPAddress = remoteIPAddress;
            RemotePort = remotePort;
            State = state;
        }
    }
}