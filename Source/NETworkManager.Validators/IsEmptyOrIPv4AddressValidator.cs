using NETworkManager.Utilities;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace NETworkManager.Validators
{
    public class IsEmptyOrIPv4AddressValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (string.IsNullOrEmpty(value as string))
                return ValidationResult.ValidResult;

            return Regex.IsMatch((string)value, RegexHelper.IPv4AddressRegex) ? ValidationResult.ValidResult : new ValidationResult(false, Localization.Resources.Strings.EnterValidIPv4Address);
        }
    }
}
