using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Utilities;

namespace NETworkManager.Validators
{
    public class ServerValidator : ValidationRule
    {
        public ServerDependencyObjectWrapper Wrapper { get; set; }
        
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            bool allowOnlyIPAddress = Wrapper.AllowOnlyIPAddress;
            var genericErrorResult = allowOnlyIPAddress ? Strings.EnterValidIPAddress : Strings.EnterValidHostnameOrIPAddress;

            var input = (value as string)?.Trim();

            if(string.IsNullOrEmpty(input))
                return new ValidationResult(false, genericErrorResult);

            // Check if it is a valid IPv4 address like 192.168.0.1
            if (Regex.IsMatch(input, RegexHelper.IPv4AddressRegex))
                return ValidationResult.ValidResult;

            // Check if it is a valid IPv6 address like ::1
            if (Regex.IsMatch(input, RegexHelper.IPv6AddressRegex))
                return ValidationResult.ValidResult;

            // Check if it is a valid hostname like server-01 or server-01.example.com
            if (Regex.IsMatch(input, RegexHelper.HostnameRegex) && !allowOnlyIPAddress)
                return ValidationResult.ValidResult;

            return new ValidationResult(false, genericErrorResult);
        }
    }
}
