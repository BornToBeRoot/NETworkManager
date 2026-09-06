using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace NETworkManager.Utilities;

public static class DNSClientHelper
{
    /// <summary>
    ///     Detect the DNS suffix to use for suffix-appending. Prefers the Windows "Primary DNS Suffix"
    ///     (<see cref="IPGlobalProperties.DomainName" />), falling back to the connection-specific suffix
    ///     of an active network adapter if no primary suffix is configured (e.g. machine is not domain-joined
    ///     or the primary suffix was cleared manually), which mirrors what shows up under "DNS Suffix Search
    ///     List" in <c>ipconfig /all</c> in that case.
    /// </summary>
    public static string DetectDNSSuffix()
    {
        var suffix = IPGlobalProperties.GetIPGlobalProperties().DomainName;

        if (!string.IsNullOrWhiteSpace(suffix))
            return suffix;

        // Rank candidates so the adapter most likely to be "the" relevant connection is tried first:
        // VPN/tunnel > wired > wireless > other, then routed (has a gateway), then faster link speed.
        // VPN ranks above wired/wireless because it's usually why suffix resolution matters in the first
        // place (reaching internal/corporate hostnames), and a split-tunnel VPN commonly has no default
        // gateway at all, so gateway-presence can't be the primary key without losing to e.g. a home
        // Ethernet connection. A candidate is only ever picked if it has a real suffix (see below), so
        // ranking a VPN/tunnel first doesn't risk picking up an irrelevant OS-internal pseudo-tunnel
        // (Teredo, ISATAP, ...) - those essentially never carry a connection-specific DNS suffix.
        var prioritizedAdapters = NetworkInterface.GetAllNetworkInterfaces()
            .Where(nic => nic.OperationalStatus == OperationalStatus.Up &&
                          nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            .OrderByDescending(nic => GetInterfaceTypePriority(nic.NetworkInterfaceType))
            .ThenByDescending(nic => nic.GetIPProperties().GatewayAddresses.Count > 0)
            .ThenByDescending(nic => nic.Speed);

        return prioritizedAdapters
            .Select(nic => nic.GetIPProperties().DnsSuffix)
            .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s))
            ?? string.Empty;
    }

    private static int GetInterfaceTypePriority(NetworkInterfaceType type) => type switch
    {
        NetworkInterfaceType.Tunnel => 3,
        NetworkInterfaceType.Ppp => 3,
        NetworkInterfaceType.Ethernet => 2,
        NetworkInterfaceType.Wireless80211 => 1,
        _ => 0
    };

    public static async Task<DNSClientResultIPAddress> ResolveAorAaaaAsync(string query, bool preferIPv4)
    {
        DNSClientResultIPAddress firstResult = null;

        for (var i = 0; i < 2; i++)
        {
            if (preferIPv4)
            {
                var resultIPv4 = await DNSClient.GetInstance().ResolveAAsync(query);

                if (!resultIPv4.HasError)
                    return resultIPv4;
                firstResult ??= resultIPv4;
            }
            else
            {
                var resultIPv6 = await DNSClient.GetInstance().ResolveAaaaAsync(query);

                if (!resultIPv6.HasError)
                    return resultIPv6;
                firstResult ??= resultIPv6;
            }

            preferIPv4 = !preferIPv4;
        }

        return firstResult;
    }

    public static string FormatDNSClientResultError(string query, DNSClientResult result)
    {
        var statusMessage = $"{query}";

        if (string.IsNullOrEmpty(result.DNSServer))
            statusMessage += $" ==> {result.ErrorMessage}";
        else
            statusMessage += $" @ {result.DNSServer} ==> {result.ErrorMessage}";

        return statusMessage;
    }
}