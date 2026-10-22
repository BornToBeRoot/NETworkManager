using System.Text.RegularExpressions;

namespace NETworkManager.Utilities;

public static partial class RegexHelper
{
    /// <summary>
    /// Represents a regular expression pattern that matches valid IPv4 address values.
    /// </summary>    
    private const string IPv4AddressValues =
        @"((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)";

    /// <summary>
    /// Represents a regular expression pattern that matches valid IPv4 subnet mask values.
    /// </summary>    
    private const string SubnetmaskValues =
        @"(((255\.){3}(255|254|252|248|240|224|192|128|0+))|((255\.){2}(255|254|252|248|240|224|192|128|0+)\.0)|((255\.)(255|254|252|248|240|224|192|128|0+)(\.0+){2})|((255|254|252|248|240|224|192|128|0+)(\.0+){3}))";

    /// <summary>
    /// Represents the regular expression pattern used to validate CIDR notation values for IPv4 subnet masks.
    /// </summary>    
    private const string CidrRegexValues = @"([1-9]|[1-2][0-9]|3[0-2])";

    /// <summary>
    /// Represents a regular expression pattern that matches a label of a hostname (1-63 characters, letters, digits and
    /// hyphens, not starting or ending with a hyphen).
    /// </summary>
    private const string HostnameLabelValues = @"[A-Za-z0-9](?:[A-Za-z0-9-]{0,61}[A-Za-z0-9])?";

    /// <summary>
    /// Represents a regular expression pattern that matches valid hostnames or fully qualified domain names (FQDNs).
    /// Every label follows <see cref="HostnameLabelValues" />. The last label (or the hostname) must contain a letter,
    /// so IPv4 addresses, shortened or partly typed ones (e.g. "10.0.0.", "192.168.1") are not hostnames.
    /// </summary>
    private const string HostnameOrDomainValues =
        $@"(?=.{{1,255}}$)(?:{HostnameLabelValues}\.)*(?=[A-Za-z0-9-]*[A-Za-z]){HostnameLabelValues}\.?";

    /// <summary>
    /// Provides a compiled regular expression that matches valid IPv4 addresses in dot-decimal notation like "192.168.178.0".
    /// </summary>    
    /// <returns>A <see cref="Regex"/> instance that matches IPv4 addresses in the format "x.x.x.x", where each x is a number
    /// from 0 to 255.</returns>
    [GeneratedRegex($"^{IPv4AddressValues}$")]
    public static partial Regex IPv4AddressRegex();

    /// <summary>
    /// Provides a compiled regular expression that matches valid IPv4 addresses within input text like "192.168.178.0".
    /// </summary>    
    /// <returns>A <see cref="Regex"/> instance that can be used to extract IPv4 addresses from strings.</returns>
    [GeneratedRegex(IPv4AddressValues)]
    public static partial Regex IPv4AddressExtractRegex();

    /// <summary>
    /// Represents a regular expression pattern that matches valid shorthand IPv4 address ranges like
    /// "192.168.178.1-100" (base IP + last octet range).
    /// </summary>
    private const string IPv4AddressShortRangeValues =
        @"((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\-(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)";

    /// <summary>
    /// Provides a compiled regular expression that matches IPv4 address ranges in the format "start-end" like
    /// "192.168.178.0-192.168.178.255".
    /// </summary>
    /// <returns>A <see cref="Regex"/> instance that matches strings representing IPv4 address ranges, such as
    /// "192.168.1.1-192.168.1.100".</returns>
    [GeneratedRegex($"^{IPv4AddressValues}-{IPv4AddressValues}$")]
    public static partial Regex IPv4AddressRangeRegex();

    /// <summary>
    /// Provides a compiled regular expression that matches shorthand IPv4 address ranges like
    /// "192.168.178.1-100" (base IP followed by a last-octet range).
    /// </summary>
    /// <returns>A <see cref="Regex"/> instance that matches strings representing shorthand IPv4 address ranges,
    /// such as "192.168.1.1-100" (192.168.1.1 to 192.168.1.100).</returns>
    [GeneratedRegex($"^{IPv4AddressShortRangeValues}$")]
    public static partial Regex IPv4AddressShortRangeRegex();

