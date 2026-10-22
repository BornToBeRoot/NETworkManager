using NETworkManager.Localization.Resources;
using NETworkManager.Utilities;
using System.DirectoryServices.ActiveDirectory;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace NETworkManager.Validators;

public class RemoteDesktopHostnameAndPortValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var hostnameAndPort = (string)value;

        if (hostnameAndPort.Contains(':'))
        {
            var hostnameAndPortValues = hostnameAndPort.Split(':');

            if (IsHost(hostnameAndPortValues[0]) &&
                !string.IsNullOrEmpty(hostnameAndPortValues[1]) &&
                Regex.IsMatch(hostnameAndPortValues[1], RegexHelper.PortRegex))
                return ValidationResult.ValidResult;

            return new ValidationResult(false, Strings.EnterValidHostnameAndPort);
        }

        return IsHost(hostnameAndPort)
            ? ValidationResult.ValidResult
            : new ValidationResult(false, Strings.EnterValidHostname);
    }

    /// <summary>
    ///     Checks if the value is an IPv4 address or a hostname (a hostname must contain a letter, so IPv4 addresses are
    ///     checked separately).
    /// </summary>
    private static bool IsHost(string value)
    {
        return RegexHelper.IPv4AddressRegex().IsMatch(value) || RegexHelper.HostnameOrDomainRegex().IsMatch(value);
    }
}