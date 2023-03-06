using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Utilities;

namespace NETworkManager.Validators;

public class IPAddressOrHostnameValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var input = (value as string)?.Trim();

        if(string.IsNullOrEmpty(input))
            return new ValidationResult(false, Strings.EnterValidHostnameOrIPAddress);

        // Check if it is a valid IPv4 address like 192.168.0.1
        if (Regex.IsMatch(input, RegexHelper.IPv4AddressRegex))
            return ValidationResult.ValidResult;

        // Check if it is a valid IPv6 address like ::1
        if (Regex.IsMatch(input, RegexHelper.IPv6AddressRegex))
            return ValidationResult.ValidResult;

        // Check if it is a valid hostname like server-01 or server-01.example.com
        if (Regex.IsMatch(input, RegexHelper.HostnameRegex))
            return ValidationResult.ValidResult;

        return new ValidationResult(false, Strings.EnterValidHostnameOrIPAddress);
    }
}
