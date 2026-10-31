using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using NETworkManager.Models.Network;
using NETworkManager.Utilities;

namespace NETworkManager.Models.RunCommand;

/// <summary>
///     Detects the kind of a target that is typed without a command, so the commands that accept it can be suggested
///     (e.g. "10.0.0.1" → ping, traceroute, ...). A single word without a domain is
///     <see cref="RunCommandTargetKind.Hostname" />, since it can also be a mistyped command.
/// </summary>
public static class RunCommandTarget
{
    /// <summary>
    ///     Checks if the ports of an entry of the port lookup (e.g. "22", "80-90/tcp") are valid, like the port lookup.
    /// </summary>
    private static bool IsValidPortLookupEntry(string entry)
    {
        var ports = entry.Split('/')[0].Split('-').Select(x => int.TryParse(x, out var port) ? port : 0).ToArray();

        return ports.Length == 1
            ? PortRangeHelper.IsValidPort(ports[0])
            : PortRangeHelper.IsValidPortRange(ports[0], ports[1]);
    }

    /// <summary>
    ///     Gets the kind of the target.
    /// </summary>
    /// <param name="target">Target without spaces (e.g. "10.0.0.1" or "10.0.0.1;10.0.0.2").</param>
    /// <returns>Kind of the target, <see cref="RunCommandTargetKind.None" /> if it is not clearly a target.</returns>
    public static RunCommandTargetKind GetKind(string target)
    {
        if (string.IsNullOrEmpty(target) || target.Any(char.IsWhiteSpace))
            return RunCommandTargetKind.None;

        if (RegexHelper.HttpOrHttpsSchemeRegex().IsMatch(target))
            return Uri.TryCreate(target, UriKind.Absolute, out _) ? RunCommandTargetKind.URL : RunCommandTargetKind.None;

        if (Regex.IsMatch(target, RegexHelper.MACAddressRegex))
            return RunCommandTargetKind.MACAddress;

        // Only with separators: 6 hex digits without separators (e.g. "facade") are more likely a word.
        if (RegexHelper.MACAddressFirst3BytesWithSeparatorRegex().IsMatch(target))
            return RunCommandTargetKind.OUI;

        // Like the port lookup: ports from 1 to 65535 and ranges from a lower to a higher port (e.g. not "99999" or
        // "90-80", which the port lookup searches as a service)
        if (RegexHelper.PortLookupEntriesRegex().IsMatch(target))
            return target.Split(';').All(IsValidPortLookupEntry) ? RunCommandTargetKind.Ports : RunCommandTargetKind.None;

        if (RegexHelper.ASNumberWithPrefixRegex().IsMatch(target))
            return RunCommandTargetKind.ASN;

        // Several hosts, IP ranges or subnets, optionally with exclusions (e.g. "10.0.0.0/24;!10.0.0.1")
        if (target.Contains(';'))
        {
            var entries = target.Split(';', StringSplitOptions.RemoveEmptyEntries);

            var kinds = entries.Where(x => !HostRangeHelper.IsExclusion(x)).Select(GetKind).ToList();
            var exclusionKinds = entries.Where(HostRangeHelper.IsExclusion).Select(x => GetKind(x[1..])).ToList();

            if (kinds.Count == 0 || kinds.Concat(exclusionKinds).Any(x => x is not (RunCommandTargetKind.Host
                    or RunCommandTargetKind.Hostname or RunCommandTargetKind.IPRange or RunCommandTargetKind.Subnet)))
                return RunCommandTargetKind.None;

            // Only host names without a domain (e.g. "foo;" or "a;b" from copied text) can also be any words, like a
            // single host name without a domain is not a list.
            if (kinds.All(x => x == RunCommandTargetKind.Hostname))
                return RunCommandTargetKind.None;

            // With an IP range, subnet or exclusion, it is a range for the scanners (e.g. not for a DNS lookup).
            return exclusionKinds.Count > 0 ||
                   kinds.Any(x => x is RunCommandTargetKind.IPRange or RunCommandTargetKind.Subnet)
                ? RunCommandTargetKind.IPRange
                : RunCommandTargetKind.Hosts;
        }

        // Subnet with CIDR or subnet mask (e.g. 10.0.0.0/24, 10.0.0.0/255.255.255.0, 2001:db8::/32), like the input
        // validation of the tools
        if (target.Contains('/'))
            return RegexHelper.IPv4AddressCidrRegex().IsMatch(target) ||
                   RegexHelper.IPv4AddressSubnetmaskRegex().IsMatch(target) ||
                   Regex.IsMatch(target, RegexHelper.IPv6AddressCidrRegex)
                ? RunCommandTargetKind.Subnet
                : RunCommandTargetKind.None;

        // IPv4 range (e.g. 10.0.0.1-10.0.0.100, 10.0.0.1-100, 10.0.[0-1].1). The special range also matches an IPv4
        // address without a range, so only with "-" or "[".
        if ((target.Contains('-') || target.Contains('[')) &&
            (RegexHelper.IPv4AddressRangeRegex().IsMatch(target) ||
             RegexHelper.IPv4AddressShortRangeRegex().IsMatch(target) ||
             RegexHelper.IPv4AddressSpecialRangeRegex().IsMatch(target)))
            return RunCommandTargetKind.IPRange;

        // IP address: IPv4 with all 4 parts (a shortened or partly typed form like "10.1" is not a target) or IPv6
        if (IPAddress.TryParse(target, out _) && (target.Count(x => x == '.') == 3 || target.Contains(':')))
            return RunCommandTargetKind.Host;

        // Host name with or without a domain (a shortened or partly typed IPv4 address like "192.168.1" is not a host
        // name, since the last label must contain a letter)
        if (!RegexHelper.HostnameOrDomainRegex().IsMatch(target))
            return RunCommandTargetKind.None;

        return target.TrimEnd('.').Contains('.') ? RunCommandTargetKind.Host : RunCommandTargetKind.Hostname;
    }
}
