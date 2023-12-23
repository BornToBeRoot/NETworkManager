using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Profiles;

namespace NETworkManager.Validators;

public class ProfileFileUniqueValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        return ProfileManager.ProfileFiles.Any(x => x.Name == value as string)
            ? new ValidationResult(false, Strings.ProfileNameAlreadyExists)
            : ValidationResult.ValidResult;
    }
}