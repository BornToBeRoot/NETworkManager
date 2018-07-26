using NETworkManager.Models.Settings;
using System.Globalization;
using System.Windows.Controls;

namespace NETworkManager.Validators
{
    public class PrivacyAESValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return value != null && ((string) value).Length < 8 ? new ValidationResult(false, LocalizationManager.GetStringByKey("String_ValidationError_KeyMustHave8CharactersOrMore")) : ValidationResult.ValidResult;
        }
    }
}
