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
    ///     Create a new instance of <see cref="DNSResolverResult" /> with an error message.
    /// </summary>
    /// <param name="hasError">Indicates if the DNS resolver information retrieval has failed.</param>
    /// <param name="errorMessage">Error message if the DNS resolver information retrieval has failed.</param>
    public DNSResolverResult(bool hasError, string errorMessage)
    {
        HasError = hasError;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    ///     Create a new instance of <see cref="DNSResolverResult" /> with an error message and an error code.
    /// </summary>
    /// <param name="hasError">Indicates if the DNS resolver information retrieval has failed.</param>
    /// <param name="errorMessage">Error message if the DNS resolver information retrieval has failed.</param>
    /// <param name="errorCode">Error code if the DNS resolver information retrieval has failed.</param>
    public DNSResolverResult(bool hasError, string errorMessage, int errorCode) : this(hasError, errorMessage)
    {
        ErrorCode = errorCode;
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
}