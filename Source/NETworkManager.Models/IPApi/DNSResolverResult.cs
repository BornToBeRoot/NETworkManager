using System;
using System.Net.Http;

namespace NETworkManager.Models.IPApi;

/// <summary>
///     Class contains the result of the DNS resolver information retrieval.
/// </summary>
public class DNSResolverResult
{
    /// <summary>
    ///     Creates a new instance of <see cref="DNSResolverResult" /> with the DNS resolver information.
    /// </summary>
    /// <param name="info">DNS resolver information retrieved from the API.</param>
    public DNSResolverResult(DNSResolverInfo info)
    {
        Info = info;
    }

    /// <summary>
    ///     Create a new instance of <see cref="DNSResolverResult" /> for a failed request.
    /// </summary>
    /// <param name="error">
    ///     Error of the request (<see cref="HttpRequestException" /> with the status code for an HTTP error).
    /// </param>
    public DNSResolverResult(Exception error)
    {
        HasError = true;
        Error = error;
        ErrorMessage = error.Message;
        ErrorCode = error is HttpRequestException { StatusCode: not null } httpEx ? (int)httpEx.StatusCode : 0;
    }

    /// <summary>
    ///     DNS resolver information retrieved from the API.
    /// </summary>
    public DNSResolverInfo Info { get; }

    /// <summary>
    ///     Indicates if the DNS resolver information retrieval has failed.
    /// </summary>
    public bool HasError { get; set; }

    /// <summary>
    ///     Error message if the DNS resolver information retrieval has failed.
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    ///     Error code if the DNS resolver information retrieval has failed.
    /// </summary>
    public int ErrorCode { get; set; }

    /// <summary>
    ///     Error of the request, mapped to a localized message by the view model.
    /// </summary>
    public Exception Error { get; set; }
}