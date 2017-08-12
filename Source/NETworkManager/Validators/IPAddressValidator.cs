using NETworkManager.Helpers;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace NETworkManager.Validators
{
    public class IPAddressValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (Regex.IsMatch(value as string, RegexHelper.IPv4AddressRegex) || Regex.IsMatch(value as string, RegexHelper.IPv6AddressRegex))
                return ValidationResult.ValidResult;

            return new ValidationResult(false, Application.Current.Resources["String_ValidateError_EnterValidIPAddress"] as string);
        }
    }
}
