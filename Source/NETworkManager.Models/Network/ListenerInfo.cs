using System.Net;

namespace NETworkManager.Models.Network;

public class ListenerInfo(TransportProtocol protocol, IPAddress ipAddress, int port)
{
    /// <summary>
    /// Transport protocol of the listener.
    /// </summary>
    public TransportProtocol Protocol { get; set; } = protocol;
    
    /// <summary>
    /// IP address of the listener.
    /// </summary>
    public IPAddress IPAddress { get; set; } = ipAddress;
    
    /// <summary>
    /// Port of the listener.
    /// </summary>
    public int Port { get; set; } = port;
}
