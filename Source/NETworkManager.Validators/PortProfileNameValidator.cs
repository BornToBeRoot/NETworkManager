using System.Globalization;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Network;

namespace NETworkManager.Validators;

/// <summary>
///     Validates the name of a port profile, see <see cref="PortProfile.IsValidName" /> (the name can be entered in the
///     ports input of the Port Scanner).
/// </summary>
public class PortProfileNameValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        return PortProfile.IsValidName(value as string)
            ? ValidationResult.ValidResult
            : new ValidationResult(false, Strings.ErrorMessage_PortProfileNameInvalid);
    }
}