    /// <summary>
    /// Provides a compiled regular expression that matches valid IPv4 subnet mask like "255.255.0.0".
    /// </summary>    
    /// <returns>A <see cref="Regex"/> instance that matches strings representing valid IPv4 subnet masks.</returns>        
    [GeneratedRegex($"^{SubnetmaskValues}$")]
    public static partial Regex SubnetmaskRegex();

    /// <summary>
    /// Provides a compiled regular expression that matches IPv4 addresses with subnet masks like
    /// "192.168.178.0/255.255.255.0".
    /// </summary>    
    /// <returns>A <see cref="Regex"/> instance that matches strings containing an IPv4 address followed by a subnet mask,
    /// separated by a forward slash.</returns>    
    [GeneratedRegex($@"^{IPv4AddressValues}\/{SubnetmaskValues}$")]
    public static partial Regex IPv4AddressSubnetmaskRegex();

    /// <summary>
    /// Provides a compiled regular expression that matches an IPv4 address with CIDR like
    /// "192.168.178.0/24".
    /// </summary>
    /// <returns>A <see cref="Regex"/> instance that matches strings containing an IPv4 address followed by a slash and a valid
    /// CIDR prefix length.</returns>
    [GeneratedRegex($@"^{IPv4AddressValues}\/{CidrRegexValues}$")]
    public static partial Regex IPv4AddressCidrRegex();

    /// <summary>
    /// Provides a compiled regular expression that matches IPv4 addresses, allowing for a special range in one or more octets like
    /// "192.168.[0-50].1".
    /// </summary>    
    /// <returns>A <see cref="Regex"/> instance that matches IPv4 addresses with support for custom special ranges like
    /// "192.168.[0-50].1".</returns>
    [GeneratedRegex($@"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|{SpecialRangeRegex})\.){{3}}((?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)|{SpecialRangeRegex})$")]
    public static partial Regex IPv4AddressSpecialRangeRegex();

    /// <summary>
    /// Provides a compiled regular expression that matches valid hostnames or fully qualified domain names (FQDNs) like
    /// server-01 or server-01.example.com.
    /// </summary>
    /// <returns> A <see cref="Regex"/> instance that matches valid hostnames or FQDNs.</returns>
    [GeneratedRegex($@"^{HostnameOrDomainValues}$")]
    public static partial Regex HostnameOrDomainRegex();

    /// <summary>
    /// Provides a compiled regular expression that matches a DNS suffix, with an optional leading dot, like
    /// "example.com" or ".example.com".
    /// </summary>
    /// <returns>A <see cref="Regex"/> instance that matches valid DNS suffixes.</returns>
    [GeneratedRegex($@"^\.?{HostnameOrDomainValues}$")]
    public static partial Regex DNSSuffixRegex();

    /// <summary>
    /// Creates a regular expression that matches a local directory path or one using environment variables,
    /// like "C:\Temp", "C:\My Settings", "%AppData%\settings".
    /// </summary>    
    /// <returns>A <see cref="Regex"/> instance that matches valid local directory paths.</returns>
    [GeneratedRegex($@"^(?:%[^%]+%|[a-zA-Z]\:)(\\[a-zA-Z0-9_\-\s\.]+)+\\?$")]
    public static partial Regex DirectoryPathWithEnvironmentVariablesRegex();

    /// <summary>
    /// Creates a regular expression that matches a UNC path like "\\server\share", "\\server\c$\settings".
    /// The share name may end with $ for hidden shares.
    /// </summary>
    /// <returns>A <see cref="Regex"/> instance that matches valid UNC paths.</returns>
    [GeneratedRegex($@"^\\\\[a-zA-Z0-9_\-\.]+(\\[a-zA-Z0-9_\-\s\.]+\$?)(\\[a-zA-Z0-9_\-\s\.]+)*\\?$")]
    public static partial Regex UncPathRegex();

