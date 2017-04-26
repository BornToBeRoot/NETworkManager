using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace NETworkManager.Views.Validators
{
    public class PortValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {            
            int portNumber = 0;

            if (int.TryParse(value as string, out portNumber))
            {
                if ((portNumber > 0) && (portNumber < 65536))
                    return ValidationResult.ValidResult;
            }

            return new ValidationResult(false, Application.Current.Resources["String_ValidateError_EnterValidPort"] as string);
        }
    }
}