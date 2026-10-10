using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using NETworkManager.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NETworkManager.Models.RDAP;

/// <summary>
///     Downloads, caches and queries the IANA RDAP bootstrap files (RFC 9224, RFC 8521) to find the RDAP server(s)
///     for a query.
/// </summary>
public class RDAPBootstrapService : SingletonBase<RDAPBootstrapService>
{
    #region Variables

    private static readonly ILog Log = LogManager.GetLogger(typeof(RDAPBootstrapService));

    /// <summary>
    ///     Base URL of the IANA bootstrap files.
    /// </summary>
    public const string BootstrapBaseUrl = "https://data.iana.org/rdap/";

    /// <summary>
    ///     RDAP server of the IANA root zone, which answers queries for TLDs.
    /// </summary>
    public const string IANARootZoneUrl = "https://rdap.iana.org/";

    /// <summary>
    ///     Fallback server for IP addresses and AS numbers that are not in the bootstrap files (e.g. private or
    ///     reserved ranges). ARIN answers them and redirects to the responsible RIR, if any.
    /// </summary>
    public const string NumberFallbackUrl = "https://rdap.arin.net/registry/";

    private const string CacheInfoFileName = "cache-info.json";
    private const string StealthEndpointsResource = "NETworkManager.Models.Resources.RDAPStealthEndpoints.json";

    /// <summary>
    ///     Minimum interval for the automatic refresh. data.iana.org sends max-age=86400 (one day), but the files rarely
    ///     change, so they are only checked once a week. "Update now" in the settings checks immediately.
    /// </summary>
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromDays(7);

    /// <summary>
    ///     Wait time before the next automatic attempt after a failed update.
    /// </summary>
    private static readonly TimeSpan RetryInterval = TimeSpan.FromHours(1);

    /// <summary>
    ///     Number of failed automatic attempts after which a cached file is used until the next regular
    ///     <see cref="RefreshInterval" /> (e.g. if data.iana.org is blocked). "Update now" always checks.
    /// </summary>
    private const int MaxFailedAttempts = 3;

    private readonly HttpClient _client;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private string _cacheDirectory;
    private List<RDAPBootstrapCacheInfo> _cacheInfos;

    private Dictionary<string, List<string>> _dns;
    private List<(IPNetwork Network, List<string> Urls)> _ipv4;
    private List<(IPNetwork Network, List<string> Urls)> _ipv6;
    private List<(uint Start, uint End, List<string> Urls)> _asn;
    private Dictionary<string, List<string>> _objectTags;
    private readonly Dictionary<string, List<string>> _stealthEndpoints;

    /// <summary>
    ///     Occurs when the status of a cached file has changed (e.g. after an update).
    /// </summary>
    public event EventHandler CacheInfoChanged;

    #endregion

    #region Constructor

