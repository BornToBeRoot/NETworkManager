using NETworkManager.Localization.Resources;
using NETworkManager.Utilities;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace NETworkManager.Validators;

public class MultipleIPAddressesValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (value == null)
            return ValidationResult.ValidResult;

        for (var index = 0; index < ((string)value).Split(';').Length; index++)
        {
            var ipAddress = ((string)value).Split(';')[index].Trim();

            if (!RegexHelper.IPv4AddressRegex().IsMatch(ipAddress) &&
                !Regex.IsMatch(ipAddress.Trim(), RegexHelper.IPv6AddressRegex))
                return new ValidationResult(false, Strings.EnterOneOrMoreValidIPAddresses);
        }

        return ValidationResult.ValidResult;
    }
}
