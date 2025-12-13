using System.Globalization;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Utilities;

namespace NETworkManager.Validators;

public class SubnetmaskValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        return value != null && RegexHelper.SubnetmaskRegex().IsMatch((string)value)
            ? ValidationResult.ValidResult
            : new ValidationResult(false, Strings.EnterValidSubnetmask);
    }
}