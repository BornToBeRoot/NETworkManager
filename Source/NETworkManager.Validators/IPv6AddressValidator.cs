using NETworkManager.Utilities;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace NETworkManager.Validators
{    
    public class IPv6AddressValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return value != null && Regex.IsMatch((string) value, RegexHelper.IPv6AddressRegex) ? ValidationResult.ValidResult : new ValidationResult(false, Localization.Resources.Strings.EnterValidIPv6Address);
        }
    }
}