    // Match a MAC-Address 000000000000, 00:00:00:00:00:00, 00-00-00-00-00-00 or 0000.0000.0000
    public const string MACAddressRegex =
        @"^[A-Fa-f0-9]{12}$|^[A-Fa-f0-9]{2}([:-][A-Fa-f0-9]{2}){5}$|^[A-Fa-f0-9]{4}\.[A-Fa-f0-9]{4}\.[A-Fa-f0-9]{4}$";

    // Match the first 3 bytes of a MAC-Address 000000, 00:00:00, 00-00-00 or 0000.00
    public const string MACAddressFirst3BytesRegex =
        @"^[A-Fa-f0-9]{6}$|^[A-Fa-f0-9]{2}[:-][A-Fa-f0-9]{2}[:-][A-Fa-f0-9]{2}$|^[A-Fa-f0-9]{4}\.[A-Fa-f0-9]{2}$";

    // Match IPv6 address like ::1
    // ReSharper disable once InconsistentNaming
    public const string IPv6AddressRegex =
        @"(?:^|(?<=\s))(([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,7}:|([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}|([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}|([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}|([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}|[0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6})|:((:[0-9a-fA-F]{1,4}){1,7}|:)|fe80:(:[0-9a-fA-F]{0,4}){0,4}%[0-9a-zA-Z]{1,}|::(ffff(:0{1,4}){0,1}:){0,1}((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])|([0-9a-fA-F]{1,4}:){1,4}:((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9]))(?=\s|$)";

    // Match a subnet like 2001:0db8::/64 (also with an embedded IPv4 address like ::ffff:192.168.0.1/96). Without
    // whitespace and zone index (e.g. "%eth0"), which a subnet can't be parsed with.
    // ReSharper disable once InconsistentNaming
    public const string IPv6AddressCidrRegex =
        @"^((([0-9A-Fa-f]{1,4}:){7}([0-9A-Fa-f]{1,4}|:))|(([0-9A-Fa-f]{1,4}:){6}(:[0-9A-Fa-f]{1,4}|((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3})|:))|(([0-9A-Fa-f]{1,4}:){5}(((:[0-9A-Fa-f]{1,4}){1,2})|:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3})|:))|(([0-9A-Fa-f]{1,4}:){4}(((:[0-9A-Fa-f]{1,4}){1,3})|((:[0-9A-Fa-f]{1,4})?:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){3}(((:[0-9A-Fa-f]{1,4}){1,4})|((:[0-9A-Fa-f]{1,4}){0,2}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){2}(((:[0-9A-Fa-f]{1,4}){1,5})|((:[0-9A-Fa-f]{1,4}){0,3}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){1}(((:[0-9A-Fa-f]{1,4}){1,6})|((:[0-9A-Fa-f]{1,4}){0,4}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(:(((:[0-9A-Fa-f]{1,4}){1,7})|((:[0-9A-Fa-f]{1,4}){0,5}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:)))(\/([0-9]|[1-9][0-9]|1[0-1][0-9]|12[0-8])){1}?$";

    // Match a range like [0-255], [0,2,4] and [2,4-6]
    public const string SpecialRangeRegex =
        @"\[((?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)|((?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)-(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)))([,]((?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)|((?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)-(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?))))*\]";

    // Match a hostname with cidr like server-01.example.com/24
    public const string HostnameOrDomainWithCidrRegex = $@"^{HostnameOrDomainValues}\/{CidrRegexValues}$";

    // Match a hostname with subnetmask like server-01.example.com/255.255.255.0
    public const string HostnameOrDomainWithSubnetmaskRegex = $@"^{HostnameOrDomainValues}\/{SubnetmaskValues}$";

    // Match a domain local.example.com: labels like a hostname (see HostnameLabelValues, no hyphen at the start or end)
    // and a top-level domain of letters. Without nested quantifiers, which backtrack exponentially for an invalid input
    // with many labels.
    public const string DomainRegex =
        $@"^(?=.{{1,255}}$)(?:{HostnameLabelValues}\.)+[a-zA-Z]{{2,63}}\.?$";

