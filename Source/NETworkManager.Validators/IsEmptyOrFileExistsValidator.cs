using System.Globalization;
using System.IO;
using System.Windows.Controls;

namespace NETworkManager.Validators
{
    public class IsEmptyOrFileExistsValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (string.IsNullOrEmpty(value as string))
                return ValidationResult.ValidResult;

            return File.Exists((string)value) ? ValidationResult.ValidResult : new ValidationResult(false, Localization.Resources.Strings.FileDoesNotExist);
        }
    }
}