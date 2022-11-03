using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;

namespace NETworkManager.Validators
{
    public class ContainsTextValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return ((string)value).All(char.IsWhiteSpace) ? new ValidationResult(false, Strings.InputDoesNotContainAnyText) : ValidationResult.ValidResult;
        }
    }
}
