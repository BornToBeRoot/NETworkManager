using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace NETworkManager.Models.RDAP;

/// <summary>
///     Validates and normalizes user input into an <see cref="RDAPQuery" /> (RFC 9082).
/// </summary>
public static partial class RDAPQueryParser
{
    [GeneratedRegex(@"^\d{1,3}(\.\d{1,3}){3}$")]
    private static partial Regex IPv4AddressRegex();

    [GeneratedRegex(@"^(?:as)?\s*(\d+)(?:\.(\d+))?$", RegexOptions.IgnoreCase)]
    private static partial Regex ASNRegex();

    private static readonly IdnMapping IdnMapping = new() { UseStd3AsciiRules = true };

    /// <summary>
    ///     Parses the input for the given query type.
    /// </summary>
    /// <param name="type">Type of the query.</param>
    /// <param name="input">User input.</param>
    /// <param name="query">Normalized query if the input is valid.</param>
    /// <returns><see cref="RDAPQueryParseError.None" /> if the input is valid, otherwise the error.</returns>
    public static RDAPQueryParseError TryParse(RDAPQueryType type, string input, out RDAPQuery query)
    {
        query = null;

        input = input?.Trim();

        if (string.IsNullOrEmpty(input))
            return GetError(type);

        query = type switch
        {
            RDAPQueryType.Domain => ParseDomain(input),
            RDAPQueryType.TLD => ParseTLD(input),
            RDAPQueryType.IPAddress => ParseIPAddress(input),
            RDAPQueryType.ASN => ParseASN(input),
            RDAPQueryType.Entity => ParseEntity(input),
            _ => null
        };

        return query == null ? GetError(type) : RDAPQueryParseError.None;
    }

    /// <summary>
    ///     Detects the query type of an input without a type (e.g. data redirected from another tool): IP address or
    ///     CIDR, AS number ("AS15169", "15169", "1.10"), TLD (single label), entity handle (single label with a hyphen,
    ///     e.g. "ORG-RIEN1-RIPE"), otherwise domain.
    /// </summary>
    /// <param name="input">User input.</param>
    /// <returns>Detected query type.</returns>
    public static RDAPQueryType DetectType(string input)
    {
        input = input?.Trim() ?? string.Empty;

        if (ParseIPAddress(input) != null)
            return RDAPQueryType.IPAddress;

        // Digits only or with "AS" prefix. Asdot ("1.10") is also numeric, a TLD is never numeric.
        if (ASNRegex().IsMatch(input))
            return RDAPQueryType.ASN;

        var label = input.Trim('.');

        if (!label.Contains('.'))
            return label.Contains('-') && !label.StartsWith("xn--", StringComparison.OrdinalIgnoreCase)
                ? RDAPQueryType.Entity
                : RDAPQueryType.TLD;

        return RDAPQueryType.Domain;
    }

    private static RDAPQueryParseError GetError(RDAPQueryType type)
    {
        return type switch
        {
            RDAPQueryType.Domain => RDAPQueryParseError.InvalidDomain,
            RDAPQueryType.TLD => RDAPQueryParseError.InvalidTLD,
            RDAPQueryType.IPAddress => RDAPQueryParseError.InvalidIPAddress,
            RDAPQueryType.ASN => RDAPQueryParseError.InvalidASN,
            _ => RDAPQueryParseError.InvalidEntity
        };
    }

