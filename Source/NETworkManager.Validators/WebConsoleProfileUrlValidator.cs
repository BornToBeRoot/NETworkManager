using System;
using System.Globalization;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Utilities;

namespace NETworkManager.Validators;

/// <summary>
///     Validates the WebConsole URL field on a profile, which may contain the <c>{{Host}}</c> placeholder.
/// </summary>
public class WebConsoleProfileUrlValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        // Substitute the {{Host}} placeholder with a dummy host so the structural
        // URL validation (scheme, format) still applies, without rejecting the placeholder itself.
        var input = PlaceholderHelper.Replace(value as string ?? "", PlaceholderHelper.Host, "placeholder-host");

        return Uri.TryCreate(input, UriKind.Absolute, out var uriResult) &&
               (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps)
            ? ValidationResult.ValidResult
            : new ValidationResult(false, Strings.EnterValidWebsiteUri);
    }
}
