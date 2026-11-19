using System;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;

namespace NETworkManager.Validators;

/// <summary>
///     Validates that a name is not used yet (case-insensitive). Leading / trailing spaces are ignored, like when a name
///     is found (e.g. a port profile in the ports input), so "Webserver " is a duplicate of "Webserver".
/// </summary>
public class IsNameUnique : ValidationRule
{
    public IsNameUniqueDependencyObjectWrapper Wrapper { get; set; }

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var name = (value as string)?.Trim();

        return Wrapper.UsedNames.Any(x => string.Equals(x?.Trim(), name, StringComparison.OrdinalIgnoreCase))
            ? new ValidationResult(false, Strings.ErrorMessage_NameIsAlreadyUsed)
            : ValidationResult.ValidResult;
    }
}