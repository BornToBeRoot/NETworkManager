using System.Globalization;
using System.Windows.Controls;

namespace NETworkManager.Validators
{
    public class EmptyOrPortRangeValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (string.IsNullOrEmpty(value as string))
                return ValidationResult.ValidResult;

            var isValid = true;

            if (value == null)
                return new ValidationResult(false, Localization.Resources.Strings.EnterValidPortOrPortRange);

            foreach (var portOrRange in ((string)value).Replace(" ", "").Split(';'))
            {
                if (portOrRange.Contains("-"))
                {
                    var portRange = portOrRange.Split('-');

                    if (int.TryParse(portRange[0], out var startPort) && int.TryParse(portRange[1], out var endPort))
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
                    if (int.TryParse(portOrRange, out var portNumber))
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

            return isValid ? ValidationResult.ValidResult : new ValidationResult(false, Localization.Resources.Strings.EnterValidPortOrPortRange);
        }
    }
}