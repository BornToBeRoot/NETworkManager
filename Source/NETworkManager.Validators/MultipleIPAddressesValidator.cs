using System.Globalization;
using System.Net;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;

namespace NETworkManager.Validators;

public class MultipleIPAddressesValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var ipAddresses = (value as string)?.Split(';');
        
        if (ipAddresses == null || ipAddresses.Length == 0)
            return new ValidationResult(false, Strings.EnterOneOrMoreValidIPAddresses);
        
        foreach (var ipAddress in ipAddresses)
        {
            if(!(IPAddress.TryParse(ipAddress, out var ip) && ip.AddressFamily is System.Net.Sockets.AddressFamily.InterNetwork or System.Net.Sockets.AddressFamily.InterNetworkV6))
                return new ValidationResult(false, Strings.EnterOneOrMoreValidIPAddresses);
        }

        return ValidationResult.ValidResult;
    }
}