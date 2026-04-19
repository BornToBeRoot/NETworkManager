using System;
using System.Globalization;
using System.Net;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;

namespace NETworkManager.Validators;

/// <summary>
/// Validates that the input is empty (meaning "Any") or contains semicolon-separated
/// valid IPv4/IPv6 addresses, CIDR subnets, or recognized Windows Firewall keywords
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

            if (!IPAddress.TryParse(addressPart, out _))
                return new ValidationResult(false, Strings.EnterValidFirewallAddress);
        }

        return ValidationResult.ValidResult;
    }
}