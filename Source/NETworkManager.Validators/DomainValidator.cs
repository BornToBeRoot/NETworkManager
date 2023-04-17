using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.Validators;

public class DomainValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (SettingsManager.Current.Whois_UseRipe)
        {
            return Regex.IsMatch((string)value, RegexHelper.IPv4AddressRangeRegex) || Regex.IsMatch((string)value, RegexHelper.IPv4AddressRegex) 
                ? ValidationResult.ValidResult : new ValidationResult(false, Strings.EnterValidIPv4Address);
        }
        else
        {
            return Regex.IsMatch((string)value, RegexHelper.DomainRegex) ? ValidationResult.ValidResult : new ValidationResult(false, Strings.EnterValidDomain);
        }

    }
}
