using System.Globalization;
using System.Linq;
using System.Windows.Controls;

namespace NETworkManager.Validators
{
    public class BaudValidator : ValidationRule
    {
        /// <summary>
        /// Possible baud rates.
        /// </summary>
        private readonly int[] _bauds = { 75, 300, 1200, 2400, 4800, 9600, 14400, 19200, 28800, 38400, 57600, 115200 };

        /// <summary>
        /// Check if the int is a valid baud rate.
        /// </summary>
        /// <param name="value">Baud like 9600.</param>
        /// <param name="cultureInfo">Culture to use for validation.</param>
        /// <returns>True if the value is a valid baut rate.</returns>
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (!int.TryParse(value as string, out var baud))
                return new ValidationResult(false, Localization.Resources.Strings.EnterValidBaud);

            return _bauds.Contains(baud) ? ValidationResult.ValidResult : new ValidationResult(false, Localization.Resources.Strings.EnterValidBaud);
        }
    }
}