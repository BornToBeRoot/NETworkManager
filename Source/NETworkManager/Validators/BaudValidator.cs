using NETworkManager.Models.Settings;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;

namespace NETworkManager.Validators
{
    public class BaudValidator : ValidationRule
    {
        private int[] Bauds = new int[] { 75, 300, 1200, 2400, 4800, 9600, 14400, 19200, 28800, 38400, 57600, 115200 };

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (int.TryParse(value as string, out int baud))
            {
                if (Bauds.Contains(baud))
                    return ValidationResult.ValidResult;
            }

            return new ValidationResult(false, LocalizationManager.GetStringByKey("String_ValidationError_EnterValidBaud"));
        }
    }
}