    // Private port values (1-65535, without leading zeros)
    private const string PortValues =
        @"(6553[0-5]|655[0-2][0-9]|65[0-4][0-9]{2}|6[0-4][0-9]{3}|[1-5][0-9]{4}|[1-9][0-9]{0,3})";

    // Match a port between 1-65535
    public const string PortRegex = $@"^{PortValues}$";

    /// <summary>
    /// Represents a regular expression pattern that matches a port number of the port lookup (1-5 digits, the range is
    /// checked separately).
    /// </summary>
    private const string PortLookupPortValues = @"[0-9]{1,5}";

    /// <summary>
    /// Represents a regular expression pattern that matches a transport protocol of the port lookup.
    /// </summary>
    private const string PortLookupProtocolValues = @"(tcp|udp|sctp)";

    /// <summary>
    /// Represents a regular expression pattern that matches an entry of the port lookup: a port or port range, optional
    /// with a protocol, like "22", "22/tcp", "80-90" or "80-90/tcp".
    /// </summary>
    private const string PortLookupEntryValues =
        $@"{PortLookupPortValues}(-{PortLookupPortValues})?(/{PortLookupProtocolValues})?";

    /// <summary>
    /// Provides a compiled regular expression that matches a port of the port lookup like "22".
    /// </summary>
    /// <returns>A <see cref="Regex"/> instance that matches a port.</returns>
    [GeneratedRegex($"^{PortLookupPortValues}$")]
    public static partial Regex PortLookupPortRegex();

    /// <summary>
    /// Provides a compiled regular expression that matches a port with a protocol of the port lookup like "22/tcp".
    /// </summary>
    /// <returns>A <see cref="Regex"/> instance that matches a port with a protocol.</returns>
    [GeneratedRegex($"^{PortLookupPortValues}/{PortLookupProtocolValues}$", RegexOptions.IgnoreCase)]
    public static partial Regex PortLookupPortAndProtocolRegex();

    /// <summary>
    /// Provides a compiled regular expression that matches a port range of the port lookup like "80-90".
    /// </summary>
    /// <returns>A <see cref="Regex"/> instance that matches a port range.</returns>
    [GeneratedRegex($"^{PortLookupPortValues}-{PortLookupPortValues}$")]
    public static partial Regex PortLookupPortRangeRegex();

    /// <summary>
    /// Provides a compiled regular expression that matches a port range with a protocol of the port lookup like
    /// "80-90/tcp".
    /// </summary>
    /// <returns>A <see cref="Regex"/> instance that matches a port range with a protocol.</returns>
    [GeneratedRegex($"^{PortLookupPortValues}-{PortLookupPortValues}/{PortLookupProtocolValues}$",
        RegexOptions.IgnoreCase)]
    public static partial Regex PortLookupPortRangeAndProtocolRegex();

    /// <summary>
    /// Provides a compiled regular expression that matches entries of the port lookup separated by ";" like
    /// "22;80-90/tcp".
    /// </summary>
    /// <returns>A <see cref="Regex"/> instance that matches a list of ports, port ranges and protocols.</returns>
    [GeneratedRegex($"^{PortLookupEntryValues}(;{PortLookupEntryValues})*$", RegexOptions.IgnoreCase)]
    public static partial Regex PortLookupEntriesRegex();

    /// <summary>
    /// Provides a compiled regular expression that matches the first 3 bytes of a MAC address with separators like
    /// "00:11:22" or "00-11-22". Stricter than <see cref="MACAddressFirst3BytesRegex"/>, since 6 hex digits without
    /// separators (e.g. "facade") are more likely a word.
    /// </summary>
    /// <returns>A <see cref="Regex"/> instance that matches the first 3 bytes of a MAC address with separators.</returns>
    [GeneratedRegex("^[A-Fa-f0-9]{2}[:-][A-Fa-f0-9]{2}[:-][A-Fa-f0-9]{2}$")]
    public static partial Regex MACAddressFirst3BytesWithSeparatorRegex();

