using System.Globalization;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Utilities;

namespace NETworkManager.Validators;

public class DNSSuffixValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (value is not string suffix || string.IsNullOrEmpty(suffix))
            return new ValidationResult(false, Strings.FieldCannotBeEmpty);

        return RegexHelper.DNSSuffixRegex().IsMatch(suffix)
            ? ValidationResult.ValidResult
            : new ValidationResult(false, Strings.EnterValidDomain);
    }
}
