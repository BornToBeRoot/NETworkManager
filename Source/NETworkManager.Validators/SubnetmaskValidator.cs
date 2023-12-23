using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Utilities;

namespace NETworkManager.Validators;

public class SubnetmaskValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        return value != null && Regex.IsMatch((string)value, RegexHelper.SubnetmaskRegex)
            ? ValidationResult.ValidResult
            : new ValidationResult(false, Strings.EnterValidSubnetmask);
    }
}