using NETworkManager.Settings;
using System.Globalization;
using System.IO;
using System.Windows.Controls;

namespace NETworkManager.Validators
{
    public class SettingsFolderExistsValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var path = value as string;

            if (Directory.Exists(path) || SettingsManager.GetDefaultSettingsLocation() == path)
                return ValidationResult.ValidResult;

            return new ValidationResult(false, Localization.Resources.Strings.FolderDoesNotExist);
        }
    }
}
