using NETworkManager.Helpers;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
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
                    return new ValidationResult(false, Application.Current.Resources["String_ValidationError_EnterOneOrMoreValidIPAddresses"] as string);
            }
            return ValidationResult.ValidResult;
        }
    }
}
