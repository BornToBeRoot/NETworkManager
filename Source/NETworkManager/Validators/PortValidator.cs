using System.Globalization;
using System.Windows.Controls;

namespace NETworkManager.Validators
{
    public class PortValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (!int.TryParse(value as string, out var portNumber))
                return new ValidationResult(false, Localization.LanguageFiles.Strings.EnterValidPort);

            if (portNumber > 0 && (portNumber < 65536))
                return ValidationResult.ValidResult;

            return new ValidationResult(false, Localization.LanguageFiles.Strings.EnterValidPort);
        }
    }
}