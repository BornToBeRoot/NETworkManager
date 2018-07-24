using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using NETworkManager.Models.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.Validators
{
    // ReSharper disable once InconsistentNaming
    public class IPv4IPv6SubnetmaskOrCIDRValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null)
                return new ValidationResult(false,LocalizationManager.GetStringByKey("String_ValidationError_EnterValidSubnetmaskOrCIDR"));

            var subnetmaskOrCidr = value as string;

            if (subnetmaskOrCidr != null && Regex.IsMatch(subnetmaskOrCidr, RegexHelper.SubnetmaskRegex))
                return ValidationResult.ValidResult;

            if (subnetmaskOrCidr == null || !subnetmaskOrCidr.StartsWith("/"))
                return new ValidationResult(false,LocalizationManager.GetStringByKey("String_ValidationError_EnterValidSubnetmaskOrCIDR"));

            if (!int.TryParse(subnetmaskOrCidr.TrimStart('/'), out var cidr))
                return new ValidationResult(false,LocalizationManager.GetStringByKey("String_ValidationError_EnterValidSubnetmaskOrCIDR"));

            if (cidr >= 0 && cidr < 129)
                return ValidationResult.ValidResult;

            return new ValidationResult(false, LocalizationManager.GetStringByKey("String_ValidationError_EnterValidSubnetmaskOrCIDR"));
        }
    }
}
