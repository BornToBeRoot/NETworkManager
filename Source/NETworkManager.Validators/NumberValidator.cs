using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Utilities;

namespace NETworkManager.Validators;

public class NumberValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        return Regex.IsMatch(((string)value).Trim(), RegexHelper.NumberRegex)
            ? ValidationResult.ValidResult
            : new ValidationResult(false, Strings.OnlyNumbersCanBeEntered);
    }
}