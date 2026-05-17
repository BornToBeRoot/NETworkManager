using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Utilities;

namespace NETworkManager.Validators;

public class EmptyOrIPAddressOrHostnameValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var input = (value as string)?.Trim();

        if (string.IsNullOrEmpty(input))
            return ValidationResult.ValidResult;

        if (RegexHelper.IPv4AddressRegex().IsMatch(input))
            return ValidationResult.ValidResult;

        if (Regex.IsMatch(input, RegexHelper.IPv6AddressRegex))
            return ValidationResult.ValidResult;

        if (RegexHelper.HostnameOrDomainRegex().IsMatch(input))
            return ValidationResult.ValidResult;

        return new ValidationResult(false, Strings.EnterValidHostnameOrIPAddress);
    }
}