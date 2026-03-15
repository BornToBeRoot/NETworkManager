using System;
using System.Globalization;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;

namespace NETworkManager.Validators;

/// <summary>
/// Evaluate whether the file path is a *.exe or *.EXE file.
/// </summary>
public class EmptyOrFileIsExeValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (value is not string strVal || string.IsNullOrEmpty(strVal)
                                       || strVal.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase))
            return ValidationResult.ValidResult;
        return new ValidationResult(false, Strings.EnterPathToExe);
    }
}