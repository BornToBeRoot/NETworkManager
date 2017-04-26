using NETworkManager.Utilities.Common;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace NETworkManager.Views.Validators
{
    public class SubnetmaskValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (Regex.IsMatch(value as string, RegexHelper.SubnetmaskRegex))
                return ValidationResult.ValidResult;

            return new ValidationResult(false, Application.Current.Resources["String_ValidateError_EnterValidSubnetmask"] as string);
        }
    }
}
