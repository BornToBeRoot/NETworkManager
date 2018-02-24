using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace NETworkManager.Validators
{
    public class PrivacyAESValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if ((value as string).Length < 8)
                return new ValidationResult(false, Application.Current.Resources["String_ValidationError_KeyMustHave8CharactersOrMore"] as string);

            return ValidationResult.ValidResult;
        }
    }
}