    /// <summary>
    ///     Converts a domain name (A-labels or U-labels) to its lowercase A-label form.
    ///     The whole name is converted, since RFC 9082 3.1.3 discourages mixing A-labels and U-labels.
    /// </summary>
    private static string ToALabel(string domain)
    {
        try
        {
            return IdnMapping.GetAscii(domain).ToLowerInvariant();
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    private static RDAPQuery ParseDomain(string input)
    {
        var domain = ToALabel(input.Trim('.'));

        if (string.IsNullOrEmpty(domain))
            return null;

        // A single label is a TLD, which is only served by the IANA root zone RDAP server.
        if (!domain.Contains('.'))
            return CreateTLDQuery(domain);

        return new RDAPQuery
        {
            Type = RDAPQueryType.Domain,
            Value = domain,
            DomainName = domain,
            PathSegment = $"domain/{domain}"
        };
    }

    private static RDAPQuery ParseTLD(string input)
    {
        // rdap.iana.org returns 400 for ".de", so the leading dot must be removed.
        var tld = ToALabel(input.Trim('.'));

        if (string.IsNullOrEmpty(tld) || tld.Contains('.'))
            return null;

        return CreateTLDQuery(tld);
    }

    private static RDAPQuery CreateTLDQuery(string tld)
    {
        return new RDAPQuery
        {
            Type = RDAPQueryType.TLD,
            Value = tld,
            DomainName = tld,
            PathSegment = $"domain/{tld}"
        };
    }

    private static RDAPQuery ParseIPAddress(string input)
    {
        var parts = input.Split('/');

        if (parts.Length > 2)
            return null;

        // Zone IDs (e.g. fe80::1%eth0) must not be sent in RDAP queries (RFC 9082 3.1.1). They are only meaningful on
        // the local host and irrelevant for the registration data, so they are removed instead of rejecting the input
        // (link-local addresses are often copied with a zone ID, e.g. from ipconfig).
        var addressPart = parts[0].Split('%')[0];

        // IPAddress.TryParse also accepts shortened IPv4 forms like "1" or "1.2", which are not valid here.
        if (!addressPart.Contains(':') && !IPv4AddressRegex().IsMatch(addressPart))
            return null;

        if (!IPAddress.TryParse(addressPart, out var ipAddress))
            return null;

        if (ipAddress.IsIPv4MappedToIPv6)
            ipAddress = ipAddress.MapToIPv4();

        int? prefixLength = null;

        if (parts.Length == 2)
        {
            var maxPrefixLength = ipAddress.AddressFamily == AddressFamily.InterNetwork ? 32 : 128;

            if (!int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var length) ||
                length > maxPrefixLength)
                return null;

            prefixLength = length;
            ipAddress = GetNetworkAddress(ipAddress, length);
        }

        var value = prefixLength == null ? ipAddress.ToString() : $"{ipAddress}/{prefixLength}";

        return new RDAPQuery
        {
            Type = RDAPQueryType.IPAddress,
            Value = value,
            IPAddress = ipAddress,
            PrefixLength = prefixLength,
            // CIDR is sent as two literal path segments, not as one URL-encoded token (RFC 9082 3.1.1).
            PathSegment = $"ip/{value}"
        };
    }

    /// <summary>
    ///     Clears all host bits of the address for the given prefix length.
    /// </summary>
    private static IPAddress GetNetworkAddress(IPAddress ipAddress, int prefixLength)
    {
        var bytes = ipAddress.GetAddressBytes();

        for (var i = 0; i < bytes.Length; i++)
        {
            var bits = Math.Clamp(prefixLength - i * 8, 0, 8);

            bytes[i] &= (byte)(0xFF << (8 - bits));
        }

        return new IPAddress(bytes);
    }

    private static RDAPQuery ParseASN(string input)
    {
        // RFC 9082 3.1.2 requires asplain. "AS" prefixes and asdot notation are converted.
        var match = ASNRegex().Match(input);

        if (!match.Success)
            return null;

        uint asn;

        if (match.Groups[2].Success)
        {
            if (!ushort.TryParse(match.Groups[1].Value, NumberStyles.None, CultureInfo.InvariantCulture,
                    out var high) ||
                !ushort.TryParse(match.Groups[2].Value, NumberStyles.None, CultureInfo.InvariantCulture,
                    out var low))
                return null;

            asn = (uint)high * 65536 + low;
        }
        else if (!uint.TryParse(match.Groups[1].Value, NumberStyles.None, CultureInfo.InvariantCulture, out asn))
        {
            return null;
        }

        var value = asn.ToString(CultureInfo.InvariantCulture);

        return new RDAPQuery
        {
            Type = RDAPQueryType.ASN,
            Value = value,
            ASN = asn,
            PathSegment = $"autnum/{value}"
        };
    }

    private static RDAPQuery ParseEntity(string input)
    {
        if (input.Any(char.IsWhiteSpace) || input.Contains('/'))
            return null;

        return new RDAPQuery
        {
            Type = RDAPQueryType.Entity,
            Value = input,
            Handle = input,
            PathSegment = $"entity/{Uri.EscapeDataString(input)}"
        };
    }
}
