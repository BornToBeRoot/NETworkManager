using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;

namespace NETworkManager.Validators
{
    /// <summary>
    /// Check if the filename is a valid file name (like "text.txt"). The file name does not have to exist on the local system.
    /// </summary>
    public class FileNameValidator : ValidationRule
    {
        /// <summary>
        /// Check if the filename is a valid file name (like "text.txt"). The filen ame does not have to exist on the local system.
        /// </summary>
        /// <param name="value">File name like "test.txt" or "README.md".</param>
        /// <param name="cultureInfo">Culture to use for validation.</param>
        /// <returns>True if the file name is valid.</returns>
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var filename = (string)value;

            // Check if the filename has valid chars and a dot.
            return filename.IndexOfAny(Path.GetInvalidFileNameChars()) < 0 && new Regex(@"^.+\..+$").IsMatch(filename) ? ValidationResult.ValidResult : new ValidationResult(false, Strings.EnterValidFileName);
        }
    }
}