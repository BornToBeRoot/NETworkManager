using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;

namespace NETworkManager.Validators;

public class TigerVNCPathValidator : ValidationRule
{
    private static readonly string[] fileNames = { "vncviewer-", "vncviewer64-" };

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var fileName = Path.GetFileName((string)value).ToLower();

        if (fileNames.Any(x => fileName.StartsWith(x) && fileName.EndsWith(".exe")))
            return ValidationResult.ValidResult;

        return new ValidationResult(false, Strings.NoValidTigerVNCPath);
    }
}