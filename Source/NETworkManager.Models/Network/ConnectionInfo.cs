using System.Net;
using System.Net.NetworkInformation;

namespace NETworkManager.Models.Network;

public class ConnectionInfo
{
    public ConnectionInfo()
    {
    }

    public ConnectionInfo(TransportProtocol protocol, IPAddress localIPAddress, int localPort,
        IPAddress remoteIPAddress, int remotePort, TcpState tcpState)
    {
        Protocol = protocol;
        LocalIPAddress = localIPAddress;
        LocalPort = localPort;
        RemoteIPAddress = remoteIPAddress;
        RemotePort = remotePort;
        TcpState = tcpState;
    }

    public TransportProtocol Protocol { get; set; }
    public IPAddress LocalIPAddress { get; set; }
    public int LocalPort { get; set; }
    public IPAddress RemoteIPAddress { get; set; }
    public int RemotePort { get; set; }
    public TcpState TcpState { get; set; }
}