using System.Globalization;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Network;
using NETworkManager.Settings;

namespace NETworkManager.Validators;

/// <summary>
///     Validates a ports input of the Port Scanner: ports, port ranges and names of port profiles separated by ";"
///     (e.g. "Webserver; 8080"), see <see cref="PortProfile.TryResolvePorts" />.
/// </summary>
public class PortsOrPortProfilesValidator : ValidationRule
{
    /// <summary>
    ///     An empty input is valid (e.g. the ports of a profile).
    /// </summary>
    public bool AllowEmpty { get; set; }

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (AllowEmpty && string.IsNullOrWhiteSpace(value as string))
            return ValidationResult.ValidResult;

        return PortProfile.TryResolvePorts(value as string, SettingsManager.Current.PortScanner_PortProfiles, out _)
            ? ValidationResult.ValidResult
            : new ValidationResult(false, Strings.EnterValidPortsOrPortProfiles);
    }
}
