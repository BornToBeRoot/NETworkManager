using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Utilities;

namespace NETworkManager.Validators;

public class IPv4IPv6SubnetValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var subnet = (value as string)?.Trim();

        if (string.IsNullOrEmpty(subnet))
            return new ValidationResult(false, Strings.EnterValidSubnet);

        // Check if it is a IPv4 address with a cidr like 192.168.0.0/24            
        if (Regex.IsMatch(subnet, RegexHelper.IPv4AddressCidrRegex))
            return ValidationResult.ValidResult;

        // Check if it is a IPv4 address with a subnetmask like 255.255.255.0
        if (Regex.IsMatch(subnet, RegexHelper.IPv4AddressSubnetmaskRegex))
            return ValidationResult.ValidResult;

        // check if it is a IPv6 address with a cidr like ::1/64
        if (Regex.IsMatch(subnet, RegexHelper.IPv6AddressCidrRegex))
            return ValidationResult.ValidResult;

        return new ValidationResult(false, Strings.EnterValidSubnet);
    }
}