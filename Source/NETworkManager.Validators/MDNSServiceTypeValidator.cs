using System.Globalization;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Utilities;

namespace NETworkManager.Validators;

public sealed class MDNSServiceTypeValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo) =>
        MDNSServiceType.TryNormalize(value as string, out _)
            ? ValidationResult.ValidResult
            : new ValidationResult(false, Strings.MDNSDiscovery_InvalidServiceType);
}
