using NETworkManager.Localization.Resources;
using NETworkManager.Utilities;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace NETworkManager.Validators;

public class EmptyOrWindowsDomainValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (string.IsNullOrEmpty(value as string))
            return ValidationResult.ValidResult;

        return Regex.IsMatch((string)value, RegexHelper.HostnameRegex) ? ValidationResult.ValidResult : new ValidationResult(false, Strings.EnterValidDomain);        
    }
}
