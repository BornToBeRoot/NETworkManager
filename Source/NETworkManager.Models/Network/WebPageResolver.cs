using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network;

/// <summary>
///     Reads the title of the web page of a host (<c>GET /</c> on an open web port): no cookies, no credentials, no
///     proxy; certificate errors are accepted (only the title is read, nothing is sent). The request appears in the
///     log of the web server.
/// </summary>
public static partial class WebPageResolver
{
    /// <summary>
    ///     Maximum number of bytes read from the web page.
    /// </summary>
    private const int MaxContentLength = 64 * 1024;

    /// <summary>
    ///     Maximum length of the title.
    /// </summary>
    private const int MaxTitleLength = 200;

    /// <summary>
    ///     Maximum number of redirects followed (only on the same host).
    /// </summary>
    private const int MaxRedirects = 3;

    /// <summary>
    ///     Key of the TLS certificate of the web server in the options of a request, set by the certificate callback of
    ///     the handler (it gets the request, unlike the callback of the <c>SslStream</c>).
    /// </summary>
    private static readonly HttpRequestOptionsKey<WebCertificateInfo> CertificateKey =
        new("NETworkManager.Certificate");

    private static readonly HttpClient Client = new(new HttpClientHandler
    {
        AllowAutoRedirect = false,
        UseCookies = false,
        UseProxy = false,
        Credentials = null,
        AutomaticDecompression = DecompressionMethods.All,
        // Devices use self-signed certificates - accepted (nothing is sent), the certificate is read
        ServerCertificateCustomValidationCallback = (request, certificate, _, _) =>
        {
            if (certificate != null)
                request.Options.Set(CertificateKey, WebCertificateInfo.FromCertificate(certificate));

            return true;
        }
    })
    {
        // The timeout of each request is set with its cancellation token
        Timeout = Timeout.InfiniteTimeSpan,
        DefaultRequestHeaders = { { "User-Agent", "NETworkManager" } }
    };

    [GeneratedRegex(@"<title[^>]*>(.*?)</title\s*>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex TitleRegex();

    [GeneratedRegex(@"<meta[^>]+charset\s*=\s*[""']?\s*([a-z0-9_\-]+)", RegexOptions.IgnoreCase)]
    private static partial Regex MetaCharsetRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"realm\s*=\s*(?:""(?<quoted>[^""]*)""|(?<token>[^\s,]+))", RegexOptions.IgnoreCase)]
    private static partial Regex RealmRegex();

    /// <summary>
    ///     Reads the title of the web page of a host from the first port that answers.
    /// </summary>
    /// <param name="ipAddress">IP address of the host.</param>
    /// <param name="ports">Open ports of the host to request, in the order to try them.</param>
    /// <param name="timeout">Timeout in milliseconds per request.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>URL and title, null if no port answered.</returns>
    public static async Task<WebPageInfo> ResolveAsync(IPAddress ipAddress, IEnumerable<int> ports,
        int timeout, CancellationToken cancellationToken)
    {
        foreach (var port in ports)
        {
            foreach (var scheme in GetSchemes(port))
            {
                var url = WebConsoleUrlHelper.CreateUrl(ipAddress.ToString(), port, scheme);

                var info = await GetWebPageAsync(url, timeout, cancellationToken).ConfigureAwait(false);

                if (info != null)
                    return info;
            }
        }

        return null;
    }

    /// <summary>
    ///     Gets the schemes to try for a port: http for 80 / 8080, https and then http for any other port (a TLS
    ///     handshake with a plain http server fails right away).
    /// </summary>
    /// <param name="port">Port number.</param>
    /// <returns>Schemes in the order to try them.</returns>
    public static string[] GetSchemes(int port)
    {
        return port is 80 or 8080 ? ["http"] : ["https", "http"];
    }

