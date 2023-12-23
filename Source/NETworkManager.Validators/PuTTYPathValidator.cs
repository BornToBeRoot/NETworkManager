using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;

namespace NETworkManager.Validators;

public class PuTTYPathValidator : ValidationRule
{
    private static readonly string[] fileNames = { "putty.exe" };

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        return fileNames.Contains(Path.GetFileName((string)value).ToLower())
            ? ValidationResult.ValidResult
            : new ValidationResult(false, Strings.NoValidPuTTYPath);
    }
}