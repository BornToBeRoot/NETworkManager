using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Utilities;
using System;

namespace NETworkManager.Validators
{
    /// <summary>
    /// Check if the string is a valid directory path (like "C:\Temp\" or "%AppData%\Temp"). The directory path does not have to exist on the local system.
    /// </summary>
    public class DirectoryPathWithEnvironmentVariablesValidator : ValidationRule
    {
        /// <summary>
        /// Check if the string is a valid directory path (like "C:\Temp\" or "%AppData%\Temp"). The directory path does not have to exist on the local system.
        /// </summary>
        /// <param name="value">Directory path like "C:\test" or "%AppData%\test".</param>
        /// <param name="cultureInfo">Culture to use for validation.</param>
        /// <returns>True if the directory path is valid.</returns>
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var path = Environment.ExpandEnvironmentVariables((string)value);

            return new Regex(RegexHelper.FilePath, RegexOptions.IgnoreCase).IsMatch(path) ? ValidationResult.ValidResult : new ValidationResult(false, Strings.EnterValidFilePath);
        }
    }
}