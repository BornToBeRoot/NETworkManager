using System.Globalization;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Utilities;

namespace NETworkManager.Validators;

public class WindowsUsernameValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var input = (value as string)?.Trim();

        if (string.IsNullOrEmpty(input))
            return new ValidationResult(false, Strings.EnterValidUsername);

        return RegexHelper.WindowsUsernameRegex().IsMatch(input)
            ? ValidationResult.ValidResult
            : new ValidationResult(false, Strings.EnterValidUsername);
    }
}