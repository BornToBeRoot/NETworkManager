using DnsClient;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.Profiles.Application;

public static class DNSLookup
{
    /// <summary>
    ///     Gets the DNS servers for a profile: its own DNS servers (if overridden), otherwise the DNS servers of its
    ///     group (if overridden), otherwise none (the DNS server selected in the DNS Lookup is used). DNS servers by name
    ///     with their current name (also after a rename); if a DNS server can't be found (deleted, or a profile file
    ///     shared from another computer - DNS servers are saved in the settings), the servers saved with the profile or
    ///     group.
    /// </summary>
    /// <param name="profile">Profile to get the DNS servers for.</param>
    /// <param name="dnsServerNotFound">True if a DNS server can't be found.</param>
    /// <returns>DNS servers, null if neither the profile nor its group has DNS servers.</returns>
    public static string GetDNSServer(ProfileInfo profile, out bool dnsServerNotFound)
    {
        // Get group info
        var group = ProfileManager.GetGroupByName(profile.Group);

        var (dnsServer, dnsServerInput) = profile.DNSLookup_OverrideDNSServer
            ? (profile.DNSLookup_DNSServer, profile.DNSLookup_DNSServerInput)
            : group.DNSLookup_OverrideDNSServer
                ? (group.DNSLookup_DNSServer, group.DNSLookup_DNSServerInput)
                : (null, null);

        if (string.IsNullOrWhiteSpace(dnsServer))
        {
            dnsServerNotFound = false;

            return null;
        }

        return DNSServer.Decode(dnsServer, dnsServerInput, SettingsManager.Current.DNSLookup_DNSServers,
            out dnsServerNotFound);
    }

    /// <summary>
    ///     Gets the query type for a profile: its own type (if overridden), otherwise PTR for IP addresses and A for
    ///     hostnames.
    /// </summary>
    /// <param name="profile">Profile to get the query type for.</param>
    /// <returns>Query type.</returns>
    public static QueryType GetQueryType(ProfileInfo profile)
    {
        return profile.DNSLookup_OverrideQueryType
            ? profile.DNSLookup_QueryType
            : IPAddressHelper.AreIPAddresses(profile.DNSLookup_Host)
                ? QueryType.PTR
                : QueryType.A;
    }
}
