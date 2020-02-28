using System.Globalization;
using System.Linq;
using System.Windows.Controls;

namespace NETworkManager.Validators
{
    public class NoSpacesValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (string.IsNullOrEmpty(value as string) || !((string) value).Any(char.IsWhiteSpace))
                return ValidationResult.ValidResult;

            return new ValidationResult(false, Localization.LanguageFiles.Strings.SpacesAreNotAllowed);
        }
    }
}