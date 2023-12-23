using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;

namespace NETworkManager.Validators;

public class PowerShellPathValidator : ValidationRule
{
    private static readonly string[] fileNames = { "powershell.exe", "pwsh.exe" };

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        return fileNames.Contains(Path.GetFileName((string)value).ToLower())
            ? ValidationResult.ValidResult
            : new ValidationResult(false, Strings.NoValidPowerShellPath);
    }
}