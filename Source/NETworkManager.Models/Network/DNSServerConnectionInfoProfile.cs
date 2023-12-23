using System.Collections.Generic;

namespace NETworkManager.Models.Network;

/// <summary>
///     Class is used to store informations about a DNS server profile.
/// </summary>
public class DNSServerConnectionInfoProfile : ServerConnectionInfoProfile
{
    /// <summary>
    ///     Create an instance of <see cref="DNSServerConnectionInfoProfile" />. This will use the
    ///     default DNS server from Windows (<see cref="UseWindowsDNSServer" />=true).
    /// </summary>
    public DNSServerConnectionInfoProfile()
    {
        UseWindowsDNSServer = true;
    }

    /// <summary>
    ///     Create an instance of <see cref="DNSServerConnectionInfoProfile" /> with parameters.
    /// </summary>
    /// <param name="name">Name of the profile.</param>
    /// <param name="servers">List of servers as <see cref="ServerConnectionInfo" />.</param>
    public DNSServerConnectionInfoProfile(string name, List<ServerConnectionInfo> servers) : base(name, servers)
    {
    }

    /// <summary>
    ///     Use the DNS server from Windows.
    /// </summary>
    public bool UseWindowsDNSServer { get; set; }
}