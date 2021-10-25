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
                return Regex.IsMatch((string)value, RegexHelper.HostnameAndPortRegex) ? ValidationResult.ValidResult : new ValidationResult(false, Strings.EnterValidHostnameAndPort);
            }
            else
            {
                return Regex.IsMatch((string)value, RegexHelper.HostnameRegex) ? ValidationResult.ValidResult : new ValidationResult(false, Strings.EnterValidHostname);
            }
        }
    }
}
