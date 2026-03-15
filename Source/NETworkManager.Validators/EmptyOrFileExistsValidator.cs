using System.Globalization;
using System.Windows.Controls;

namespace NETworkManager.Validators;

public class EmptyOrFileExistsValidator : ValidationRule
{
    private static FileExistsValidator FileExistsValidator
    {
        get
        {
            field ??= new FileExistsValidator();
            return field;
        }
    }

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        return string.IsNullOrEmpty(value as string)
            ? ValidationResult.ValidResult : FileExistsValidator.Validate(value, cultureInfo);
    }
}