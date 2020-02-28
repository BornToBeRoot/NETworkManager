using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using NETworkManager.Utilities;

namespace NETworkManager.Validators
{
    // ReSharper disable once InconsistentNaming
    public class IPv4SubnetValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var subnet = (value as string)?.Trim();

            if (subnet != null && Regex.IsMatch(subnet, RegexHelper.IPv4AddressCidrRegex))
                return ValidationResult.ValidResult;

            if (subnet != null && Regex.IsMatch(subnet, RegexHelper.IPv4AddressSubnetmaskRegex))
                return ValidationResult.ValidResult;

            return new ValidationResult(false, Localization.LanguageFiles.Strings.EnterValidSubnet);
        }
    }
}
