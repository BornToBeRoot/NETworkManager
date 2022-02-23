using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Utilities;

namespace NETworkManager.Validators
{
    public class RemoteDesktopHostnameAndPortValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string hostnameAndPort = (string)value;

            if(hostnameAndPort.Contains(":"))
            {
                string[] hostnameAndPortValues = hostnameAndPort.Split(':');

                if (Regex.IsMatch(hostnameAndPortValues[0], RegexHelper.HostnameRegex) && !string.IsNullOrEmpty(hostnameAndPortValues[1]) && Regex.IsMatch(hostnameAndPortValues[1], RegexHelper.PortRegex))
                    return ValidationResult.ValidResult;
                
                return new ValidationResult(false, Strings.EnterValidHostnameAndPort);
            }
            else
            {
                return Regex.IsMatch((string)value, RegexHelper.HostnameRegex) ? ValidationResult.ValidResult : new ValidationResult(false, Strings.EnterValidHostname);
            }
        }
    }
}
