using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using NETworkManager.Models.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.Validators
{
    public class IPv4IPv6SubnetmaskOrCIDRValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string subnetmaskOrCidr = value as string;

            if (Regex.IsMatch(subnetmaskOrCidr, RegexHelper.SubnetmaskRegex))
                return ValidationResult.ValidResult;

            if (subnetmaskOrCidr.StartsWith("/"))
            {
                if (int.TryParse(subnetmaskOrCidr.TrimStart('/'), out int cidr))
                {
                    if (cidr >= 0 && cidr < 129)
                        return ValidationResult.ValidResult;
                }
            }

            return new ValidationResult(false, LocalizationManager.GetStringByKey("String_ValidationError_EnterValidSubnetmaskOrCIDR"));
        }
    }
}
