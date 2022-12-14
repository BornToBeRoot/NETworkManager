using System.Globalization;
using System.IO;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;

namespace NETworkManager.Validators
{
    /// <summary>
    /// Check if the folder in the full path exists on the system (like "C:\Temp" from "C:\Temp\export.json").
    /// </summary>
    public class FolderInPathExistsValidator : ValidationRule
    {
        /// <summary>
        /// Check if the string is a valid file path (like "C:\Temp\Test.txt"). The file path does not have to exist on the local system.
        /// </summary>
        /// <param name="value">File path like "C:\Temp\Test.txt"".</param>
        /// <param name="cultureInfo">Culture to use for validation.</param>
        /// <returns>True if the folder exists.</returns>
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string path = Path.GetDirectoryName((string)value);

            if (Directory.Exists(path))
                return ValidationResult.ValidResult;
            else
                return new ValidationResult(false, Strings.FolderDoesNotExist);
        }
    }
}