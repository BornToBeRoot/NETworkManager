using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using NETworkManager.Utilities;

namespace NETworkManager.Validators
{
    public class IPv4IPv6SubnetmaskOrCIDRValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {   
            var subnetmaskOrCidr = (value as string)?.Trim();

            if(string.IsNullOrEmpty(subnetmaskOrCidr))
                return new ValidationResult(false, Localization.Resources.Strings.EnterValidSubnetmaskOrCIDR);

            // Check if it is a subnetmask like 255.255.255.0
            if (Regex.IsMatch(subnetmaskOrCidr, RegexHelper.SubnetmaskRegex))
                return ValidationResult.ValidResult;

            // Check if it is a CIDR like /24
            if (int.TryParse(subnetmaskOrCidr.TrimStart('/'), out var cidr))
            {
                if (cidr >= 0 && cidr < 129)
                    return ValidationResult.ValidResult;
            }           
            
            return new ValidationResult(false, Localization.Resources.Strings.EnterValidSubnetmaskOrCIDR);
        }
    }
}
