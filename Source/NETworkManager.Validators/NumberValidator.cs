using System.Globalization;
using System.Linq;
using System.Windows.Controls;

namespace NETworkManager.Validators
{
    public class NumberValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return ((string)value).All(char.IsNumber) ? ValidationResult.ValidResult : new ValidationResult(false, Localization.Resources.Strings.OnlyNumbersCanBeEntered);
        }
    }
}
