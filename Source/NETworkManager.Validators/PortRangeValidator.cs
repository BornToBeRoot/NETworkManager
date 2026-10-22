using System.Globalization;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Utilities;

namespace NETworkManager.Validators;

public class PortRangeValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var isValid = true;

        if (value == null)
            return new ValidationResult(false, Strings.EnterValidPortOrPortRange);

        foreach (var portOrRange in ((string)value).Replace(" ", "").Split(';'))
            if (portOrRange.Contains('-'))
            {
                var portRange = portOrRange.Split('-');

                if (int.TryParse(portRange[0], out var startPort) && int.TryParse(portRange[1], out var endPort))
                {
                    if (!PortRangeHelper.IsValidPortRange(startPort, endPort))
                        isValid = false;
                }
                else
                {
                    isValid = false;
                }
            }
            else
            {
                if (int.TryParse(portOrRange, out var portNumber))
                {
                    if (!PortRangeHelper.IsValidPort(portNumber))
                        isValid = false;
                }
                else
                {
                    isValid = false;
                }
            }

        return isValid
            ? ValidationResult.ValidResult
            : new ValidationResult(false, Strings.EnterValidPortOrPortRange);
    }
}