using ControlzEx.Standard;
using NETworkManager.Localization.Resources;
using NETworkManager.Utilities;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace NETworkManager.Validators;

public class EmptyOrIPv4AddressValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (string.IsNullOrEmpty(value as string))
            return ValidationResult.ValidResult;

        return RegexHelper.IPv4AddressRegex().IsMatch((string)value)
            ? ValidationResult.ValidResult
            : new ValidationResult(false, Strings.EnterValidIPv4Address);
    }
}