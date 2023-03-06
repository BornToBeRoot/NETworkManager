using System.Collections.Generic;

namespace NETworkManager.Models.Network;

/// <summary>
/// Class provides static informations about dns servers.
/// </summary>
public static class DNSServer
{
    /// <summary>
    /// Method will return a default list of common dns servers.
    /// </summary>
    /// <returns>List of common dns servers.</returns>
    public static List<DNSServerConnectionInfoProfile> GetDefaultList() => new()
    {
            new DNSServerConnectionInfoProfile(), // Windows DNS server
            new DNSServerConnectionInfoProfile("Cloudflare", new() {
                 new ServerConnectionInfo("1.1.1.1", 53, TransportProtocol.UDP),
                 new ServerConnectionInfo("1.0.0.1", 53, TransportProtocol.UDP)
            }),
            new DNSServerConnectionInfoProfile("DNS.Watch", new()
            {    new ServerConnectionInfo("84.200.69.80", 53, TransportProtocol.UDP),
                 new ServerConnectionInfo("84.200.70.40", 53, TransportProtocol.UDP)
            }),
            new DNSServerConnectionInfoProfile("Google Public DNS", new() {
                 new ServerConnectionInfo("8.8.8.8", 53, TransportProtocol.UDP),
                 new ServerConnectionInfo("8.8.4.4", 53, TransportProtocol.UDP)
            }),
            new DNSServerConnectionInfoProfile("Level3", new(){
                 new ServerConnectionInfo("209.244.0.3", 53, TransportProtocol.UDP),
                 new ServerConnectionInfo("209.244.0.4", 53, TransportProtocol.UDP)
            }),
            new DNSServerConnectionInfoProfile("Verisign", new()
            {
                 new ServerConnectionInfo("64.6.64.6", 53, TransportProtocol.UDP),
                 new ServerConnectionInfo("64.6.65.6", 53, TransportProtocol.UDP)
            })
    };
}
