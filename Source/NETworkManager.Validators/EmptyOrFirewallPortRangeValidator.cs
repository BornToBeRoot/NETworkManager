using System.Globalization;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Settings;

namespace NETworkManager.Validators;

public class EmptyOrFirewallPortRangeValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (string.IsNullOrEmpty(value as string))
            return ValidationResult.ValidResult;

        var isValid = true;
        char portSeparator = SettingsManager.Current.Firewall_UseWindowsPortSyntax ? ',' : ';';
        var portList = ((string)value).Replace(" ", "").Split(portSeparator);
        if (portList.Length > 10000)
            return new ValidationResult(false, Strings.EnterLessThan10001PortsOrPortRanges);
        foreach (var portOrRange in portList)
            if (portOrRange.Contains('-'))
            {
                var portRange = portOrRange.Split('-');

                if (int.TryParse(portRange[0], out var startPort) && int.TryParse(portRange[1], out var endPort))
                {
                    if (startPort is < 0 or > 65536 || endPort is < 0 or > 65536 &&
                        startPort > endPort)
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
                    if (portNumber is <= 0 or >= 65536)
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