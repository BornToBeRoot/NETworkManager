using System.Globalization;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Utilities;

namespace NETworkManager.Validators;

public class PortValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (!int.TryParse(value as string, out var portNumber))
            return new ValidationResult(false, Strings.EnterValidPort);

        if (PortRangeHelper.IsValidPort(portNumber))
            return ValidationResult.ValidResult;

        return new ValidationResult(false, Strings.EnterValidPort);
    }
}