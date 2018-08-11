using System.Globalization;
using System.Linq;
using System.Windows.Controls;

namespace NETworkManager.Validators
{
    public class BaudValidator : ValidationRule
    {
        private readonly int[] _bauds = { 75, 300, 1200, 2400, 4800, 9600, 14400, 19200, 28800, 38400, 57600, 115200 };

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (!int.TryParse(value as string, out var baud))
                return new ValidationResult(false, Resources.Localization.Strings.EnterValidBaud);

            return _bauds.Contains(baud) ? ValidationResult.ValidResult : new ValidationResult(false, Resources.Localization.Strings.EnterValidBaud);
        }
    }
}