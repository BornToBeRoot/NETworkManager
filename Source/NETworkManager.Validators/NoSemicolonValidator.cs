using System.Globalization;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;

namespace NETworkManager.Validators;

/// <summary>
///     Validates that a name doesn't contain ";" - e.g. the name of a DNS server, which can be entered in a list
///     separated by ";".
/// </summary>
public class NoSemicolonValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        return (value as string)?.Contains(';') == true
            ? new ValidationResult(false, Strings.ErrorMessage_NameMustNotContainSemicolon)
            : ValidationResult.ValidResult;
    }
}
