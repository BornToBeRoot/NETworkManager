using System.Globalization;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Utilities;

namespace NETworkManager.Validators;

public class EmptyOrWindowsDomainValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var domain = value as string;

        if (string.IsNullOrEmpty(domain))
            return ValidationResult.ValidResult;

        // For local authentication "." is a valid domain
        if (domain.Equals("."))
            return ValidationResult.ValidResult;

        return RegexHelper.HostnameOrDomainRegex().IsMatch(domain)
            ? ValidationResult.ValidResult
            : new ValidationResult(false, Strings.EnterValidDomain);
    }
}