using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using NETworkManager.Helpers;

namespace NETworkManager.Validators
{
    public class SubnetValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string subnet = (value as string).Trim();

            if (Regex.IsMatch(subnet, RegexHelper.IPv4AddressCidrRegex))
                return ValidationResult.ValidResult;

            if (Regex.IsMatch(subnet, RegexHelper.IPv4AddressSubnetmaskRegex))
                return ValidationResult.ValidResult;

            return new ValidationResult(false, Application.Current.Resources["String_ValidateError_EnterValidSubnet"] as string);
        }
    }
}
