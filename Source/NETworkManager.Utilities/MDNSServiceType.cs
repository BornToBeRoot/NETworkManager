using System;
using System.Text.RegularExpressions;

namespace NETworkManager.Utilities;

/// <summary>Service types accepted by the multicast DNS-SD browser (local-link discovery only).</summary>
public static class MDNSServiceType
{
    public const string Enumeration = "_services._dns-sd._udp.local.";

    /// <summary>Normalizes a service type, or * for automatic service type enumeration.</summary>
    public static bool TryNormalize(string input, out string serviceType)
    {
        serviceType = null;
        var value = input == null ? null : DNSNameComparer.ToLower(input.Trim().TrimEnd('.'));
        if (value == "*")
        {
            serviceType = value;
            return true;
        }

        if (value == null || value.Length > 90)
            return false;

        // RFC 6335 service name: 1–15 letters/digits/hyphens, at least one letter, no consecutive hyphens.
        if (!Regex.IsMatch(value, @"^_[a-z0-9](?:[a-z0-9-]{0,13}[a-z0-9])?\._(?:tcp|udp)(?:\.local)?$",
                RegexOptions.CultureInvariant) || value.Split('.')[0].Contains("--") ||
            !Regex.IsMatch(value.Split('.')[0], "[a-z]", RegexOptions.CultureInvariant))
            return false;

        serviceType = value.EndsWith(".local", StringComparison.Ordinal) ? value + "." : value + ".local.";
        return true;
    }
}
