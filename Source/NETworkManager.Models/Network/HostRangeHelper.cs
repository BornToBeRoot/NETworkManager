using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network;

/// <summary>
///     Helper class to interact with host ranges.
///     E.g. Parse inputs, resolve hostnames and ip ranges.
/// </summary>
public static class HostRangeHelper
{
    /// <summary>
    ///     Number of addresses after which a range expansion checks for cancellation.
    /// </summary>
    private const int CancellationCheckInterval = 65536;

    /// <summary>
    ///     Create a list of hosts from a string input like "10.0.0.1; example.com; 10.0.0.0/24".
    ///     Inputs can also be separated by newlines (e.g. pasted from Excel, one host/range per line).
    /// </summary>
    /// <param name="hosts">Hosts like "10.0.0.1; example.com; 10.0.0.0/24" or newline-separated lines</param>
    /// <returns>List of hosts.</returns>
    public static IEnumerable<string> CreateListFromInput(string hosts)
    {
        return hosts.Replace(" ", "")
            .Split([';', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .ToArray();
    }

    /// <summary>
    ///     Adds every IPv4 address in the inclusive range [<paramref name="start" />, <paramref name="end" />] to
    ///     <paramref name="hosts" /> (in ascending order). Iterates using the unsigned IPv4 value widened to
    ///     <see cref="long" /> so the inclusive upper bound stays representable even for the last address in the
    ///     32-bit space (255.255.255.255), which would otherwise overflow <see cref="int" /> arithmetic.
    /// </summary>
    private static void AddIPv4Range(IPAddress start, IPAddress end,
        List<(IPAddress ipAddress, string hostname)> hosts, CancellationToken ct)
    {
        long from = IPv4Address.ToUInt32(start);
        long to = IPv4Address.ToUInt32(end);

        // Allocate the list once instead of growing it step by step (e.g. 65536 entries for a /16)
        if (hosts.Count == 0 && to >= from && to - from + 1 <= Array.MaxLength)
            hosts.Capacity = (int)(to - from + 1);

        for (var i = from; i <= to; i++)
        {
            if ((i - from) % CancellationCheckInterval == 0)
                ct.ThrowIfCancellationRequested();

            hosts.Add((IPv4Address.FromInt32(unchecked((int)i)), string.Empty));
        }
    }

    /// <summary>
    ///     Adds every IPv4 address of a special range like "192.168.[50-100,200].1" to <paramref name="hosts" />.
    /// </summary>
    private static void AddIPv4SpecialRange(string host, List<(IPAddress ipAddress, string hostname)> hosts,
        CancellationToken ct)
    {
        var octets = GetIPv4SpecialRangeOctets(host);

        // Build the new ipv4
        var count = 0;

        foreach (var i in octets[0])
        foreach (var j in octets[1])
        foreach (var k in octets[2])
        foreach (var h in octets[3])
        {
            if (count++ % CancellationCheckInterval == 0)
                ct.ThrowIfCancellationRequested();

            hosts.Add((new IPAddress([i, j, k, h]), string.Empty));
        }
    }

    /// <summary>
    ///     Gets the numbers of each octet of a special range like "192.168.[50-100,200].1".
    /// </summary>
    private static List<List<byte>> GetIPv4SpecialRangeOctets(string host)
    {
        var octets = new List<List<byte>>();

        foreach (var octet in host.Split('.'))
        {
            var numbers = new List<byte>();

            // Create a range for each octet
            if (Regex.IsMatch(octet, RegexHelper.SpecialRangeRegex))
                foreach (var numberOrRange in octet[1..^1].Split(','))
                    // 50-100
                    if (numberOrRange.Contains('-'))
                    {
                        var rangeNumbers = numberOrRange.Split('-');

                        for (var i = int.Parse(rangeNumbers[0]); i <= int.Parse(rangeNumbers[1]); i++)
                            numbers.Add((byte)i);
                    } // 200
                    else
                    {
                        numbers.Add(byte.Parse(numberOrRange));
                    }
            else
                numbers.Add(byte.Parse(octet));

            octets.Add(numbers);
        }

        return octets;
    }

    /// <summary>
    ///     Gets the number of IP addresses of the inputs without expanding the ranges or resolving the hostnames
    ///     (a hostname counts as 1, overlapping inputs are counted multiple times). Used to warn about (or refuse)
    ///     very large scans before any memory is allocated.
    /// </summary>
    /// <param name="hosts">Inputs like "10.0.0.1", "example.com", "10.0.0.0/24" (see <see cref="CreateListFromInput" />).</param>
    /// <returns>Number of IP addresses.</returns>
    public static long GetHostCount(IEnumerable<string> hosts)
    {
        long count = 0;

        foreach (var host in hosts)
        {
            switch (host)
            {
                // 192.168.0.1
                case var _ when RegexHelper.IPv4AddressRegex().IsMatch(host):
                    count++;
                    break;

                // 192.168.0.0/24 or 192.168.0.0/255.255.255.0
                case var _ when RegexHelper.IPv4AddressCidrRegex().IsMatch(host):
                case var _ when RegexHelper.IPv4AddressSubnetmaskRegex().IsMatch(host):
                    count += (long)IPNetwork2.Parse(host).Total;
                    break;

                // 192.168.0.1-100
                case var _ when RegexHelper.IPv4AddressShortRangeRegex().IsMatch(host):
                    var shortRange = host.Split('-');
                    var shortBase = shortRange[0][..shortRange[0].LastIndexOf('.')];

                    count += GetIPv4RangeCount(IPAddress.Parse(shortRange[0]),
                        IPAddress.Parse($"{shortBase}.{shortRange[1]}"));
                    break;

                // 192.168.0.0 - 192.168.0.100
                case var _ when RegexHelper.IPv4AddressRangeRegex().IsMatch(host):
                    var range = host.Split('-');

                    count += GetIPv4RangeCount(IPAddress.Parse(range[0]), IPAddress.Parse(range[1]));
                    break;

                // 192.168.[50-100].1
                case var _ when RegexHelper.IPv4AddressSpecialRangeRegex().IsMatch(host):
                    count += GetIPv4SpecialRangeOctets(host).Aggregate(1L, (product, octet) => product * octet.Count);
                    break;

                // example.com/24 or example.com/255.255.255.128 (IPv6 address and example.com count as 1)
                case var _ when Regex.IsMatch(host, RegexHelper.HostnameOrDomainWithCidrRegex):
                case var _ when Regex.IsMatch(host, RegexHelper.HostnameOrDomainWithSubnetmaskRegex):
                    count += (long)IPNetwork2.Parse($"0.0.0.0/{host.Split('/')[1]}").Total;
                    break;

                default:
                    count++;
                    break;
            }
        }

        return count;
    }

    /// <summary>
    ///     Gets the number of IPv4 addresses in the inclusive range [<paramref name="start" />, <paramref name="end" />].
    /// </summary>
    private static long GetIPv4RangeCount(IPAddress start, IPAddress end)
    {
        long from = IPv4Address.ToUInt32(start);
        long to = IPv4Address.ToUInt32(end);

        return Math.Max(to - from + 1, 0);
    }

    /// <summary>
    ///     Resolves the inputs (IP addresses, ranges, subnets, hostnames) into a list of hosts. The list is sorted
    ///     by IP address and contains every IP address only once (e.g. overlapping ranges); if an IP address was
    ///     also entered as hostname, the entry with the hostname is kept.
    /// </summary>
    /// <param name="hosts">Inputs like "10.0.0.1", "example.com", "10.0.0.0/24" (see <see cref="CreateListFromInput" />).</param>
    /// <param name="dnsResolveHostnamePreferIPv4">Prefer the A record over the AAAA record for hostnames.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Sorted, distinct list of hosts and the hostnames that could not be resolved.</returns>
    public static async Task<(List<(IPAddress ipAddress, string hostname)> hosts, List<string> hostnamesNotResolved)>
        ResolveAsync(IEnumerable<string> hosts, bool dnsResolveHostnamePreferIPv4, CancellationToken cancellationToken)
    {
        var inputs = hosts.ToList();

        // Results per input (index = position in the input) to keep the input order
        var hostsPerInput = new List<(IPAddress ipAddress, string hostname)>[inputs.Count];
        var hostnameNotResolvedPerInput = new string[inputs.Count];

        await Parallel.ForEachAsync(Enumerable.Range(0, inputs.Count),
            new ParallelOptions { CancellationToken = cancellationToken },
            async (index, ct) =>
        {
            var host = inputs[index];
            var list = new List<(IPAddress ipAddress, string hostname)>();

            hostsPerInput[index] = list;

            switch (host)
            {
                // 192.168.0.1
                case var _ when RegexHelper.IPv4AddressRegex().IsMatch(host):
                // 2001:db8:85a3::8a2e:370:7334
                case var _ when Regex.IsMatch(host, RegexHelper.IPv6AddressRegex):
                    list.Add((IPAddress.Parse(host), string.Empty));

                    break;

                // 192.168.0.0/24
                case var _ when RegexHelper.IPv4AddressCidrRegex().IsMatch(host):
                // 192.168.0.0/255.255.255.0
                case var _ when RegexHelper.IPv4AddressSubnetmaskRegex().IsMatch(host):
                    var network = IPNetwork2.Parse(host);

                    AddIPv4Range(network.Network, network.Broadcast, list, ct);

                    break;

                // 192.168.0.1-100
                case var _ when RegexHelper.IPv4AddressShortRangeRegex().IsMatch(host):
                    var shortRange = host.Split('-');
                    var shortBase = shortRange[0][..shortRange[0].LastIndexOf('.')];

                    AddIPv4Range(IPAddress.Parse(shortRange[0]),
                        IPAddress.Parse($"{shortBase}.{shortRange[1]}"), list, ct);

                    break;

                // 192.168.0.0 - 192.168.0.100
                case var _ when RegexHelper.IPv4AddressRangeRegex().IsMatch(host):
                    var range = host.Split('-');

                    AddIPv4Range(IPAddress.Parse(range[0]), IPAddress.Parse(range[1]), list, ct);

                    break;

                // 192.168.[50-100].1
                case var _ when RegexHelper.IPv4AddressSpecialRangeRegex().IsMatch(host):
                    AddIPv4SpecialRange(host, list, ct);

                    break;

                // example.com
                case var _ when RegexHelper.HostnameOrDomainRegex().IsMatch(host):
                    var dnsResult = await DNSClientHelper.ResolveAorAaaaAsync(host, dnsResolveHostnamePreferIPv4)
                        .WaitAsync(ct).ConfigureAwait(false);

                    if (!dnsResult.HasError)
                        list.Add((IPAddress.Parse($"{dnsResult.Value}"), host));
                    else
                        hostnameNotResolvedPerInput[index] = host;

                    break;

                // example.com/24 or example.com/255.255.255.128
                case var _ when Regex.IsMatch(host, RegexHelper.HostnameOrDomainWithCidrRegex):
                case var _ when Regex.IsMatch(host, RegexHelper.HostnameOrDomainWithSubnetmaskRegex):
                    var hostAndSubnet = host.Split('/');

                    // Only support IPv4
                    var dnsResultWithSubnet = await DNSClientHelper.ResolveAorAaaaAsync(hostAndSubnet[0], true)
                        .WaitAsync(ct).ConfigureAwait(false);

                    // Only support IPv4 for ranges for now
                    if (!dnsResultWithSubnet.HasError &&
                        dnsResultWithSubnet.Value.AddressFamily == AddressFamily.InterNetwork)
                    {
                        network = IPNetwork2.Parse($"{dnsResultWithSubnet.Value}/{hostAndSubnet[1]}");

                        AddIPv4Range(network.Network, network.Broadcast, list, ct);
                    }
                    else
                    {
                        hostnameNotResolvedPerInput[index] = hostAndSubnet[0];
                    }

                    break;
            }
        }).ConfigureAwait(false);

        return (SortAndRemoveDuplicates(hostsPerInput),
            [.. hostnameNotResolvedPerInput.Where(x => x != null)]);
    }

    /// <summary>
    ///     Merges the hosts of all inputs into one list sorted by IP address, with every IP address only once.
    ///     If an IP address exists with and without hostname, the entry with the hostname is kept.
    /// </summary>
    private static List<(IPAddress ipAddress, string hostname)> SortAndRemoveDuplicates(
        List<(IPAddress ipAddress, string hostname)>[] hostsPerInput)
    {
        var hosts = hostsPerInput.Length == 1 ? hostsPerInput[0] : [.. hostsPerInput.SelectMany(x => x)];

        // The common case (a single range or subnet) is already sorted and distinct - one cheap check
        // instead of sorting millions of entries again
        var isSortedAndDistinct = true;

        for (var i = 1; i < hosts.Count; i++)
        {
            if (IPAddressHelper.CompareIPAddresses(hosts[i - 1].ipAddress, hosts[i].ipAddress) < 0)
                continue;

            isSortedAndDistinct = false;
            break;
        }

        if (isSortedAndDistinct)
            return hosts;

        hosts.Sort((x, y) => IPAddressHelper.CompareIPAddresses(x.ipAddress, y.ipAddress));

        var distinctHosts = new List<(IPAddress ipAddress, string hostname)>(hosts.Count);

        foreach (var host in hosts)
        {
            if (distinctHosts.Count > 0 &&
                IPAddressHelper.CompareIPAddresses(distinctHosts[^1].ipAddress, host.ipAddress) == 0)
            {
                // Keep the entry with the hostname (e.g. "server01.example.com; 10.0.0.0/24")
                if (string.IsNullOrEmpty(distinctHosts[^1].hostname) && !string.IsNullOrEmpty(host.hostname))
                    distinctHosts[^1] = host;

                continue;
            }

            distinctHosts.Add(host);
        }

        return distinctHosts;
    }
}
