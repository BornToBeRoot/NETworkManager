using NETworkManager.Models.Settings;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace NETworkManager.Validators
{
    public class FileExistsValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (File.Exists(value as string))
                return ValidationResult.ValidResult;

            return new ValidationResult(false, Application.Current.Resources["String_ValidationError_FileDoesNotExist"] as string);
        }
    }
}
