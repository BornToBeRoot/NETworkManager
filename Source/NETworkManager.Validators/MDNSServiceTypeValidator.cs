using System.Globalization;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Network;
using NETworkManager.Settings;

namespace NETworkManager.Validators;

/// <summary>
///     Validates the service type input of the mDNS Discovery tool (input field and run command): <c>*</c>, service
///     types and / or names of service type profiles separated by ";" (e.g. "Printers; _ssh._tcp"), see
///     <see cref="MDNSServiceTypeProfile.TryResolve" />.
/// </summary>
public sealed class MDNSServiceTypeValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo) =>
        MDNSServiceTypeProfile.TryResolve(value as string, SettingsManager.Current.MDNSDiscovery_ServiceTypeProfiles,
            out _)
            ? ValidationResult.ValidResult
            : new ValidationResult(false, Strings.MDNSDiscovery_InvalidServiceType);
}
