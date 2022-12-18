using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace NETworkManager.Validators
{
    public class PuTTYPathValidator : ValidationRule
    {
        private static string[] fileNames = new[] { "putty.exe" };

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return fileNames.Contains(Path.GetFileName((string)value).ToLower()) ? ValidationResult.ValidResult : new ValidationResult(false, Localization.Resources.Strings.NoValidPuTTYPath);
        }
    }
}
