namespace NETworkManager.Models.Network;

/// <summary>
///     Web page of a host, see <see cref="WebPageResolver" />.
/// </summary>
/// <param name="URL">
///     URL that was requested on the port that answered (e.g. "https://192.168.1.1/") - also after a redirect on the
///     same host, so the Web Console opens the port (and scheme) that answered; the other values are of the last
///     response.
/// </param>
/// <param name="StatusCode">HTTP status code of the (last) response, e.g. 200.</param>
/// <param name="Title">Title of the web page, empty if it has none (e.g. a redirect to another host).</param>
/// <param name="Server">Value of the <c>Server</c> header (e.g. "nginx"), empty if there is none.</param>
/// <param name="PoweredBy">Value of the <c>X-Powered-By</c> header (e.g. "PHP/8.2"), empty if there is none.</param>
/// <param name="Realm">
///     Realm of the <c>WWW-Authenticate</c> header (login prompt, e.g. "RT-AC68U"), empty if there is none.
/// </param>
/// <param name="RedirectURL">
///     Target of a redirect to another host (e.g. "https://unifi.local:8443/"), empty if there is none.
/// </param>
/// <param name="Certificate">
///     TLS certificate of the web server (of the last response), null for http (also after a redirect from https to
///     http).
/// </param>
public sealed record WebPageInfo(
    string URL,
    int StatusCode,
    string Title,
    string Server,
    string PoweredBy,
    string Realm,
    string RedirectURL,
    WebCertificateInfo Certificate);
