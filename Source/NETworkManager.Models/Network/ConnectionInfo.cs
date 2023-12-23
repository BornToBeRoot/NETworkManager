using System.Net;
using System.Net.NetworkInformation;

namespace NETworkManager.Models.Network;

public class ConnectionInfo(
    TransportProtocol protocol,
    IPAddress localIPAddress,
    int localPort,
    IPAddress remoteIPAddress,
    int remotePort,
    string remoteHostname,
    TcpState tcpState,
    int processId,
    string processName,
    string processPath
)
{
    /// <summary>
    /// Protocol used by the connection. Only TCP is supported.
    /// </summary>
    public TransportProtocol Protocol { get; set; } = protocol;

    /// <summary>
    /// Local IP address of the connection.
    /// </summary>
    public IPAddress LocalIPAddress { get; set; } = localIPAddress;

    /// <summary>
    /// Local port of the connection. 
    /// </summary>
    public int LocalPort { get; set; } = localPort;

    /// <summary>
    /// Remote IP address of the connection.
    /// </summary>
    public IPAddress RemoteIPAddress { get; set; } = remoteIPAddress;

    /// <summary>
    /// Remote port of the connection.
    /// </summary>
    public int RemotePort { get; set; } = remotePort;
    
    /// <summary>
    /// Remote host name of the connection.
    /// </summary>
    public string RemoteHostname { get; set; } = remoteHostname;

    /// <summary>
    /// State of the connection.
    /// </summary>
    public TcpState TcpState { get; set; } = tcpState;

    /// <summary>
    /// PID of the process that owns the connection.
    /// </summary>
    public int ProcessId { get; set; } = processId;

    /// <summary>
    /// Name of the process that owns the connection.
    /// </summary>
    public string ProcessName { get; set; } = processName;

    /// <summary>
    /// Path of the process that owns the connection.
    /// </summary>
    public string ProcessPath { get; set; } = processPath;
}
