using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Utilities;

namespace NETworkManager.Validators;

/// <summary>
/// Validates that the input is empty (meaning "Any") or contains semicolon-separated
/// valid IPv4/IPv6 addresses, IPv4/IPv6 CIDR subnets, IPv4 subnets in subnet-mask
/// notation (e.g. <c>10.0.0.0/255.0.0.0</c>), IPv4/IPv6 ranges (e.g. <c>1.2.3.4-1.2.3.7</c>),
/// or recognized Windows Firewall keywords (Any, LocalSubnet, DNS, DHCP, WINS, DefaultGateway,
/// Internet, Intranet, IntranetRemoteAccess, PlayToDevice, CaptivePortal). Keywords may be
/// suffixed with <c>4</c> or <c>6</c> to restrict matching to IPv4 or IPv6 (e.g. <c>LocalSubnet4</c>).
/// </summary>
public class EmptyOrFirewallAddressValidator : ValidationRule
{
    private static readonly string[] Keywords =
    [
        "Any", "LocalSubnet", "DNS", "DHCP", "WINS", "DefaultGateway",
        "Internet", "Intranet", "IntranetRemoteAccess", "PlayToDevice", "CaptivePortal"
    ];

    /// <inheritdoc />
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (string.IsNullOrEmpty(value as string))
            return ValidationResult.ValidResult;

        foreach (var entry in ((string)value).Split(';'))
        {
            var token = entry.Trim();

            if (string.IsNullOrEmpty(token))
                continue;

            if (IsKeyword(token))
                continue;

            if (!token.Contains('/') && token.Contains('-'))
            {
                if (IsValidRange(token))
                    continue;

                return new ValidationResult(false, Strings.EnterValidFirewallAddress);
            }

            var slashIndex = token.IndexOf('/');
            var addressPart = slashIndex > 0 ? token[..slashIndex] : token;

            if (!IPAddress.TryParse(addressPart, out var ip))
                return new ValidationResult(false, Strings.EnterValidFirewallAddress);

            if (slashIndex <= 0)
                continue;

            var suffix = token[(slashIndex + 1)..];

            if (ip.AddressFamily == AddressFamily.InterNetwork && RegexHelper.SubnetmaskRegex().IsMatch(suffix))
                continue;

            var maxPrefix = ip.AddressFamily == AddressFamily.InterNetworkV6 ? 128 : 32;

            if (!int.TryParse(suffix, out var prefix) || prefix < 0 || prefix > maxPrefix)
                return new ValidationResult(false, Strings.EnterValidFirewallAddress);
        }

        return ValidationResult.ValidResult;
    }

    private static bool IsKeyword(string token)
    {
        if (Array.Exists(Keywords, k => k.Equals(token, StringComparison.OrdinalIgnoreCase)))
            return true;

        if (token.Length < 2 || (token[^1] != '4' && token[^1] != '6'))
            return false;

        var bare = token[..^1];
        return Array.Exists(Keywords, k => k.Equals(bare, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsValidRange(string token)
    {
        var dashIndex = token.IndexOf('-');

        if (dashIndex <= 0 || dashIndex >= token.Length - 1)
            return false;

        if (!IPAddress.TryParse(token[..dashIndex], out var start) ||
            !IPAddress.TryParse(token[(dashIndex + 1)..], out var end))
            return false;

        return start.AddressFamily == end.AddressFamily;
    }
}