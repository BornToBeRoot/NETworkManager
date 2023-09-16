using System.Net;

namespace NETworkManager.Models.Network;

public class ListenerInfo
{
    public TransportProtocol Protocol { get; set; }
    public IPAddress IPAddress { get; set; }
    public int Port { get; set; }

    public int IPAddressInt32 => IPAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? IPv4Address.ToInt32(IPAddress) : 0;

    public ListenerInfo()
    {

    }

    public ListenerInfo(TransportProtocol protocol, IPAddress ipAddress, int port)
    {
        Protocol = protocol;
        IPAddress = ipAddress;
        Port = port;
    }
}