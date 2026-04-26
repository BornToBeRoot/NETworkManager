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
/// notation (e.g. <c>10.0.0.0/255.0.0.0</c>), or recognized Windows Firewall keywords
/// (e.g. LocalSubnet, Internet, Intranet, DNS, DHCP, WINS, DefaultGateway).
/// </summary>
public class EmptyOrFirewallAddressValidator : ValidationRule
{
    private static readonly string[] Keywords =
    [
        "Any", "LocalSubnet", "Internet", "Intranet", "DNS", "DHCP", "WINS", "DefaultGateway"
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

            if (Array.Exists(Keywords, k => k.Equals(token, StringComparison.OrdinalIgnoreCase)))
                continue;

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
}