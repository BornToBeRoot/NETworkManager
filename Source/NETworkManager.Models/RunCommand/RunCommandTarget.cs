using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace NETworkManager.Models.RunCommand;

/// <summary>
///     Detects the kind of a target that is typed without a command, so the commands that accept it can be suggested
///     (e.g. "10.0.0.1" → ping, traceroute, ...). A single word without a domain is
///     <see cref="RunCommandTargetKind.Hostname" />, since it can also be a mistyped command.
/// </summary>
public static partial class RunCommandTarget
{
    [GeneratedRegex("^https?://", RegexOptions.IgnoreCase)]
    private static partial Regex UrlRegex();

    [GeneratedRegex("^(?:[0-9a-f]{2}[:-]){5}[0-9a-f]{2}$|^[0-9a-f]{12}$|^(?:[0-9a-f]{4}\\.){2}[0-9a-f]{4}$",
        RegexOptions.IgnoreCase)]
    private static partial Regex MACAddressRegex();

    [GeneratedRegex("^(?:[0-9a-f]{2}[:-]){2}[0-9a-f]{2}$", RegexOptions.IgnoreCase)]
    private static partial Regex OUIRegex();

    [GeneratedRegex("^\\d{1,5}(?:-\\d{1,5})?(?:/(?:tcp|udp|sctp))?(?:;\\d{1,5}(?:-\\d{1,5})?(?:/(?:tcp|udp|sctp))?)*$",
        RegexOptions.IgnoreCase)]
    private static partial Regex PortsRegex();

    [GeneratedRegex("^as\\d{1,10}$", RegexOptions.IgnoreCase)]
    private static partial Regex ASNRegex();

    [GeneratedRegex("^[0-9.\\-\\[\\],]+$")]
    private static partial Regex IPv4RangeRegex();

    // The last label (top-level domain) has at least one letter, so a shortened or partly typed IPv4 address (e.g.
    // "192.168.1") is not a host name.
    [GeneratedRegex(
        "^(?=.{1,253}\\.?$)(?:[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?\\.)+" +
        "(?=[a-z0-9-]*[a-z])[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?\\.?$",
        RegexOptions.IgnoreCase)]
    private static partial Regex HostnameWithDomainRegex();

    // A label with at least one letter (only digits is not a host name)
    [GeneratedRegex("^(?=[a-z0-9-]*[a-z])[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?$", RegexOptions.IgnoreCase)]
    private static partial Regex HostnameRegex();

    /// <summary>
    ///     Gets the kind of the target.
    /// </summary>
    /// <param name="target">Target without spaces (e.g. "10.0.0.1" or "10.0.0.1;10.0.0.2").</param>
    /// <returns>Kind of the target, <see cref="RunCommandTargetKind.None" /> if it is not clearly a target.</returns>
    public static RunCommandTargetKind GetKind(string target)
    {
        if (string.IsNullOrEmpty(target) || target.Any(char.IsWhiteSpace))
            return RunCommandTargetKind.None;

        if (UrlRegex().IsMatch(target))
            return Uri.TryCreate(target, UriKind.Absolute, out _) ? RunCommandTargetKind.URL : RunCommandTargetKind.None;

        if (MACAddressRegex().IsMatch(target))
            return RunCommandTargetKind.MACAddress;

        if (OUIRegex().IsMatch(target))
            return RunCommandTargetKind.OUI;

        if (PortsRegex().IsMatch(target))
            return RunCommandTargetKind.Ports;

        if (ASNRegex().IsMatch(target))
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

        // Subnet with CIDR or subnet mask (e.g. 10.0.0.0/24, 10.0.0.0/255.255.255.0, 2001:db8::/32)
        var slash = target.IndexOf('/');

        if (slash > 0)
            return IPAddress.TryParse(target[..slash], out _) &&
                   (int.TryParse(target[(slash + 1)..], out _) || IPAddress.TryParse(target[(slash + 1)..], out _))
                ? RunCommandTargetKind.Subnet
                : RunCommandTargetKind.None;

        // IPv4 range (e.g. 10.0.0.1-10.0.0.100, 10.0.0.1-100, 10.0.[0-1].1)
        if ((target.Contains('-') || target.Contains('[')) && target.Contains('.') && IPv4RangeRegex().IsMatch(target))
            return RunCommandTargetKind.IPRange;

        // IP address: IPv4 with all 4 parts (a shortened or partly typed form like "10.1" is not a target) or IPv6
        if (IPAddress.TryParse(target, out _) && (target.Count(x => x == '.') == 3 || target.Contains(':')))
            return RunCommandTargetKind.Host;

        if (HostnameWithDomainRegex().IsMatch(target))
            return RunCommandTargetKind.Host;

        return HostnameRegex().IsMatch(target) ? RunCommandTargetKind.Hostname : RunCommandTargetKind.None;
    }
}
