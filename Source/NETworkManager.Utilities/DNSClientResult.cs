namespace NETworkManager.Utilities;

/// <summary>
/// Base class for the result from a <see cref="DNSClient"/> query.
/// </summary>
public abstract class DNSClientResult
{
    /// <summary>
    /// DNS server which was used for resolving the query.
    /// </summary>
    public string DNSServer { get; set; }

    /// <summary>
    /// Indicates if an error has occurred.
    /// </summary>
    public bool HasError { get; set; }

    /// <summary>
    /// Error message when an error has occurred.
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// Create an instance of <see cref="DNSClientResult"/>.
    /// </summary>
    public DNSClientResult()
    {
        
    }

    /// <summary>
    /// Create an instance of <see cref="DNSClientResult"/> with parameters.
    /// </summary>
    /// <param name="dnsServer">DNS server which was used for resolving the query.</param>
    public DNSClientResult(string dnsServer)
    {
        DNSServer = dnsServer;
    }

    /// <summary>
    /// Create an instance of <see cref="DNSClientResult"/> with parameters if an error has occurred.
    /// </summary>    
    /// <param name="hasError">Indicates if an error has occurred.</param>
    /// <param name="errorMessage">Error message when an error has occurred.</param>    
    public DNSClientResult(bool hasError, string errorMessage)
    {
        HasError = hasError;
        ErrorMessage = errorMessage;
    }
    
    /// <summary>
    /// Create an instance of <see cref="DNSClientResult"/> with parameters if an error has occurred.
    /// </summary>    
    /// <param name="hasError">Indicates if an error has occurred.</param>
    /// <param name="errorMessage">Error message when an error has occurred.</param>
    /// <param name="dnsServer">DNS server which was used for resolving the query.</param>
    public DNSClientResult(bool hasError, string errorMessage, string dnsServer) : this (dnsServer)
    {            
        HasError = hasError;
        ErrorMessage = errorMessage;
    }
}
