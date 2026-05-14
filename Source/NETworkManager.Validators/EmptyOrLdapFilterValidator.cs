using System.Globalization;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;

namespace NETworkManager.Validators;

/// <summary>
///     Accepts an empty value or a string that looks like an LDAP filter expression
///     (starts with '(' and ends with ')'). Heavy syntax checking is deferred to
///     the directory server itself.
/// </summary>
public class EmptyOrLdapFilterValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var input = (value as string)?.Trim();

        if (string.IsNullOrEmpty(input))
            return ValidationResult.ValidResult;

        return input.StartsWith('(') && input.EndsWith(')')
            ? ValidationResult.ValidResult
            : new ValidationResult(false, Strings.ActiveDirectoryAdditionalLdapFilterInvalid);
    }
}
