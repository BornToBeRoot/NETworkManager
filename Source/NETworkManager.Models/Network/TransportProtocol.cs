namespace NETworkManager.Models.Network;

/// <summary>
/// Enum for the transport protocol.
/// </summary>
public enum TransportProtocol
{
    /// <summary>
    /// Transmission Control Protocol (TCP).
    /// </summary>
    Tcp,
    
    /// <summary>
    /// User Datagram Protocol (UDP).
    /// </summary>
    Udp,
    
    /// <summary>
    /// Stream Control Transmission Protocol (SCTP).
    /// </summary>
    Sctp,
    
    /// <summary>
    /// Datagram Congestion Control Protocol (DCCP).
    /// </summary>
    Dccp
}
