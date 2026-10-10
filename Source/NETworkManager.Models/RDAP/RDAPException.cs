using System;

namespace NETworkManager.Models.RDAP;

/// <summary>
///     Kind of an RDAP error. Used by the UI to show a localized message.
/// </summary>
public enum RDAPErrorKind
{
    /// <summary>
    ///     No RDAP server is known for the query (not in the IANA bootstrap files).
    /// </summary>
    NoServer,

    /// <summary>
    ///     The IANA bootstrap files could not be downloaded and no cached copy exists.
    /// </summary>
    BootstrapUnavailable,

    /// <summary>
    ///     HTTP 404 - the server has no data for the query (RFC 7480 5.3).
    /// </summary>
    NotFound,

    /// <summary>
    ///     HTTP 400 - the server could not interpret the query (RFC 7480 5.4).
    /// </summary>
    BadRequest,

    /// <summary>
    ///     HTTP 429 - the query was rate limited (RFC 7480 5.5).
    /// </summary>
    RateLimited,

    /// <summary>
    ///     HTTP 501 - the server does not implement this query.
    /// </summary>
    NotImplemented,

    /// <summary>
    ///     Any other non-successful HTTP status code.
    /// </summary>
    HttpError,

    /// <summary>
    ///     A redirect would have downgraded the connection from HTTPS to HTTP.
    /// </summary>
    InsecureRedirect,

    /// <summary>
    ///     The referral to the registrar was not followed, because it does not use HTTPS.
    /// </summary>
    InsecureReferral,

    /// <summary>
    ///     The referral to the registrar was not followed, because it points to a local or private address.
    /// </summary>
    PrivateAddress,

    /// <summary>
    ///     Too many redirects or a redirect loop.
    /// </summary>
    TooManyRedirects,

    /// <summary>
    ///     The response is not a valid RDAP JSON object.
    /// </summary>
    InvalidResponse,

    /// <summary>
    ///     DNS, connection, TLS or timeout error for all known servers.
    /// </summary>
    Network
}

/// <summary>
///     Exception thrown by <see cref="RDAPClient" />.
/// </summary>
public class RDAPException : Exception
{
    public RDAPException(RDAPErrorKind kind, string message = null, Exception innerException = null) : base(
        message ?? kind.ToString(), innerException)
    {
        Kind = kind;
    }

    /// <summary>
    ///     Kind of the error.
    /// </summary>
    public RDAPErrorKind Kind { get; }

    /// <summary>
    ///     HTTP status code, if a server answered.
    /// </summary>
    public int? StatusCode { get; init; }

    /// <summary>
    ///     Title of the RDAP error response, if the server sent one.
    /// </summary>
    public string ErrorTitle { get; init; }

    /// <summary>
    ///     Description of the RDAP error response, if the server sent one.
    /// </summary>
    public string ErrorDescription { get; init; }

    /// <summary>
    ///     Time to wait before the next query (HTTP 429 with Retry-After header).
    /// </summary>
    public TimeSpan? RetryAfter { get; init; }

    /// <summary>
    ///     URL of the request that failed.
    /// </summary>
    public string RequestUrl { get; init; }
}
