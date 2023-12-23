using System.Globalization;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;

namespace NETworkManager.Validators;

public class EmptyValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        return string.IsNullOrEmpty((string)value)
            ? new ValidationResult(false, Strings.FieldCannotBeEmpty)
            : ValidationResult.ValidResult;
    }
}