    public RDAPBootstrapService()
    {
        _client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.All })
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        _client.DefaultRequestHeaders.UserAgent.ParseAdd("NETworkManager");

        _stealthEndpoints = LoadStealthEndpoints();
    }

    #endregion

    #region Public methods

    /// <summary>
    ///     Gets the file name of a bootstrap file.
    /// </summary>
    /// <param name="type">Type of the bootstrap file.</param>
    /// <returns>File name (e.g. dns.json).</returns>
    public static string GetFileName(RDAPBootstrapFileType type)
    {
        return type switch
        {
            RDAPBootstrapFileType.DNS => "dns.json",
            RDAPBootstrapFileType.IPv4 => "ipv4.json",
            RDAPBootstrapFileType.IPv6 => "ipv6.json",
            RDAPBootstrapFileType.ASN => "asn.json",
            RDAPBootstrapFileType.ObjectTags => "object-tags.json",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    /// <summary>
    ///     Gets the status of all cached bootstrap files.
    /// </summary>
    /// <param name="cacheDirectory">Directory where the bootstrap files are cached.</param>
    /// <returns>Status of each bootstrap file.</returns>
    public async Task<IReadOnlyList<RDAPBootstrapCacheInfo>> GetCacheInfosAsync(string cacheDirectory)
    {
        await _semaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            LoadCacheInfos(cacheDirectory);

            return _cacheInfos.Select(Clone).ToList();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    ///     Makes sure the bootstrap files are cached, up to date (according to the HTTP caching headers) and loaded.
    ///     If a download fails, the cached copy is used.
    /// </summary>
    /// <param name="cacheDirectory">Directory where the bootstrap files are cached.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task EnsureUpToDateAsync(string cacheDirectory, CancellationToken cancellationToken = default)
    {
        return UpdateAsync(cacheDirectory, false, cancellationToken);
    }

    /// <summary>
    ///     Checks all bootstrap files for updates, regardless of their cache lifetime.
    /// </summary>
    /// <param name="cacheDirectory">Directory where the bootstrap files are cached.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="RDAPException">If one or more files could not be updated.</exception>
    public Task UpdateNowAsync(string cacheDirectory, CancellationToken cancellationToken = default)
    {
        return UpdateAsync(cacheDirectory, true, cancellationToken);
    }

    /// <summary>
    ///     Gets the base RDAP URLs for a query, in the order they should be tried. Call
    ///     <see cref="EnsureUpToDateAsync" /> first.
    /// </summary>
    /// <param name="query">Query to get the base URLs for.</param>
    /// <returns>Base URLs, each ending with "/".</returns>
    /// <exception cref="RDAPException">If no server is known for the query.</exception>
    public List<string> GetBaseUrls(RDAPQuery query)
    {
        var urls = query.Type switch
        {
            RDAPQueryType.TLD => [IANARootZoneUrl],
            RDAPQueryType.Domain => FindDomain(query.DomainName),
            RDAPQueryType.IPAddress => FindIPAddress(query.IPAddress, query.PrefixLength),
            RDAPQueryType.ASN => FindASN(query.ASN),
            RDAPQueryType.Entity => FindEntity(query.Handle),
            _ => null
        };

        if (urls == null || urls.Count == 0)
            throw new RDAPException(RDAPErrorKind.NoServer);

        return OrderUrls(urls);
    }

    #endregion

    #region Lookup

    /// <summary>
    ///     Label-wise longest match, from right to left (RFC 9224 4). The IANA bootstrap is checked first, then the
    ///     table of registries that are not (yet) listed by IANA.
    /// </summary>
    private List<string> FindDomain(string domain)
    {
        var dns = _dns ?? throw CreateBootstrapUnavailableException(RDAPBootstrapFileType.DNS);

        var labels = domain.Split('.');

        foreach (var registry in new[] { dns, _stealthEndpoints })
        {
            for (var i = 0; i < labels.Length; i++)
            {
                if (registry.TryGetValue(string.Join('.', labels[i..]), out var urls))
                    return urls;
            }

            // The root of the domain name space is specified as "" (RFC 9224 4).
            if (registry.TryGetValue(string.Empty, out var rootUrls))
                return rootUrls;
        }

        return null;
    }

    /// <summary>
    ///     Longest prefix match (RFC 9224 5.1, 5.2). Falls back to ARIN for addresses that are not in the bootstrap.
    /// </summary>
    private List<string> FindIPAddress(IPAddress ipAddress, int? prefixLength)
    {
        var isIPv4 = ipAddress.AddressFamily == AddressFamily.InterNetwork;

        var registry = (isIPv4 ? _ipv4 : _ipv6) ??
                       throw CreateBootstrapUnavailableException(isIPv4
                           ? RDAPBootstrapFileType.IPv4
                           : RDAPBootstrapFileType.IPv6);

        var length = prefixLength ?? (isIPv4 ? 32 : 128);

        var match = registry
            .Where(x => x.Network.PrefixLength <= length && x.Network.Contains(ipAddress))
            .OrderByDescending(x => x.Network.PrefixLength)
            .Select(x => x.Urls)
            .FirstOrDefault();

        return match ?? [NumberFallbackUrl];
    }

    /// <summary>
    ///     Range match (RFC 9224 5.3). Falls back to ARIN for AS numbers that are not in the bootstrap.
    /// </summary>
    private List<string> FindASN(uint asn)
    {
        var registry = _asn ?? throw CreateBootstrapUnavailableException(RDAPBootstrapFileType.ASN);

        var match = registry.FirstOrDefault(x => asn >= x.Start && asn <= x.End).Urls;

        return match ?? [NumberFallbackUrl];
    }

    /// <summary>
    ///     Object tag lookup (RFC 8521): the service provider tag follows the last hyphen of the handle.
    /// </summary>
    private List<string> FindEntity(string handle)
    {
        var objectTags = _objectTags ?? throw CreateBootstrapUnavailableException(RDAPBootstrapFileType.ObjectTags);

        var index = handle.LastIndexOf('-');

        if (index <= 0 || index == handle.Length - 1)
            return null;

        return objectTags.GetValueOrDefault(handle[(index + 1)..]);
    }

    /// <summary>
    ///     Creates the exception for a bootstrap file that is not available, including the reason of the last update.
    /// </summary>
    private RDAPException CreateBootstrapUnavailableException(RDAPBootstrapFileType type)
    {
        var info = _cacheInfos?.FirstOrDefault(x => x.Type == type);

        return new RDAPException(RDAPErrorKind.BootstrapUnavailable,
            string.IsNullOrEmpty(info?.LastError) ? GetFileName(type) : $"{GetFileName(type)}: {info.LastError}");
    }

    /// <summary>
    ///     Orders the base URLs: HTTPS is preferred and tried first (RFC 9224 3). If only HTTP is listed, the same URL
    ///     is tried over HTTPS first (some registries serve HTTPS without listing it), then over HTTP. HTTP is never
    ///     used if the registry lists an HTTPS URL.
    /// </summary>
    private static List<string> OrderUrls(IEnumerable<string> urls)
    {
        var uris = urls
            .Select(x => Uri.TryCreate(x.EndsWith('/') ? x : x + "/", UriKind.Absolute, out var uri) ? uri : null)
            .Where(x => x != null && (x.Scheme == Uri.UriSchemeHttps || x.Scheme == Uri.UriSchemeHttp))
            .ToList();

        var https = uris.Where(x => x.Scheme == Uri.UriSchemeHttps).Select(x => x.AbsoluteUri).ToList();

        if (https.Count > 0)
            return https;

        var result = uris
            .Select(x => new UriBuilder(x) { Scheme = Uri.UriSchemeHttps, Port = -1 }.Uri.AbsoluteUri)
            .ToList();

        result.AddRange(uris.Select(x => x.AbsoluteUri));

        return result.Distinct().ToList();
    }

    #endregion

    #region Download and cache

    private async Task UpdateAsync(string cacheDirectory, bool force, CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

        var errors = new List<string>();

        try
        {
            LoadCacheInfos(cacheDirectory);

            Directory.CreateDirectory(_cacheDirectory);

            var attempted = false;
            var contentChanged = false;
            Exception networkError = null;

            foreach (var info in _cacheInfos)
            {
                // "Expires" is the time of the next check, after a successful update as well as after a failure.
                if (!force && info.Expires > DateTime.Now)
                    continue;

                attempted = true;

                // All files are on the same server. After a network error (DNS, connection, timeout), the other files
                // are not tried, so a blocked server costs at most one timeout per update.
                if (networkError != null)
                {
                    RecordFailure(info, networkError);
                    errors.Add($"{info.FileName}: {networkError.Message}");
                    continue;
                }

                var path = Path.Combine(_cacheDirectory, info.FileName);

                try
                {
                    contentChanged |= await DownloadAsync(info, path, File.Exists(path), cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is not OperationCanceledException ||
                                           !cancellationToken.IsCancellationRequested)
                {
                    Log.Warn($"Could not update RDAP bootstrap file {info.FileName}.", ex);

                    if (IsNetworkError(ex))
                        networkError = ex;

                    RecordFailure(info, ex);
                    errors.Add($"{info.FileName}: {ex.Message}");
                }
            }

            // The cached copy is kept and used, if an update fails.
            if (contentChanged || _dns == null)
                LoadRegistries();

            if (attempted)
            {
                SaveCacheInfos();
                CacheInfoChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        finally
        {
            _semaphore.Release();
        }

        if (force && errors.Count > 0)
            throw new RDAPException(RDAPErrorKind.BootstrapUnavailable, string.Join(Environment.NewLine, errors));
    }

    /// <summary>
    ///     Records a failed update. A cached file is retried after <see cref="RetryInterval" />, at most
    ///     <see cref="MaxFailedAttempts" /> times, then after the regular <see cref="RefreshInterval" />. A missing file
    ///     is retried after <see cref="RetryInterval" />, since the query type cannot be used without it.
    /// </summary>
    private void RecordFailure(RDAPBootstrapCacheInfo info, Exception ex)
    {
        info.LastError = ex.Message;
        info.FailedAttempts++;

        var exists = File.Exists(Path.Combine(_cacheDirectory, info.FileName));

        info.Expires = DateTime.Now + (exists && info.FailedAttempts >= MaxFailedAttempts
            ? RefreshInterval
            : RetryInterval);
    }

    /// <summary>
    ///     Network errors (DNS, connection, TLS, timeout) as opposed to an HTTP error status or invalid content.
    /// </summary>
    private static bool IsNetworkError(Exception ex)
    {
        return ex is TaskCanceledException || ex is HttpRequestException { StatusCode: null };
    }

    /// <summary>
    ///     Downloads a bootstrap file with a conditional request (If-None-Match / If-Modified-Since).
    /// </summary>
    /// <returns>True if the file content changed.</returns>
    private async Task<bool> DownloadAsync(RDAPBootstrapCacheInfo info, string path, bool exists,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, BootstrapBaseUrl + info.FileName);

        // Only send a conditional request if the cache entry is complete. Otherwise, download the file again, so an
        // inconsistent entry (e.g. from an interrupted update) is repaired.
        if (exists && info.Downloaded != null && info.Size > 0)
        {
            if (!string.IsNullOrEmpty(info.ETag) &&
                System.Net.Http.Headers.EntityTagHeaderValue.TryParse(info.ETag, out var eTag))
                request.Headers.IfNoneMatch.Add(eTag);

            if (info.LastModified != null)
                request.Headers.IfModifiedSince = info.LastModified;
        }

        using var response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);

        info.LastChecked = DateTime.Now;
        var maxAge = GetMaxAge(response);

        info.Expires = DateTime.Now + (maxAge > RefreshInterval ? maxAge.Value : RefreshInterval);

        if (response.StatusCode == HttpStatusCode.NotModified && exists)
        {
            info.LastError = null;
            info.FailedAttempts = 0;

            return false;
        }

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);

        // Validate the file before replacing the cached copy. Dates are kept as strings (no culture conversion).
        using var reader = new JsonTextReader(new StringReader(System.Text.Encoding.UTF8.GetString(content)))
        {
            DateParseHandling = DateParseHandling.None
        };

        var json = JObject.Load(reader);

        if (json["services"] is not JArray)
            throw new InvalidDataException($"{info.FileName} does not contain any services.");

        await ReplaceFileAsync(path, content, cancellationToken).ConfigureAwait(false);

        info.Publication = ParsePublication(json);
        info.Downloaded = DateTime.Now;
        info.LastError = null;
        info.FailedAttempts = 0;
        info.Size = content.LongLength;
        info.ETag = response.Headers.ETag?.ToString();
        info.LastModified = response.Content.Headers.LastModified;

        return true;
    }

    /// <summary>
    ///     Gets the publication date (local time) of a bootstrap file.
    /// </summary>
    private static DateTime? ParsePublication(JObject json)
    {
        return RDAPDateParser.TryParse(json["publication"]?.ToString(), out var publication)
            ? publication.LocalDateTime
            : null;
    }

    /// <summary>
    ///     Reads the publication date (local time) of a cached bootstrap file.
    /// </summary>
    private static DateTime? ReadPublication(string path)
    {
        try
        {
            using var reader = new JsonTextReader(new StreamReader(path)) { DateParseHandling = DateParseHandling.None };

            return ParsePublication(JObject.Load(reader));
        }
        catch (Exception ex)
        {
            Log.Warn($"Could not read the publication date of {path}.", ex);
            return null;
        }
    }

    /// <summary>
    ///     Writes the file to a temporary file first and then replaces the cached copy. The replace is retried, since
    ///     the file can be locked for a short time (e.g. by a virus scanner checking the new file).
    /// </summary>
    private static async Task ReplaceFileAsync(string path, byte[] content, CancellationToken cancellationToken)
    {
        var tempPath = path + ".tmp";

        try
        {
            await File.WriteAllBytesAsync(tempPath, content, cancellationToken).ConfigureAwait(false);

            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    File.Move(tempPath, path, true);
                    return;
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException && attempt < 5)
                {
                    await Task.Delay(200 * attempt, cancellationToken).ConfigureAwait(false);
                }
            }
        }
        catch
        {
            // Don't leave a partial file behind, the cached copy is still used.
            try
            {
                File.Delete(tempPath);
            }
            catch (Exception ex)
            {
                Log.Warn($"Could not delete {tempPath}.", ex);
            }

            throw;
        }
    }

    /// <summary>
    ///     Gets the cache lifetime from the Cache-Control or Expires header (RFC 9224 8), or null if there is none.
    /// </summary>
    private static TimeSpan? GetMaxAge(HttpResponseMessage response)
    {
        if (response.Headers.CacheControl?.MaxAge is { } maxAge && maxAge > TimeSpan.Zero)
            return maxAge;

        if (response.Content.Headers.Expires is { } expires && expires > DateTimeOffset.Now)
            return expires - DateTimeOffset.Now;

        return null;
    }

    private void LoadCacheInfos(string cacheDirectory)
    {
        if (_cacheInfos != null && _cacheDirectory == cacheDirectory)
            return;

        _cacheDirectory = cacheDirectory;
        _dns = null;

        List<RDAPBootstrapCacheInfo> stored = null;

        var path = Path.Combine(cacheDirectory, CacheInfoFileName);

        try
        {
            if (File.Exists(path))
                stored = JsonConvert.DeserializeObject<List<RDAPBootstrapCacheInfo>>(File.ReadAllText(path),
                    new JsonSerializerSettings
                    {
                        // Ignore single invalid values (e.g. from an older format), the entry is repaired below.
                        Error = (_, args) => args.ErrorContext.Handled = true
                    });
        }
        catch (Exception ex)
        {
            Log.Warn($"Could not read {path}.", ex);
        }

        _cacheInfos = Enum.GetValues<RDAPBootstrapFileType>().Select(type =>
        {
            var info = stored?.FirstOrDefault(x => x.Type == type) ?? new RDAPBootstrapCacheInfo { Type = type };

            info.FileName = GetFileName(type);

            var file = new FileInfo(Path.Combine(cacheDirectory, info.FileName));

            if (!file.Exists)
            {
                // The file may have been deleted manually.
                info.Downloaded = null;
                info.Size = 0;
                info.Expires = null;
            }
            else
            {
                // The publication date is always taken from the cached file itself.
                info.Publication = ReadPublication(file.FullName) ?? info.Publication;
            }

            if (file.Exists && (info.Downloaded == null || info.Size == 0))
            {
                // Incomplete entry: show the file on disk and download it again with the next update.
                info.Downloaded = file.LastWriteTime;
                info.Size = file.Length;
                info.ETag = null;
                info.LastModified = null;
                info.Expires = null;
            }

            return info;
        }).ToList();
    }

    private void SaveCacheInfos()
    {
        try
        {
            File.WriteAllText(Path.Combine(_cacheDirectory, CacheInfoFileName),
                JsonConvert.SerializeObject(_cacheInfos, Formatting.Indented));
        }
        catch (Exception ex)
        {
            Log.Warn("Could not save RDAP bootstrap cache info.", ex);
        }
    }

    private static RDAPBootstrapCacheInfo Clone(RDAPBootstrapCacheInfo info)
    {
        return new RDAPBootstrapCacheInfo
        {
            Type = info.Type,
            FileName = info.FileName,
            Publication = info.Publication,
            Downloaded = info.Downloaded,
            LastChecked = info.LastChecked,
            Size = info.Size,
            ETag = info.ETag,
            LastModified = info.LastModified,
            Expires = info.Expires,
            LastError = info.LastError,
            FailedAttempts = info.FailedAttempts
        };
    }

    #endregion

    #region Parse

    private void LoadRegistries()
    {
        _dns = ToDictionary(ReadServices(RDAPBootstrapFileType.DNS));
        _objectTags = ToDictionary(ReadServices(RDAPBootstrapFileType.ObjectTags));
        _ipv4 = ToNetworks(ReadServices(RDAPBootstrapFileType.IPv4));
        _ipv6 = ToNetworks(ReadServices(RDAPBootstrapFileType.IPv6));

        var asn = ReadServices(RDAPBootstrapFileType.ASN);

        _asn = asn == null
            ? null
            : asn.SelectMany(x => x.Keys.Select(key => ParseASNRange(key, x.Urls)))
                .Where(x => x.Urls != null)
                .ToList();
    }

    /// <summary>
    ///     Reads the services of a cached bootstrap file. A service is [[keys...], [urls...]], or
    ///     [[contacts...], [tags...], [urls...]] for object tags (RFC 8521), so keys and URLs are always the last two
    ///     elements.
    /// </summary>
    private List<(List<string> Keys, List<string> Urls)> ReadServices(RDAPBootstrapFileType type)
    {
        var path = Path.Combine(_cacheDirectory, GetFileName(type));

        try
        {
            return File.Exists(path) ? ParseServices(File.ReadAllText(path)) : null;
        }
        catch (Exception ex)
        {
            Log.Error($"Could not parse RDAP bootstrap file {path}.", ex);
            return null;
        }
    }

    private static List<(List<string> Keys, List<string> Urls)> ParseServices(string json)
    {
        var services = JObject.Parse(json)["services"] as JArray ?? [];

        return services.OfType<JArray>()
            .Where(x => x.Count >= 2 && x[^2] is JArray && x[^1] is JArray)
            .Select(x => (x[^2].Select(key => key.ToString()).ToList(),
                x[^1].Select(url => url.ToString()).ToList()))
            .ToList();
    }

    private static Dictionary<string, List<string>> ToDictionary(List<(List<string> Keys, List<string> Urls)> services)
    {
        if (services == null)
            return null;

        var dictionary = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var (keys, urls) in services)
        foreach (var key in keys)
            dictionary.TryAdd(key, urls);

        return dictionary;
    }

    private static List<(IPNetwork Network, List<string> Urls)> ToNetworks(
        List<(List<string> Keys, List<string> Urls)> services)
    {
        return services?.SelectMany(x => x.Keys
                .Select(key => (Valid: IPNetwork.TryParse(key, out var network), Network: network, x.Urls)))
            .Where(x => x.Valid)
            .Select(x => (x.Network, x.Urls))
            .ToList();
    }

    /// <summary>
    ///     Parses an AS number range "start-end". IANA also lists single numbers without a hyphen (e.g. "2043"),
    ///     although RFC 9224 5.3 requires "2043-2043".
    /// </summary>
    private static (uint Start, uint End, List<string> Urls) ParseASNRange(string key, List<string> urls)
    {
        var parts = key.Split('-');

        if (parts.Length is < 1 or > 2 || !uint.TryParse(parts[0], out var start))
            return (0, 0, null);

        var end = start;

        if (parts.Length == 2 && !uint.TryParse(parts[1], out end))
            return (0, 0, null);

        return (start, end, urls);
    }

    private static Dictionary<string, List<string>> LoadStealthEndpoints()
    {
        try
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(StealthEndpointsResource);

            if (stream == null)
                return [];

            using var reader = new StreamReader(stream);

            return ToDictionary(ParseServices(reader.ReadToEnd()));
        }
        catch (Exception ex)
        {
            Log.Error("Could not load RDAP stealth endpoints.", ex);
            return [];
        }
    }

    #endregion
}
