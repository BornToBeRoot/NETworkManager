namespace NETworkManager.Utilities;

/// <summary>
///     Class is used to store the <see cref="string" /> result from a <see cref="DNSClient" /> query.
/// </summary>
public class DNSClientResultString : DNSClientResult
{
    /// <summary>
    ///     Create an instance of <see cref="DNSClientResultString" /> with parameters.
    /// </summary>
    /// <param name="value">Query result as <see cref="string" />.</param>
    /// <param name="dnsServer">DNS server which was used for resolving the query.</param>
    public DNSClientResultString(string value, string dnsServer) : base(dnsServer)
    {
        Value = value;
    }

    /// <summary>
    ///     Create an instance of <see cref="DNSClientResultString" /> with parameters if an error has occurred.
    /// </summary>
    /// <param name="hasError">Indicates if an error has occurred.</param>
    /// <param name="errorMessage">Error message when an error has occurred.</param>
    public DNSClientResultString(bool hasError, string errorMessage) : base(hasError, errorMessage)
    {
    }

    /// <summary>
    ///     Create an instance of <see cref="DNSClientResultString" /> with parameters if an error has occurred.
    /// </summary>
    /// <param name="hasError">Indicates if an error has occurred.</param>
    /// <param name="errorMessage">Error message when an error has occurred.</param>
    /// <param name="dnsServer">DNS server which was used for resolving the query.</param>
    public DNSClientResultString(bool hasError, string errorMessage, string dnsServer) : base(hasError, errorMessage,
        dnsServer)
    {
    }

    /// <summary>
    ///     Query result as <see cref="string" />.
    /// </summary>
    public string Value { get; set; }
}