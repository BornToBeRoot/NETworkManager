using NETworkManager.Localization.Resources;
using NETworkManager.Utilities;
using System.Globalization;
using System.Windows.Controls;

namespace NETworkManager.Validators;

public class IPv4AddressValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var ipAddress = (value as string)?.Trim();

        if (string.IsNullOrEmpty(ipAddress))
            return new ValidationResult(false, Strings.EnterValidIPv4Address);

        return RegexHelper.IPv4AddressRegex().IsMatch(ipAddress)
            ? ValidationResult.ValidResult
            : new ValidationResult(false, Strings.EnterValidIPv4Address);
    }
}