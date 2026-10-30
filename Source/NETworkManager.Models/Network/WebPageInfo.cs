namespace NETworkManager.Models.Network;

/// <summary>
///     Web page of a host, see <see cref="WebPageResolver" />.
/// </summary>
/// <param name="URL">URL that answered (e.g. "https://192.168.1.1/").</param>
/// <param name="StatusCode">HTTP status code of the (last) response, e.g. 200.</param>
/// <param name="Title">Title of the web page, empty if it has none (e.g. a redirect to another host).</param>
/// <param name="Server">Value of the <c>Server</c> header (e.g. "nginx"), empty if there is none.</param>
/// <param name="PoweredBy">Value of the <c>X-Powered-By</c> header (e.g. "PHP/8.2"), empty if there is none.</param>
public sealed record WebPageInfo(string URL, int StatusCode, string Title, string Server, string PoweredBy);
