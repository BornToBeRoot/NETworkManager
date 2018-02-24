using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace NETworkManager.Validators
{
    public class MultipleHostsValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            foreach(string host in (value as string).Split(';'))
            {
                if (string.IsNullOrEmpty(host))
                    return new ValidationResult(false, Application.Current.Resources["String_ValidationError_EnterValidHosts"] as string);
            }                

            return ValidationResult.ValidResult;
        }
    }
}
