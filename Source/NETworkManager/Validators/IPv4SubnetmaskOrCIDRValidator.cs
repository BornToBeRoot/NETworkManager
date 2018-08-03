using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using NETworkManager.Models.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.Validators
{
    // ReSharper disable once InconsistentNaming
    public class IPv4SubnetmaskOrCIDRValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var subnetmaskOrCidr = value as string;

            if (subnetmaskOrCidr != null && Regex.IsMatch(subnetmaskOrCidr, RegexHelper.SubnetmaskRegex))
                return ValidationResult.ValidResult;

            if (subnetmaskOrCidr == null || !subnetmaskOrCidr.StartsWith("/"))
                return new ValidationResult(false, Resources.Localization.Strings.EnterValidSubnetmaskOrCIDR);

            if (!int.TryParse(subnetmaskOrCidr.TrimStart('/'), out var cidr))
                return new ValidationResult(false, Resources.Localization.Strings.EnterValidSubnetmaskOrCIDR);

            if (cidr >= 0 && cidr < 33)
                return ValidationResult.ValidResult;

            return new ValidationResult(false, Resources.Localization.Strings.EnterValidSubnetmaskOrCIDR);
        }
    }
}
