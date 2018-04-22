using NETworkManager.Models.Settings;
using System.Globalization;
using System.Windows.Controls;

namespace NETworkManager.Validators
{
    public class PrivacyAESValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if ((value as string).Length < 8)
                return new ValidationResult(false, LocalizationManager.GetStringByKey("String_ValidationError_KeyMustHave8CharactersOrMore"));

            return ValidationResult.ValidResult;
        }
    }
}
