using System;
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
        Name = "[Windows DNS]";
        UseWindowsDNSServer = true;
    }

    /// <summary>
    ///     Create an instance of <see cref="DNSServerConnectionInfoProfile" /> with parameters.
    /// </summary>
    /// <param name="name">Name of the profile.</param>
    /// <param name="servers">List of servers as <see cref="ServerConnectionInfo" />.</param>
    public DNSServerConnectionInfoProfile(string name, List<ServerConnectionInfo> servers) : base(name, servers)
    {
        Id = Guid.NewGuid();
    }

    /// <summary>
    ///     Internal ID of the DNS server profile - profiles reference a DNS server profile by it, so a renamed DNS
    ///     server profile is still found. Kept when the DNS server profile is edited. The Windows DNS profile is
    ///     referenced by its name (it exists on every computer, with another ID).
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Use the DNS server from Windows.
    /// </summary>
    public bool UseWindowsDNSServer { get; set; }
}