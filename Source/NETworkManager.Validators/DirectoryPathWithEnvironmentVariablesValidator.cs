using System.Globalization;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Utilities;

namespace NETworkManager.Validators;

/// <summary>
/// Provides a validation rule that determines whether a value represents a syntactically valid directory path,
/// supporting the inclusion of environment variable references like %UserProfile%.
/// </summary>
public class DirectoryPathWithEnvironmentVariablesValidator : ValidationRule
{
    /// <summary>
    /// Validates whether the specified value represents a valid directory path, allowing for the inclusion of
    /// environment variables like %UserProfile%.
    /// </summary>    
    /// <param name="value">The value to validate as a directory path. May include environment variable references. Can be a string or an
    /// object convertible to a string.</param>
    /// <param name="cultureInfo">The culture-specific information relevant to the validation process. This parameter is not used in this
    /// implementation.</param>
    /// <returns>A ValidationResult that indicates whether the value is a valid directory path. Returns
    /// ValidationResult.ValidResult if the value is valid; otherwise, returns a ValidationResult with an error message.</returns>
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var path = $"{value}";

        return RegexHelper.DirectoryPathWithEnvironmentVariablesRegex().IsMatch(path) || RegexHelper.UncPathRegex().IsMatch(path)
            ? ValidationResult.ValidResult
            : new ValidationResult(false, Strings.EnterValidFolderPath);
    }
}
