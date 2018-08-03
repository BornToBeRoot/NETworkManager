using System;
using System.Globalization;
using System.Windows.Controls;

namespace NETworkManager.Validators
{
    public class HttpAndHttpsUriValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return Uri.TryCreate(value as string, UriKind.Absolute, out var uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps) ? ValidationResult.ValidResult : new ValidationResult(false, Resources.Localization.Strings.EnterValidWebsiteUri);
        }
    }
}
