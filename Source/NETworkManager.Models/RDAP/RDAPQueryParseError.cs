namespace NETworkManager.Models.RDAP;

/// <summary>
///     Errors returned by <see cref="RDAPQueryParser" /> if the input is not valid for the selected query type.
/// </summary>
public enum RDAPQueryParseError
{
    None,
    InvalidQuery,
    InvalidDomain,
    InvalidNameserver,
    InvalidTLD,
    InvalidIPAddress,
    InvalidASN,
    InvalidEntity
}
