using System.Globalization;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.RDAP;

namespace NETworkManager.Validators;

/// <summary>
///     Validates an RDAP query (domain, nameserver, TLD, IP address/CIDR, AS number or entity handle) depending on the
///     query type.
///     With <see cref="RDAPQueryType.Auto" />, the input must be valid for the detected type.
/// </summary>
public class RDAPQueryValidator : ValidationRule
{
    public RDAPQueryDependencyObjectWrapper Wrapper { get; set; }

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var error = RDAPQueryParser.TryParse(Wrapper?.QueryType ?? RDAPQueryType.Auto, value as string, out _);

        return error == RDAPQueryParseError.None
            ? ValidationResult.ValidResult
            : new ValidationResult(false, GetErrorMessage(error));
    }

    /// <summary>
    ///     Gets the localized error message for an invalid RDAP query.
    /// </summary>
    /// <param name="error">Error returned by <see cref="RDAPQueryParser" />.</param>
    /// <returns>Localized error message.</returns>
    public static string GetErrorMessage(RDAPQueryParseError error)
    {
        return error switch
        {
            RDAPQueryParseError.InvalidQuery => Strings.EnterValidRDAPQuery,
            RDAPQueryParseError.InvalidDomain => Strings.EnterValidDomain,
            RDAPQueryParseError.InvalidNameserver => Strings.EnterValidNameserver,
            RDAPQueryParseError.InvalidTLD => Strings.EnterValidTLD,
            RDAPQueryParseError.InvalidIPAddress => Strings.EnterValidIPAddressOrCIDR,
            RDAPQueryParseError.InvalidASN => Strings.EnterValidASN,
            _ => Strings.EnterValidEntityHandle
        };
    }
}
