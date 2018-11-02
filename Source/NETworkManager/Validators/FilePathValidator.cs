using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using NETworkManager.Resources.Localization;
using NETworkManager.Utilities;

namespace NETworkManager.Validators
{
    public class FilePathValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var regex = new Regex(RegexHelper.FilePath, RegexOptions.IgnoreCase);

            return value != null && regex.IsMatch((string)value) ? ValidationResult.ValidResult : new ValidationResult(false, Strings.EnterValidFilePath);
        }
    }
}