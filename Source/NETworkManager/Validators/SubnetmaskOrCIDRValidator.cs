using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using NETworkManager.Utilities.Common;

namespace NETworkManager.Validators
{
    public class SubnetmaskOrCIDRValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string subnetmaskOrCidr = value as string;

            if (Regex.IsMatch(subnetmaskOrCidr, RegexHelper.SubnetmaskRegex))
                return ValidationResult.ValidResult;

            if (subnetmaskOrCidr.StartsWith("/"))
            {
                int cidr;

                if (int.TryParse(subnetmaskOrCidr.TrimStart('/'), out cidr))
                {
                    if (cidr >= 0 && cidr < 33)
                        return ValidationResult.ValidResult;
                }
            }

            return new ValidationResult(false, Application.Current.Resources["String_ValidateError_EnterValidSubnetmaskOrCIDR"] as string);
        }
    }
}
