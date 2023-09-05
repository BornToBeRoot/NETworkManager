using System.Diagnostics.CodeAnalysis;
using NETworkManager.Models.Network;

namespace NETworkManager.Models.Lookup;

/// <summary>
/// Class with information about a port, protocol and service.
/// </summary>
public class PortLookupInfo
{
    /// <summary>
    /// Port number.
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    /// Port protocol.
    /// </summary>
    public TransportProtocol Protocol { get; set; }

    /// <summary>
    /// Service associated with the port number and protocol.
    /// </summary>
    public string Service { get; set; }

    /// <summary>
    /// Description of the service associated with the port number and protocol.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Create an instance of <see cref="PortLookupInfo"/> with parameters.
    /// </summary>
    /// <param name="number">Port number.</param>
    /// <param name="protocol">Port protocol.</param>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public PortLookupInfo(int number, TransportProtocol protocol)
    {
        Number = number;
        Protocol = protocol;
    }

    /// <summary>
    /// Create an instance of <see cref="PortLookupInfo"/> with parameters.
    /// </summary>
    /// <param name="number">Port number.</param>
    /// <param name="protocol">Port protocol.</param>
    /// <param name="service">Service associated with the port number and protocol.</param>
    /// <param name="description">Description of the service associated with the port number and protocol.</param>
    public PortLookupInfo(int number, TransportProtocol protocol, string service, string description) : this(number, protocol)
    {
        Service = service;
        Description = description;
    }
}
