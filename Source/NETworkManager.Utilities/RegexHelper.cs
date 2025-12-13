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
    /// Represents a regular expression pattern that matches valid hostnames or fully qualified domain names (FQDNs).
    /// </summary>
    private const string HostnameOrDomainValues =
        @"(?=.{1,255}$)(?!-)[A-Za-z0-9-]{1,63}(?<!-)(\.[A-Za-z0-9-]{1,63})*\.?";

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
    /// Provides a compiles regular expression that matches IPv4 address ranges in the format "start-end" like
    /// "192.168.178.0-192.168.178.255".
    /// </summary>    
    /// <returns>A <see cref="Regex"/> instance that matches strings representing IPv4 address ranges, such as
    /// "192.168.1.1-192.168.1.100".</returns>
    [GeneratedRegex($"^{IPv4AddressValues}-{IPv4AddressValues}$")]
    public static partial Regex IPv4AddressRangeRegex();

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

    // Match a MAC-Address 000000000000 00:00:00:00:00:00, 00-00-00-00-00-00-00 or 0000.0000.0000
    public const string MACAddressRegex =
        @"^^[A-Fa-f0-9]{12}$|^[A-Fa-f0-9]{2}(:|-){1}[A-Fa-f0-9]{2}(:|-){1}[A-Fa-f0-9]{2}(:|-){1}[A-Fa-f0-9]{2}(:|-){1}[A-Fa-f0-9]{2}(:|-){1}[A-Fa-f0-9]{2}$|^[A-Fa-f0-9]{4}.[A-Fa-f0-9]{4}.[A-Fa-f0-9]{4}$$";

    // Match the first 3 bytes of a MAC-Address 000000, 00:00:00, 00-00-00
    public const string MACAddressFirst3BytesRegex =
        @"^[A-Fa-f0-9]{6}$|^[A-Fa-f0-9]{2}(:|-){1}[A-Fa-f0-9]{2}(:|-){1}[A-Fa-f0-9]{2}$|^[A-Fa-f0-9]{4}.[A-Fa-f0-9]{2}$";

    // Match IPv6 address like ::1
    // ReSharper disable once InconsistentNaming
    public const string IPv6AddressRegex =
        @"(?:^|(?<=\s))(([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,7}:|([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}|([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}|([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}|([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}|[0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6})|:((:[0-9a-fA-F]{1,4}){1,7}|:)|fe80:(:[0-9a-fA-F]{0,4}){0,4}%[0-9a-zA-Z]{1,}|::(ffff(:0{1,4}){0,1}:){0,1}((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])|([0-9a-fA-F]{1,4}:){1,4}:((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9]))(?=\s|$)";

    // Match a subnet like 2001:0db8::/64
    // ReSharper disable once InconsistentNaming
    public const string IPv6AddressCidrRegex =
        @"^s*((([0-9A-Fa-f]{1,4}:){7}([0-9A-Fa-f]{1,4}|:))|(([0-9A-Fa-f]{1,4}:){6}(:[0-9A-Fa-f]{1,4}|((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3})|:))|(([0-9A-Fa-f]{1,4}:){5}(((:[0-9A-Fa-f]{1,4}){1,2})|:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3})|:))|(([0-9A-Fa-f]{1,4}:){4}(((:[0-9A-Fa-f]{1,4}){1,3})|((:[0-9A-Fa-f]{1,4})?:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){3}(((:[0-9A-Fa-f]{1,4}){1,4})|((:[0-9A-Fa-f]{1,4}){0,2}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){2}(((:[0-9A-Fa-f]{1,4}){1,5})|((:[0-9A-Fa-f]{1,4}){0,3}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){1}(((:[0-9A-Fa-f]{1,4}){1,6})|((:[0-9A-Fa-f]{1,4}){0,4}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(:(((:[0-9A-Fa-f]{1,4}){1,7})|((:[0-9A-Fa-f]{1,4}){0,5}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:)))(%.+)?s*(\/([0-9]|[1-9][0-9]|1[0-1][0-9]|12[0-8])){1}?$";

    // Match a range like [0-255], [0,2,4] and [2,4-6]
    public const string SpecialRangeRegex =
        @"\[((?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)|((?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)-(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)))([,]((?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)|((?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)-(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?))))*\]";



    /// <summary>
    /// Provides a compiled regular expression that matches valid hostnames or fully qualified domain names (FQDNs) like
    /// server-01 or server-01.example.com.
    /// </summary>
    /// <returns> A <see cref="Regex"/> instance that matches valid hostnames or FQDNs.</returns>
    [GeneratedRegex($@"^{HostnameOrDomainValues}$")]
    public static partial Regex HostnameOrDomainRegex();

    // Match a hostname with cidr like server-01.example.com/24
    public const string HostnameOrDomainWithCidrRegex = $@"^{HostnameOrDomainValues}\/{CidrRegexValues}$";

    // Match a hostname with subnetmask like server-01.example.com/255.255.255.0
    public const string HostnameOrDomainWithSubnetmaskRegex = $@"^{HostnameOrDomainValues}\/{SubnetmaskValues}$";

    // Match a domain local.example.com
    public const string DomainRegex =
        @"^(?=.{1,255}$)[a-zA-Z0-9\-_]{1,63}((\.[a-zA-Z0-9\-_]+){1,63})*\.[a-zA-Z]{2,}\.?$";

    // Private port values
    private const string PortValues =
        @"((6553[0-5])|(655[0-2][0-9])|(65[0-4][0-9]{2})|(6[0-4][0-9]{3})|([1-5][0-9]{4})|([0-5]{0,5})|([0-9]{1,4}))";

    // Match a port between 1-65535
    public const string PortRegex = $@"^{PortValues}$";

    // Match any filepath (like "c:\temp\") --> https://www.codeproject.com/Tips/216238/Regular-Expression-to-Validate-File-Path-and-Exten
    public const string FilePathRegex = @"^(?:[\w]\:|\\)(\\[a-z_\-\s0-9\.]+)+$";

    // Match any fullname (like "c:\temp\test.txt") --> https://www.codeproject.com/Tips/216238/Regular-Expression-to-Validate-File-Path-and-Exten
    public const string FullNameRegex =
        @"^(?:[\w]\:|\\)(\\[a-zA-Z0-9_\-\s\.()~!@#$%^&=+';,{}\[\]]+)+\.[a-zA-z0-9]{1,4}$";

    // Match a string that doesn't end with ";" (like "80; ldap; ssh; 443")
    public const string StringNotEndWithSemicolonRegex = @"^.*[^;]$";

    // Match a number (like 12, 12.4, 12,3) 
    public const string NumberRegex = @"^\d+((\.|,)\d+)?$";

    // Match an SNMP OID (like 1.3.6.1 or .1.3.6.2)
    public const string SnmpOidRegex = @"^\.?[012]\.(?:[0-9]|[1-3][0-9])(\.\d+)*$";

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