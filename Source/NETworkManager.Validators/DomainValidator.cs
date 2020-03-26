using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Utilities;

namespace NETworkManager.Validators
{
    public class DomainValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return Regex.IsMatch((string) value, RegexHelper.DomainRegex) ? ValidationResult.ValidResult : new ValidationResult(false,Strings.EnterValidDomain);
        }
    }
}
