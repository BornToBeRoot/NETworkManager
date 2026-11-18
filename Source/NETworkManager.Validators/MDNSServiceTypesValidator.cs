using System.Globalization;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Network;

namespace NETworkManager.Validators;

/// <summary>
///     Validates the service types of a service type profile of the mDNS / DNS-SD tool: service types separated by
///     ";" (e.g. "_ipp._tcp; _ipps._tcp"), see <see cref="MDNSServiceTypeProfile.TryParseServiceTypes" />.
/// </summary>
public class MDNSServiceTypesValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        return MDNSServiceTypeProfile.TryParseServiceTypes(value as string, out _)
            ? ValidationResult.ValidResult
            : new ValidationResult(false, Strings.MDNSDiscovery_EnterValidServiceTypes);
    }
}
