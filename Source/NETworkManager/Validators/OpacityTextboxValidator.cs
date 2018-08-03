using NETworkManager.Models.Settings;
using System.Globalization;
using System.Windows.Controls;

namespace NETworkManager.Validators
{
    public class OpacityTextboxValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (!int.TryParse(value as string, out var result))
                return new ValidationResult(false,Resources.Localization.Strings.OnlyNumbersCanBeEntered);

            if (result < 10 || result > 100)
                return new ValidationResult(false, Resources.Localization.Strings.EnterValidValueBetween25and100);

            return ValidationResult.ValidResult;

        }
    }
}
