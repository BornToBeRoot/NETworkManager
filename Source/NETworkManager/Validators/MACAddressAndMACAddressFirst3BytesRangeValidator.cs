using NETworkManager.Helpers;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace NETworkManager.Validators
{
    public class MACAddressAndMACAddressFirst3BytesRangeValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            foreach (string macAddress in (value as string).Replace(" ","").Split(';'))
            {
                if (!Regex.IsMatch(macAddress, RegexHelper.MACAddressFirst3BytesRegex) && !Regex.IsMatch(macAddress, RegexHelper.MACAddressRegex))
                    return new ValidationResult(false, Application.Current.Resources["String_ValidateError_EnterValidMACAddressOrFirst3Bytes"] as string);
            }

            return ValidationResult.ValidResult;
        }
    }
}