    /// <summary>
    /// Provides a compiled regular expression that matches an AS number with the prefix "AS" like "AS13335".
    /// </summary>
    /// <returns>A <see cref="Regex"/> instance that matches an AS number with the prefix.</returns>
    [GeneratedRegex("^AS[0-9]{1,10}$", RegexOptions.IgnoreCase)]
    public static partial Regex ASNumberWithPrefixRegex();

    /// <summary>
    /// Provides a compiled regular expression that matches the scheme of an HTTP or HTTPS URL like "https://".
    /// </summary>
    /// <returns>A <see cref="Regex"/> instance that matches the start of an HTTP or HTTPS URL.</returns>
    [GeneratedRegex("^https?://", RegexOptions.IgnoreCase)]
    public static partial Regex HttpOrHttpsSchemeRegex();

    // Match any fullname (like "c:\temp\test.txt") --> https://www.codeproject.com/Tips/216238/Regular-Expression-to-Validate-File-Path-and-Exten
    public const string FullNameRegex =
        @"^(?:[\w]\:|\\)(\\[a-zA-Z0-9_\-\s\.()~!@#$%^&=+';,{}\[\]]+)+\.[a-zA-z0-9]{1,4}$";

    // Match a string that doesn't end with ";" (like "80; ldap; ssh; 443")
    public const string StringNotEndWithSemicolonRegex = @"^.*[^;]$";

    // Match a number (like 12, 12.4, 12,3) 
    public const string NumberRegex = @"^\d+((\.|,)\d+)?$";

    // Match an SNMP OID (like 1.3.6.1 or .1.3.6.2)
    public const string SnmpOidRegex = @"^\.?[012]\.(?:[0-9]|[1-3][0-9])(\.\d+)*$";

    /// <summary>
    /// Provides a compiled regular expression that matches a Windows username in SAM format (DOMAIN\user or .\user),
    /// UPN format (user@domain.com), or a simple username (user).
    /// </summary>
    /// <returns>A <see cref="Regex"/> instance that matches Windows usernames.</returns>
    [GeneratedRegex(@"^([\w.-]+\\)?[\w.-]+(@[\w.-]+)?$")]
    public static partial Regex WindowsUsernameRegex();

    /// <summary>
    /// Provides a compiled regular expression that matches a valid LDAP distinguished name like "DC=example,DC=com".
    /// Each component must have the form attr=value; deep syntax is left to the directory server.
    /// </summary>
    /// <returns>A <see cref="Regex"/> instance that matches LDAP distinguished names.</returns>
    [GeneratedRegex(@"^[A-Za-z][A-Za-z0-9-]*=[^,]+(,\s*[A-Za-z][A-Za-z0-9-]*=[^,]+)*$")]
    public static partial Regex LdapSearchBaseRegex();

    // Match a hosts file entry with optional comments, supporting IPv4, IPv6, and hostnames
    // ^*                                    : Matches the beginning of the line
    // (#)?                                  : Optionally matches a comment (#) at the start of the line
    // \s*                                   : Matches any whitespace after the comment (or before the IP)
    // ((?:(?:\d{1,3}\.){3}\d{1,3})          : Matches an IPv4 address (e.g., 192.168.1.1)
    // |                                     : OR (alternation between IPv4 and IPv6)
    // (?:(?:[A-Fa-f0-9:]+:+)+[A-Fa-f0-9]+)  : Matches an IPv6 address (e.g., 2001:db8::1)
    // \s+                                   : Matches one or more spaces between the IP and the hostnames
    // ([\w.-]+(?:\s+[\w.-]+)*)              : Matches one or more hostnames, separated by spaces
    // \s*                                   : Matches optional whitespace after hostnames
    // (#.*)?                                : Optionally matches a comment after hostnames
    // $                                     : Anchors the match to the end of the line
    public static string HostsEntryRegex =>
        @"^(#)?\s*((?:(?:\d{1,3}\.){3}\d{1,3})|(?:(?:[A-Fa-f0-9:]+:+)+[A-Fa-f0-9]+))\s+([\w.-]+(?:\s+[\w.-]+)*)\s*(#.*)?$";

}