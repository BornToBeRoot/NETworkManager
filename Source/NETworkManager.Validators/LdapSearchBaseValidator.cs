using System.Globalization;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Utilities;

namespace NETworkManager.Validators;

public class LdapSearchBaseValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var input = (value as string)?.Trim();

        if (string.IsNullOrEmpty(input))
            return new ValidationResult(false, Strings.FieldCannotBeEmpty);

        return RegexHelper.LdapSearchBaseRegex().IsMatch(input)
            ? ValidationResult.ValidResult
            : new ValidationResult(false, Strings.ActiveDirectorySearchBaseInvalid);
    }
}