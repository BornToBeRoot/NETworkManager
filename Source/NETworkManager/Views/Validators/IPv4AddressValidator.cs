using NETworkManager.Utilities.Common;
using NETworkManager.Utilities.Network;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace NETworkManager.Views.Validators
{
    public class IPv4AddressValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (Regex.IsMatch(value as string, RegexHelper.IPv4AddressRegex))
                return ValidationResult.ValidResult;

            return new ValidationResult(false, Application.Current.Resources["String_ValidateError_EnterValidIPv4Address"] as string);
        }
    }
}
