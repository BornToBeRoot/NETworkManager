using NETworkManager.Models.Settings;
using System.Globalization;
using System.Windows.Controls;

namespace NETworkManager.Validators
{
    public class Int32Validator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (int.TryParse(value as string, out int x) && x > 0)
                return ValidationResult.ValidResult;
            else
                return new ValidationResult(false, LocalizationManager.GetStringByKey("String_ValidationError_EnterValidNumber"));
        }
    }
}
