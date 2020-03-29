using System.Globalization;
using System.Windows.Controls;

namespace NETworkManager.Validators
{
    public class PrivacyAESValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return value != null && ((string) value).Length < 8 ? new ValidationResult(false, Localization.Resources.Strings.KeyMustHave8CharactersOrMore) : ValidationResult.ValidResult;
        }
    }
}
