using System.Globalization;
using System.Windows.Controls;

namespace NETworkManager.Validators;

public class EmptyOrInt32Validator : ValidationRule
{
    private static Int32Validator Int32Validator
    {
        get
        {
            field ??= new Int32Validator();
            return field;
        }
    }

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (value is string strValue && string.IsNullOrEmpty(strValue))
            return ValidationResult.ValidResult;
        return Int32Validator.Validate(value, cultureInfo);
    }
}