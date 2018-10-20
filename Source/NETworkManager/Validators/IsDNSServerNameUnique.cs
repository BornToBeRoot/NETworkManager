using System;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using NETworkManager.Models.Settings;

namespace NETworkManager.Validators
{
    public class IsDNSServerNameUnique : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return SettingsManager.Current.DNSLookup_DNSServers.Any(x => string.Equals(x.Name, value as string, StringComparison.OrdinalIgnoreCase)) ? new ValidationResult(false, Resources.Localization.Strings.DNSServerWithThisNameAlreadyExists) : ValidationResult.ValidResult;
        }
    }
}
