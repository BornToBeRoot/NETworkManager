using System.Globalization;
using System.IO;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;

namespace NETworkManager.Validators;

public class FileExistsValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        return File.Exists((string)value)
            ? ValidationResult.ValidResult
            : new ValidationResult(false, Strings.FileDoesNotExist);
    }
}