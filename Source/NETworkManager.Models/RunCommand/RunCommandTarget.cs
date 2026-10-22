using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
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

        if (RegexHelper.PortLookupEntriesRegex().IsMatch(target))
            return RunCommandTargetKind.Ports;

        if (RegexHelper.ASNumberWithPrefixRegex().IsMatch(target))
            return RunCommandTargetKind.ASN;

        // Several hosts, IP ranges or subnets
        if (target.Contains(';'))
        {
            var parts = target.Split(';', StringSplitOptions.RemoveEmptyEntries);

            return parts.Length > 0 && parts.All(x => GetKind(x) is RunCommandTargetKind.Host
                       or RunCommandTargetKind.Hostname or RunCommandTargetKind.IPRange or RunCommandTargetKind.Subnet)
                ? RunCommandTargetKind.Hosts
                : RunCommandTargetKind.None;
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
