using System.Globalization;
using System.Windows.Controls;

namespace NETworkManager.Validators;

public class EmptyOrFilePathValidator : ValidationRule
{
    private static FilePathValidator FilePathValidator
    {
        get
        {
            field ??= new FilePathValidator();
            return field;
        }
    }

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (value is not string strValue || string.IsNullOrEmpty(strValue))
            return ValidationResult.ValidResult;
        return FilePathValidator.Validate(value, cultureInfo);
    }
}