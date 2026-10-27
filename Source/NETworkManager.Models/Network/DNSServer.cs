using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using DnsClient;

namespace NETworkManager.Models.Network;

/// <summary>
///     Class provides static information's about dns servers.
/// </summary>
public static class DNSServer
{
    /// <summary>
    ///     Separator of the entries of a DNS server input (e.g. "Google Public DNS; 9.9.9.9").
    /// </summary>
    private const char EntrySeparator = ';';

    /// <summary>
    ///     Default port of a DNS server entered without a port.
    /// </summary>
    private const int DefaultPort = 53;

    /// <summary>
    ///     Finds a DNS server profile by its name (case-insensitive, leading / trailing spaces are ignored).
    /// </summary>
    /// <param name="profiles">DNS server profiles.</param>
    /// <param name="name">Name of the DNS server profile.</param>
    /// <returns>DNS server profile, or null if there is none with the name.</returns>
    public static DNSServerConnectionInfoProfile FindByName(IEnumerable<DNSServerConnectionInfoProfile> profiles,
        string name)
    {
        var trimmedName = name?.Trim();

        return string.IsNullOrEmpty(trimmedName)
            ? null
            : profiles.FirstOrDefault(x =>
                string.Equals(x.Name?.Trim(), trimmedName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    ///     Gets the entries of a DNS server input that are neither the name of a DNS server profile nor a DNS server
    ///     (see <see cref="ResolveServers" />), without resolving anything.
    /// </summary>
    /// <param name="input">DNS server input.</param>
    /// <param name="profiles">DNS server profiles that can be used by their name.</param>
    /// <param name="hostnameNeedsDomain">
    ///     A hostname must contain a dot (e.g. the run command, where a single word can also be a mistyped type).
    /// </param>
    /// <returns>Invalid entries, the input itself if it is empty.</returns>
    public static List<string> GetInvalidEntries(string input, IEnumerable<DNSServerConnectionInfoProfile> profiles,
        bool hostnameNeedsDomain = false)
    {
        if (string.IsNullOrWhiteSpace(input))
            return [input ?? string.Empty];

        var profileList = profiles.ToList();

        return
        [
            .. input.Split(EntrySeparator)
                .Where(entry => FindByName(profileList, entry) == null &&
                                !TryParseServer(entry, hostnameNeedsDomain, out _))
                .Select(entry => entry.Trim())
        ];
    }

    /// <summary>
    ///     Resolves a DNS server input into DNS servers. Every entry (separated by ";") is the name of a DNS server
    ///     profile (e.g. "Google Public DNS", case-insensitive), which is replaced by its servers, or a DNS server (IP
    ///     address or hostname, optional port, e.g. "9.9.9.9" or "dns.example.com:5353") - e.g. "Google Public DNS;
    ///     9.9.9.9". The Windows DNS profile in a list is replaced by the DNS servers of Windows. Every server only once.
    /// </summary>
    /// <param name="input">DNS server input.</param>
    /// <param name="profiles">DNS server profiles that can be used by their name.</param>
    /// <param name="useWindowsDNSServer">
    ///     True if the input is only the Windows DNS profile: the DNS servers of Windows are used (no servers are
    ///     returned).
    /// </param>
    /// <param name="invalidEntries">Entries that are neither a DNS server profile nor a DNS server (skipped).</param>
    /// <returns>DNS servers (hostnames are not resolved).</returns>
    public static List<ServerConnectionInfo> ResolveServers(string input,
        IEnumerable<DNSServerConnectionInfoProfile> profiles, out bool useWindowsDNSServer,
        out List<string> invalidEntries)
    {
        useWindowsDNSServer = false;
        invalidEntries = [];

        List<ServerConnectionInfo> servers = [];

        if (string.IsNullOrWhiteSpace(input))
            return servers;

        var profileList = profiles.ToList();
        var entries = input.Split(EntrySeparator);

        foreach (var entry in entries)
        {
            var profile = FindByName(profileList, entry);

            if (profile is { UseWindowsDNSServer: true })
            {
                // Only the Windows DNS profile: DNS lookup uses the DNS servers of Windows itself
                if (entries.Length == 1)
                {
                    useWindowsDNSServer = true;
                    return servers;
                }

                servers.AddRange(NameServer.ResolveNameServers(true, false).Select(x =>
                    new ServerConnectionInfo(x.Address, x.Port, TransportProtocol.Udp)));
            }
            else if (profile != null)
            {
                servers.AddRange(profile.Servers);
            }
            else if (TryParseServer(entry, false, out var server))
            {
                servers.Add(server);
            }
            else
            {
                invalidEntries.Add(entry.Trim());
            }
        }

        return [.. servers.DistinctBy(x => (x.Server.ToLowerInvariant(), x.Port, x.TransportProtocol))];
    }

    /// <summary>
    ///     Parses a DNS server entry (IP address or hostname, optional port).
    /// </summary>
    private static bool TryParseServer(string entry, bool hostnameNeedsDomain, out ServerConnectionInfo server)
    {
        return ServerConnectionInfo.TryParse(entry, out server, DefaultPort, TransportProtocol.Udp) &&
               (!hostnameNeedsDomain || IPAddress.TryParse(server.Server, out _) || server.Server.Contains('.'));
    }

    /// <summary>
    ///     Gets the default list of common dns servers.
    /// </summary>
    /// <returns>List of common dns servers.</returns>
    public static List<DNSServerConnectionInfoProfile> GetDefaultList()
    {
        return
        [
            new(), // Windows DNS server
            new("Cloudflare",
            [
                new("1.1.1.1", 53, TransportProtocol.Udp),
                new("1.0.0.1", 53, TransportProtocol.Udp)
            ]),
            new("DNS.Watch",
            [
                new("84.200.69.80", 53, TransportProtocol.Udp),
                new("84.200.70.40", 53, TransportProtocol.Udp)
            ]),
            new("Google Public DNS",
            [
                new("8.8.8.8", 53, TransportProtocol.Udp),
                new("8.8.4.4", 53, TransportProtocol.Udp)
            ]),
            new("Level3",
            [
                new("209.244.0.3", 53, TransportProtocol.Udp),
                new("209.244.0.4", 53, TransportProtocol.Udp)
            ]),
            new("Quad9",
            [
                new ("9.9.9.9", 53, TransportProtocol.Udp),
                new ("149.112.112.112", 53 , TransportProtocol.Udp)
            ]),
            new("Verisign",
            [
                new("64.6.64.6", 53, TransportProtocol.Udp),
                new("64.6.65.6", 53, TransportProtocol.Udp)
            ])
        ];
    }
}