using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Utilities;

namespace NETworkManager.Validators
{
    public class DomainOrIPAddressValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {            
            if (value != null && (Regex.IsMatch((string)value, RegexHelper.IPv4AddressRegex) || Regex.IsMatch((string)value, RegexHelper.IPv6AddressRegex) || Regex.IsMatch((string)value, RegexHelper.DomainRegex)))
                return ValidationResult.ValidResult;

            return new ValidationResult(false, Strings.EnterValidDomainOrIPAddress);
        }
    }
}
