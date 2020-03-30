using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Utilities;

namespace NETworkManager.Validators
{
    /// <summary>
    /// Check if the string is a valid file path (like "C:\Temp\Test.txt"). The file path does not have to exist on the local system.
    /// </summary>
    public class FilePathValidator : ValidationRule
    {
        /// <summary>
        /// Check if the string is a valid file path (like "C:\Temp\Test.txt"). The file path does not have to exist on the local system.
        /// </summary>
        /// <param name="value">File path like "C:\Temp\Test.txt"".</param>
        /// <param name="cultureInfo">Culture to use for validation.</param>
        /// <returns>True if the file path is valid.</returns>
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return new Regex(RegexHelper.FullName, RegexOptions.IgnoreCase).IsMatch((string)value) ? ValidationResult.ValidResult : new ValidationResult(false, Strings.EnterValidFilePath);
        }
    }
}