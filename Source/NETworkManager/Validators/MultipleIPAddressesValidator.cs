using NETworkManager.Models.Settings;
using NETworkManager.Utilities;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace NETworkManager.Validators
{
    public class MultipleIPAddressesValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            foreach (string ipAddress in (value as string).Split(';'))
            {
                if (!Regex.IsMatch(ipAddress.Trim(), RegexHelper.IPv4AddressRegex) && !Regex.IsMatch(ipAddress.Trim(), RegexHelper.IPv6AddressRegex))
                    return new ValidationResult(false, LocalizationManager.GetStringByKey("String_ValidationError_EnterOneOrMoreValidIPAddresses"));
            }
            return ValidationResult.ValidResult;
        }
    }
}
