using NETworkManager.Localization.Resources;
using System.Globalization;
using System.Net.Sockets;
using System.Windows.Controls;

namespace NETworkManager.Validators;

public class IPAddressValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var input = (value as string);

        if (System.Net.IPAddress.TryParse(input, out var address) && (address.AddressFamily == AddressFamily.InterNetwork || address.AddressFamily == AddressFamily.InterNetworkV6))
            return ValidationResult.ValidResult;

        return new ValidationResult(false, Strings.EnterValidIPAddress);
    }
}
