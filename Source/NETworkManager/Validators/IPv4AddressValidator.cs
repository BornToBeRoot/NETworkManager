using NETworkManager.Utilities;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace NETworkManager.Validators
{
    // ReSharper disable once InconsistentNaming
    public class IPv4AddressValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return value != null && Regex.IsMatch((string) value, RegexHelper.IPv4AddressRegex) ? ValidationResult.ValidResult : new ValidationResult(false, Resources.Localization.Strings.EnterValidIPv4Address);
        }
    }
}
