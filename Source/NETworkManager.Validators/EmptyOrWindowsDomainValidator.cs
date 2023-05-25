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
        string domain = value as string;

        if (string.IsNullOrEmpty(domain))
            return ValidationResult.ValidResult;

        // For local authentication "." is a valid domain
        if (domain.Equals("."))
            return ValidationResult.ValidResult;
        
        return Regex.IsMatch(domain, RegexHelper.HostnameRegex) ? ValidationResult.ValidResult : new ValidationResult(false, Strings.EnterValidDomain);        
    }
}
