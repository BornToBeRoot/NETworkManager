namespace NETworkManager.Models.Lookup;

/// <summary>
/// Represents the protocol used by the port.
/// </summary>
public enum PortLookupProtocol
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