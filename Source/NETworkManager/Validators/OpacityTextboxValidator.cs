using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace NETworkManager.Validators
{
    public class OpacityTextboxValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (int.TryParse(value as string, out int result))
            {
                if (result < 10 || result > 100)
                    return new ValidationResult(false, Application.Current.Resources["String_ValidationError_EnterValidValueBetween10and100"] as string);
                else
                    return ValidationResult.ValidResult;
            }
            else
            {
                return new ValidationResult(false, Application.Current.Resources["String_ValidationError_OnlyNumbersCanBeEntered"] as string);
            }
        }
    }
}
