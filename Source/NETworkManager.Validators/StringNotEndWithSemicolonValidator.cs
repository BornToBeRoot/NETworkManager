using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Utilities;

namespace NETworkManager.Validators
{
    public class StringNotEndWithSemicolonValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return Regex.IsMatch(((string) value).Trim(), RegexHelper.StringNotEndWithSemicolon) ? ValidationResult.ValidResult : new ValidationResult(false, Strings.InputCannotEndWithSemicolon);
        }
    }
}
