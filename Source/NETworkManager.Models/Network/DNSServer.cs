using System.Collections.Generic;

namespace NETworkManager.Models.Network;

/// <summary>
/// Class provides static information's about dns servers.
/// </summary>
public static class DNSServer
{
    /// <summary>
    /// Gets the default list of common dns servers.
    /// </summary>
    /// <returns>List of common dns servers.</returns>
    public static List<DNSServerConnectionInfoProfile> GetDefaultList() => new()
    {
            new DNSServerConnectionInfoProfile(), // Windows DNS server
            new DNSServerConnectionInfoProfile("Cloudflare", new List<ServerConnectionInfo>
            {
                 new("1.1.1.1", 53, TransportProtocol.Udp),
                 new("1.0.0.1", 53, TransportProtocol.Udp)
            }),
            new DNSServerConnectionInfoProfile("DNS.Watch", new List<ServerConnectionInfo>
            {    new("84.200.69.80", 53, TransportProtocol.Udp),
                 new("84.200.70.40", 53, TransportProtocol.Udp)
            }),
            new DNSServerConnectionInfoProfile("Google Public DNS", new List<ServerConnectionInfo>
            {
                 new("8.8.8.8", 53, TransportProtocol.Udp),
                 new("8.8.4.4", 53, TransportProtocol.Udp)
            }),
            new DNSServerConnectionInfoProfile("Level3", new List<ServerConnectionInfo>
            {
                 new("209.244.0.3", 53, TransportProtocol.Udp),
                 new("209.244.0.4", 53, TransportProtocol.Udp)
            }),
            new DNSServerConnectionInfoProfile("Verisign", new List<ServerConnectionInfo>
            {
                 new("64.6.64.6", 53, TransportProtocol.Udp),
                 new("64.6.65.6", 53, TransportProtocol.Udp)
            })
    };
}
