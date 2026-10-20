using System;
using System.Globalization;
using System.IO;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;

namespace NETworkManager.Validators;

public class EmptyOrFileExistsValidator : ValidationRule
{
    /// <summary>
    /// Expand environment variables (e.g. <c>%SystemRoot%</c>) before checking if the file exists.
    /// Only enable this if the consumer of the path expands them as well (e.g. Windows Firewall).
    /// </summary>
    public bool ExpandEnvironmentVariables { get; set; }

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (string.IsNullOrEmpty(value as string))
            return ValidationResult.ValidResult;

        var path = ExpandEnvironmentVariables
            ? Environment.ExpandEnvironmentVariables((string)value)
            : (string)value;

        return File.Exists(path)
            ? ValidationResult.ValidResult
            : new ValidationResult(false, Strings.FileDoesNotExist);
    }
}