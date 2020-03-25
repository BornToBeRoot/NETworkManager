using NETworkManager.Utilities;
using System.Net;
using System.Net.NetworkInformation;
using NETworkManager.Models.Settings;
using static NETworkManager.Models.Network.Connection;
using NETworkManager.Localization;
using NETworkManager.Localization.Translators;

namespace NETworkManager.Models.Network
{
    public class ConnectionInfo
    {
        public Connection.Protocol Protocol { get; set; }
        public IPAddress LocalIPAddress { get; set; }
        public int LocalPort { get; set; }
        public IPAddress RemoteIPAddress { get; set; }
        public int RemotePort { get; set; }
        public TcpState TcpState { get; set; }

        public int LocalIPAddressInt32 => LocalIPAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? IPv4AddressConverter.ToInt32(LocalIPAddress) : 0;
        public int RemoteIPAddressInt32 => LocalIPAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? IPv4AddressConverter.ToInt32(RemoteIPAddress) : 0;
        public string TcpStateTranslated => TcpStateTranslator.GetInstance().Translate(TcpState);

        public ConnectionInfo()
        {

        }

        public ConnectionInfo(Connection.Protocol protocol, IPAddress localIPAddress, int localPort, IPAddress remoteIPAddress, int remotePort, TcpState tcpState)
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