    /// <summary>
    ///     Requests the web page and reads its title and headers, follows redirects on the same host.
    /// </summary>
    /// <param name="url">URL of the web page.</param>
    /// <param name="timeout">Timeout in milliseconds per request.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Web page (title, status code, headers), null if the host didn't answer.</returns>
    private static async Task<WebPageInfo> GetWebPageAsync(string url, int timeout,
        CancellationToken cancellationToken)
    {
        var uri = new Uri(url);

        // Certificate of the last https response (e.g. after a redirect from http to https)
        WebCertificateInfo certificate = null;

        try
        {
            for (var redirects = 0; ; redirects++)
            {
                using var timeoutCts = new CancellationTokenSource(timeout);
                using var linkedCts =
                    CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

                // A new connection per request (no pooled one), so the certificate callback runs for every request
                using var request = new HttpRequestMessage(HttpMethod.Get, uri);
                request.Headers.ConnectionClose = true;

                using var response = await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead,
                    linkedCts.Token).ConfigureAwait(false);

                if (request.Options.TryGetValue(CertificateKey, out var requestCertificate))
                    certificate = requestCertificate;

                var statusCode = (int)response.StatusCode;
                var server = CleanHeaderValue(response.Headers.Server.ToString());
                var poweredBy = CleanHeaderValue(response.Headers.TryGetValues("X-Powered-By", out var values)
                    ? string.Join(", ", values)
                    : string.Empty);

                // Login prompt (401) of routers, cameras, printers - the realm often names the model
                var realm = ParseRealm(response.Headers.WwwAuthenticate.Select(x => x.ToString()));

                // Redirect on the same host (e.g. http -> https, / -> /login.html)
                if ((int)response.StatusCode is >= 300 and < 400 && response.Headers.Location != null &&
                    redirects < MaxRedirects)
                {
                    var location = response.Headers.Location.IsAbsoluteUri
                        ? response.Headers.Location
                        : new Uri(uri, response.Headers.Location);

                    if (location.Scheme is "http" or "https" &&
                        string.Equals(location.Host, uri.Host, StringComparison.OrdinalIgnoreCase))
                    {
                        uri = location;
                        continue;
                    }

                    // Redirect to another host - only the headers and the target
                    return new WebPageInfo(url, statusCode, string.Empty, server, poweredBy, realm,
                        CleanHeaderValue(location.ToString()), certificate);
                }

                var content = await ReadContentAsync(response, linkedCts.Token).ConfigureAwait(false);

                var title = ParseTitle(Decode(content, response.Content.Headers.ContentType?.CharSet));

                return new WebPageInfo(url, statusCode, title, server, poweredBy, realm, string.Empty, certificate);
            }
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            // Connection refused / reset, TLS error, timeout, invalid response, ...
            return null;
        }
    }

    /// <summary>
    ///     Gets the realm of the <c>WWW-Authenticate</c> header(s) of a response (e.g. <c>Basic realm="RT-AC68U"</c>,
    ///     <c>Digest realm="IP Camera", nonce="..."</c>).
    /// </summary>
    /// <param name="headerValues">Values of the <c>WWW-Authenticate</c> header(s).</param>
    /// <returns>First realm (cleaned, see <see cref="CleanHeaderValue" />), empty if there is none.</returns>
    public static string ParseRealm(IEnumerable<string> headerValues)
    {
        foreach (var headerValue in headerValues ?? [])
        {
            var match = RealmRegex().Match(headerValue ?? string.Empty);

            if (!match.Success)
                continue;

            var realm = CleanHeaderValue(match.Groups["quoted"].Success
                ? match.Groups["quoted"].Value
                : match.Groups["token"].Value);

            if (!string.IsNullOrEmpty(realm))
                return realm;
        }

        return string.Empty;
    }

    /// <summary>
    ///     Cleans a header value received from the host for display: control characters removed, whitespace collapsed,
    ///     at most <see cref="MaxTitleLength" /> characters.
    /// </summary>
    /// <param name="value">Header value.</param>
    /// <returns>Cleaned value.</returns>
    public static string CleanHeaderValue(string value)
    {
        var withoutControlCharacters = new string([.. (value ?? string.Empty).Where(x => !char.IsControl(x))]);
        var cleaned = WhitespaceRegex().Replace(withoutControlCharacters, " ").Trim();

        return cleaned.Length > MaxTitleLength ? cleaned[..MaxTitleLength] + "..." : cleaned;
    }

    /// <summary>
    ///     Reads the content of the response, at most <see cref="MaxContentLength" /> bytes.
    /// </summary>
    private static async Task<byte[]> ReadContentAsync(HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

        var buffer = new byte[MaxContentLength];
        var length = 0;

        while (length < buffer.Length)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(length), cancellationToken).ConfigureAwait(false);

            if (read == 0)
                break;

            length += read;
        }

        return buffer[..length];
    }

    /// <summary>
    ///     Decodes the content of a web page: charset of the <c>Content-Type</c> header, of a <c>meta</c> tag or
    ///     UTF-8.
    /// </summary>
    /// <param name="content">Content of the web page.</param>
    /// <param name="charset">Charset of the <c>Content-Type</c> header, null if there is none.</param>
    /// <returns>Decoded content.</returns>
    public static string Decode(byte[] content, string charset)
    {
        // The meta tag is ASCII in all common charsets
        charset ??= MetaCharsetRegex().Match(Encoding.ASCII.GetString(content)) is { Success: true } match
            ? match.Groups[1].Value
            : null;

        Encoding encoding;

        try
        {
            encoding = string.IsNullOrEmpty(charset) ? Encoding.UTF8 : Encoding.GetEncoding(charset.Trim('"'));
        }
        catch (ArgumentException)
        {
            encoding = Encoding.UTF8;
        }

        return encoding.GetString(content);
    }

    /// <summary>
    ///     Gets the title of a web page: content of the first <c>title</c> element, HTML entities decoded, whitespace
    ///     collapsed, at most <see cref="MaxTitleLength" /> characters.
    /// </summary>
    /// <param name="html">HTML of the web page.</param>
    /// <returns>Title, empty if there is none.</returns>
    public static string ParseTitle(string html)
    {
        var match = TitleRegex().Match(html ?? string.Empty);

        if (!match.Success)
            return string.Empty;

        var title = WhitespaceRegex().Replace(WebUtility.HtmlDecode(match.Groups[1].Value), " ").Trim();

        // Control characters (e.g. from a broken page) are not shown
        title = new string([.. title.Where(x => !char.IsControl(x))]);

        return title.Length > MaxTitleLength ? title[..MaxTitleLength] + "..." : title;
    }
}
