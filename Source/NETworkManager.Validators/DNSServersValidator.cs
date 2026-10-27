using System.Globalization;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Network;
using NETworkManager.Settings;

namespace NETworkManager.Validators;

/// <summary>
///     Validates a DNS server input of DNS Lookup: names of DNS servers and DNS servers (IP address or hostname,
///     optional port) separated by ";" (e.g. "Google Public DNS; 9.9.9.9"), see <see cref="DNSServer.ResolveServers" />.
/// </summary>
public class DNSServersValidator : ValidationRule
{
    /// <summary>
    ///     An empty input is valid (e.g. the DNS server of a profile).
    /// </summary>
    public bool AllowEmpty { get; set; }

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (AllowEmpty && string.IsNullOrWhiteSpace(value as string))
            return ValidationResult.ValidResult;

        return DNSServer.GetInvalidEntries(value as string, SettingsManager.Current.DNSLookup_DNSServers).Count == 0
            ? ValidationResult.ValidResult
            : new ValidationResult(false, Strings.EnterValidDNSServersOrDNSServerProfiles);
    }
}
