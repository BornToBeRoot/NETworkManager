using System.Globalization;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;

namespace NETworkManager.Validators;

public class GroupNameValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var groupName = value as string;

        if (string.IsNullOrEmpty(groupName))
            return ValidationResult.ValidResult;

        if (groupName.StartsWith("~"))
            return new ValidationResult(false,
                string.Format(Strings.GroupNameCannotStartWithX, "~"));

        return ValidationResult.ValidResult;
    }
}