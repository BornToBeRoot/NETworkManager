using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace NETworkManager.Validators
{
    public class TigerVNCPathValidator : ValidationRule
    {
        private static string[] fileNames = new[] { "vncviewer-", "vncviewer64-" };

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string fileName = Path.GetFileName((string)value).ToLower();

            if (fileNames.Any(x => fileName.StartsWith(x) && fileName.EndsWith(".exe")))
                return ValidationResult.ValidResult;

            return new ValidationResult(false, Localization.Resources.Strings.NoValidTigerVNCPath);
        }
    }
}
