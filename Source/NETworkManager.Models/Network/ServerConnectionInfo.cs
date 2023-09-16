using System.Net;

namespace NETworkManager.Models.Network;

/// <summary>
/// Class contains information about a server.
/// </summary>
public class ServerConnectionInfo
{
    /// <summary>
    /// Server name or IP address.
    /// </summary>
    public string Server { get; set; }

    /// <summary>
    /// Port used for the connection.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Transport protocol used for the connection.
    /// </summary>
    public TransportProtocol TransportProtocol { get; set; }

    /// <summary>
    /// Create an instance of <see cref="ServerConnectionInfo"/>.
    /// </summary>
    public ServerConnectionInfo()
    {
        
    }

    /// <summary>
    /// Create an instance of <see cref="ServerConnectionInfo"/> with parameters. Default transport protocol is TCP.
    /// </summary>
    /// <param name="server">Server name or IP address.</param>
    /// <param name="port">Port used for the connection.</param>
    public ServerConnectionInfo(string server, int port)
    {
        Server = server;
        Port = port;
        TransportProtocol = TransportProtocol.Tcp;
    }

    /// <summary>
    /// Create an instance of <see cref="ServerConnectionInfo"/> with parameters. 
    /// </summary>
    /// <param name="server">Server name or IP address.</param>
    /// <param name="port">Port used for the connection.</param>
    /// <param name="transportProtocol">Transport protocol used for the connection.</param>
    public ServerConnectionInfo(string server, int port, TransportProtocol transportProtocol)
    {
        Server = server;
        Port = port;
        TransportProtocol = transportProtocol;
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>Server:Port</returns>
    public override string ToString()
    {
        return $"{Server}:{Port}";
    }
}
