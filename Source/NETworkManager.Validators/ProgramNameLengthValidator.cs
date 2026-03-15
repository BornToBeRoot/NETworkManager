using System.Globalization;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;

namespace NETworkManager.Validators;

public class ProgramNameLengthValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (value is not string strVal)
            return ValidationResult.ValidResult;
        return strVal.Length > 259 ? new ValidationResult(false, Strings.ProgramNameTooLong)
            : ValidationResult.ValidResult;
    }
}