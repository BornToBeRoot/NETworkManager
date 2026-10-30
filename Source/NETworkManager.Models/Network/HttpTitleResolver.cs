using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
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
public static partial class HttpTitleResolver
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

    private static readonly HttpClient Client = new(new SocketsHttpHandler
    {
        AllowAutoRedirect = false,
        UseCookies = false,
        UseProxy = false,
        Credentials = null,
        AutomaticDecompression = DecompressionMethods.All,
        PooledConnectionLifetime = TimeSpan.FromSeconds(30),
        SslOptions = new SslClientAuthenticationOptions
        {
            // Devices use self-signed certificates - only the title is read
            RemoteCertificateValidationCallback = (_, _, _, _) => true
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

    /// <summary>
    ///     Reads the title of the web page of a host from the first port that answers.
    /// </summary>
    /// <param name="ipAddress">IP address of the host.</param>
    /// <param name="ports">Open ports of the host to request, in the order to try them.</param>
    /// <param name="timeout">Timeout in milliseconds per request.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>URL and title, null if no port answered.</returns>
    public static async Task<HttpTitleInfo> ResolveAsync(IPAddress ipAddress, IEnumerable<int> ports,
        int timeout, CancellationToken cancellationToken)
    {
        foreach (var port in ports)
        {
            foreach (var scheme in GetSchemes(port))
            {
                var url = WebConsoleUrlHelper.CreateUrl(ipAddress.ToString(), port, scheme);

                var title = await GetTitleAsync(new Uri(url), timeout, cancellationToken).ConfigureAwait(false);

                if (title != null)
                    return new HttpTitleInfo(url, title);
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
    ///     Requests the web page and reads its title, follows redirects on the same host.
    /// </summary>
    /// <param name="uri">URL of the web page.</param>
    /// <param name="timeout">Timeout in milliseconds per request.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Title (or <c>Server</c> header, may be empty), null if the host didn't answer.</returns>
    private static async Task<string> GetTitleAsync(Uri uri, int timeout, CancellationToken cancellationToken)
    {
        try
        {
            for (var redirects = 0; ; redirects++)
            {
                using var timeoutCts = new CancellationTokenSource(timeout);
                using var linkedCts =
                    CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

                using var response = await Client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead,
                    linkedCts.Token).ConfigureAwait(false);

                var server = response.Headers.Server.ToString();

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

                    return server;
                }

                var content = await ReadContentAsync(response, linkedCts.Token).ConfigureAwait(false);

                var title = ParseTitle(Decode(content, response.Content.Headers.ContentType?.CharSet));

                return string.IsNullOrEmpty(title) ? server : title;
            }
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            // Connection refused / reset, TLS error, timeout, invalid response, ...
            return null;
        }
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
