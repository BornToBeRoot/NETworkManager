using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using NETworkManager.Helpers;

namespace NETworkManager.Validators
{
    public class SubnetCalculatorSubnetValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string subnet = (value as string).Trim();

            if (Regex.IsMatch(subnet, RegexHelper.SubnetCalculatorIPv4AddressCidrRegex))
                return ValidationResult.ValidResult;

            if (Regex.IsMatch(subnet, RegexHelper.SubnetCalculatorIPv4AddressSubnetmaskRegex))
                return ValidationResult.ValidResult;

            return new ValidationResult(false, Application.Current.Resources["String_ValidationError_EnterValidSubnet"] as string);
        }
    }
}
