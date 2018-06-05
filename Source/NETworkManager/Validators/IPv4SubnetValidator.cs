using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using NETworkManager.Models.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.Validators
{
    public class IPv4SubnetValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string subnet = (value as string).Trim();

            if (Regex.IsMatch(subnet, RegexHelper.IPv4AddressCidrRegex))
                return ValidationResult.ValidResult;

            if (Regex.IsMatch(subnet, RegexHelper.IPv4AddressSubnetmaskRegex))
                return ValidationResult.ValidResult;

            return new ValidationResult(false, LocalizationManager.GetStringByKey("String_ValidationError_EnterValidSubnet"));
        }
    }
}
