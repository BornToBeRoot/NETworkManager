using System.Globalization;
using System.Windows.Controls;

namespace NETworkManager.Validators
{
    public class MultipleHostsValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null)
                return ValidationResult.ValidResult;

            for (var index = 0; index < ((string)value).Split(';').Length; index++)
            {
                var host = ((string)value).Split(';')[index];

                if (string.IsNullOrEmpty(host))
                    return new ValidationResult(false, Resources.Localization.Strings.EnterValidHosts);
            }

            return ValidationResult.ValidResult;
        }
    }
}
