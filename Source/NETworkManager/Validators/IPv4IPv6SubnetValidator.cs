using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using NETworkManager.Models.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.Validators
{
    // ReSharper disable once InconsistentNaming
    public class IPv4IPv6SubnetValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var subnet = (value as string)?.Trim();

            if (subnet != null && Regex.IsMatch(subnet, RegexHelper.SubnetCalculatorIPv4AddressCidrRegex))
                return ValidationResult.ValidResult;

            if (subnet != null && Regex.IsMatch(subnet, RegexHelper.SubnetCalculatorIPv4AddressSubnetmaskRegex))
                return ValidationResult.ValidResult;

            if (subnet != null && Regex.IsMatch(subnet, RegexHelper.IPv6AddressCidrRegex))
                return ValidationResult.ValidResult;

            return new ValidationResult(false, Resources.Localization.Strings.EnterValidSubnet);
        }
    }
}
