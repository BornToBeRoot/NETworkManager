using System;
using System.Net.Http;
using System.Threading.Tasks;
using NETworkManager.Localization.Resources;

namespace NETworkManager;

/// <summary>
/// Localized message for a failed web request of a dashboard widget (instead of the English exception text of .NET,
/// which is logged).
/// </summary>
public static class WebRequestErrorMessage
{
    /// <summary>
    /// Gets the message for a failed request, with the hint to check the network connection.
    /// </summary>
    /// <param name="host">Host of the request (e.g. <c>ip-api.com</c>).</param>
    /// <param name="timeout">Timeout of the request (shown if it did not respond in time).</param>
    /// <param name="ex">Error of the request: <see cref="TimeoutException" /> (also as the inner exception of the
    ///     <see cref="TaskCanceledException" /> of <see cref="HttpClient.Timeout" />), <see cref="HttpRequestException" />
    ///     with a status code (HTTP error) or any other error (no connection).</param>
    /// <returns>Message in one paragraph.</returns>
    public static string Get(string host, TimeSpan timeout, Exception ex)
    {
        var message = ex switch
        {
            TimeoutException or { InnerException: TimeoutException } => string.Format(
                Strings.XXXDidNotRespondWithinXXXSecondsMessage, host, timeout.TotalSeconds),
            HttpRequestException { StatusCode: not null } httpEx => string.Format(Strings.CouldNotConnectToXXXReasonXXX,
                host, $"{Strings.HTTPStatusCode} {(int)httpEx.StatusCode}"),
            _ => string.Format(Strings.CouldNotConnectToXXXMessage, host)
        };

        // One message with its hint (a new line would look like a second message), wraps if the widget is narrow
        return $"{message} {Strings.CheckNetworkConnectionTryAgainMessage}";
    }
}
