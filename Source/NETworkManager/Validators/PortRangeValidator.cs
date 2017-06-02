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

                    int startPort = 0;
                    int endPort = 0;

                    if (int.TryParse(portRange[0], out startPort) && int.TryParse(portRange[1], out endPort))
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
                    int portNumber = 0;

                    if (int.TryParse(portOrRange, out portNumber))
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
                return new ValidationResult(false, Application.Current.Resources["String_ValidationError_EnterValidPortOrPortRange"] as string);
        }
    }
}