using NETworkManager.Models.Settings;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace NETworkManager.Validators
{
    public class SettingsFolderExistsValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string path = value as string;

            if (Directory.Exists(path) || SettingsManager.DefaultSettingsLocation == path)
                return ValidationResult.ValidResult;

            return new ValidationResult(false, Application.Current.Resources["String_ValidateError_FolderDoesNotExist"] as string);
        }
    }
}
