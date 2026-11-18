using System.Globalization;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Network;

namespace NETworkManager.Validators;

/// <summary>
///     Validates the name of a service type profile of the mDNS / DNS-SD tool, see
///     <see cref="MDNSServiceTypeProfile.IsValidName" /> (the name can be entered in the service type input).
/// </summary>
public class MDNSServiceTypeProfileNameValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        return MDNSServiceTypeProfile.IsValidName(value as string)
            ? ValidationResult.ValidResult
            : new ValidationResult(false, Strings.MDNSDiscovery_ServiceTypeProfileNameInvalid);
    }
}
