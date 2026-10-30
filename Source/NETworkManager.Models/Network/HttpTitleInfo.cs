namespace NETworkManager.Models.Network;

/// <summary>
///     Title of the web page of a host, see <see cref="HttpTitleResolver" />.
/// </summary>
/// <param name="URL">URL that answered (e.g. "https://192.168.1.1/").</param>
/// <param name="Title">Title of the web page, the <c>Server</c> header if the page has no title (may be empty).</param>
public sealed record HttpTitleInfo(string URL, string Title);
