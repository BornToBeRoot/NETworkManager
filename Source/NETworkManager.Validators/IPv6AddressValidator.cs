using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Utilities;

namespace NETworkManager.Validators;

public class IPv6AddressValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var ipAddress = (value as string)?.Trim();

        if (string.IsNullOrEmpty(ipAddress))
            return new ValidationResult(false, Strings.EnterValidIPv6Address);

        return Regex.IsMatch(ipAddress, RegexHelper.IPv6AddressRegex)
            ? ValidationResult.ValidResult
            : new ValidationResult(false, Strings.EnterValidIPv6Address);
    }
}