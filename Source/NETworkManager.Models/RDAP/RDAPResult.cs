namespace NETworkManager.Models.RDAP;

/// <summary>
///     Result of an RDAP query.
/// </summary>
public class RDAPResult
{
    /// <summary>
    ///     Query that was executed.
    /// </summary>
    public RDAPQuery Query { get; init; }

    /// <summary>
    ///     Final URL that returned the response (after redirects).
    /// </summary>
    public string RequestUrl { get; init; }

    /// <summary>
    ///     Response body exactly as sent by the server.
    /// </summary>
    public string RawJson { get; init; }

    /// <summary>
    ///     Response body indented for display.
    /// </summary>
    public string FormattedJson { get; init; }

    /// <summary>
    ///     Parsed response.
    /// </summary>
    public RDAPResponse Response { get; init; }

    /// <summary>
    ///     Result of the registrar referral, if it was requested and found.
    /// </summary>
    public RDAPResult Referral { get; set; }

    /// <summary>
    ///     Error of the registrar referral, if it was found but could not be queried.
    /// </summary>
    public RDAPException ReferralError { get; set; }
}
