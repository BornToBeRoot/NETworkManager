using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace NETworkManager.Validators
{
    public class PortRangeValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            bool isValid = true;

            foreach (string portOrRange in (value as string).Replace(" ", "").Split(';'))
            {
                if (portOrRange.Contains("-"))
                {
                    string[] portRange = portOrRange.Split('-');

                    if (int.TryParse(portRange[0], out int startPort) && int.TryParse(portRange[1], out int endPort))
                    {
                        if (!((startPort > 0) && (startPort < 65536) && (endPort > 0) && (endPort < 65536) && (startPort < endPort)))
                            isValid = false;
                    }
                    else
                    {
                        isValid = false;
                    }
                }
                else
                {
                    if (int.TryParse(portOrRange, out int portNumber))
                    {
                        if (!((portNumber > 0) && (portNumber < 65536)))
                            isValid = false;
                    }
                    else
                    {
                        isValid = false;
                    }
                }
            }

            if (isValid)
                return ValidationResult.ValidResult;
            else
                return new ValidationResult(false, Application.Current.Resources["String_ValidateError_EnterValidPortOrPortRange"] as string);
        }
    }
}