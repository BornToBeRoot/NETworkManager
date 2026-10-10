using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using NETworkManager.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NETworkManager.Models.RDAP;

/// <summary>
///     Client for the Registration Data Access Protocol (RDAP, RFC 7480, RFC 9082, RFC 9083, RFC 9224).
/// </summary>
public class RDAPClient : SingletonBase<RDAPClient>
{
    #region Variables

    private static readonly ILog Log = LogManager.GetLogger(typeof(RDAPClient));

    private const string RDAPMediaType = "application/rdap+json";

    /// <summary>
    ///     Maximum number of redirects to follow for one request.
    /// </summary>
    private const int MaxRedirects = 5;

    private static readonly JsonSerializerSettings SerializerSettings = new()
    {
        // Keep dates as strings. Newtonsoft would otherwise convert them to DateTime and lose the offset.
        DateParseHandling = DateParseHandling.None,
        MissingMemberHandling = MissingMemberHandling.Ignore,
        Converters = [new RDAPStringListConverter()],
        // Ignore members with an unexpected type (e.g. a number sent as string) instead of failing the whole response.
        Error = (_, args) => args.ErrorContext.Handled = true
    };

    private readonly HttpClient _client;

    #endregion

    #region Constructor

    public RDAPClient()
    {
        // Redirects are followed manually to limit the hops, detect loops and prevent HTTPS to HTTP downgrades.
        _client = new HttpClient(new HttpClientHandler
        {
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.All
        })
        {
            Timeout = Timeout.InfiniteTimeSpan
        };

        _client.DefaultRequestHeaders.UserAgent.ParseAdd("NETworkManager");
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(RDAPMediaType));
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    #endregion

    #region Public methods

    /// <summary>
    ///     Executes an RDAP query.
    /// </summary>
    /// <param name="query">Query to execute (see <see cref="RDAPQueryParser" />).</param>
    /// <param name="cacheDirectory">Directory where the IANA bootstrap files are cached.</param>
    /// <param name="timeout">Timeout for each request.</param>
    /// <param name="followReferral">Follow the referral to the registrar's RDAP record (domain queries only).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result of the query.</returns>
    /// <exception cref="RDAPException">If the query fails.</exception>
    public async Task<RDAPResult> QueryAsync(RDAPQuery query, string cacheDirectory, TimeSpan timeout,
        bool followReferral, CancellationToken cancellationToken = default)
    {
        var bootstrapService = RDAPBootstrapService.GetInstance();

        // TLD queries go directly to IANA and don't need the bootstrap files.
        if (query.Type != RDAPQueryType.TLD)
            await bootstrapService.EnsureUpToDateAsync(cacheDirectory, cancellationToken).ConfigureAwait(false);

        var baseUrls = bootstrapService.GetBaseUrls(query);

        RDAPException lastNetworkError = null;

        // Try the next server only on network errors. Any HTTP answer is authoritative for the query.
        foreach (var baseUrl in baseUrls)
        {
            try
            {
                var result = await GetAsync(query, baseUrl + query.PathSegment, timeout, cancellationToken)
                    .ConfigureAwait(false);

                if (followReferral && query.Type == RDAPQueryType.Domain)
                    await FollowReferralAsync(result, timeout, cancellationToken).ConfigureAwait(false);

                return result;
            }
            catch (RDAPException ex) when (ex.Kind == RDAPErrorKind.Network)
            {
                Log.Warn($"RDAP request to {ex.RequestUrl} failed: {ex.Message}");
                lastNetworkError = ex;
            }
        }

        throw lastNetworkError ?? new RDAPException(RDAPErrorKind.NoServer);
    }

    /// <summary>
    ///     Gets the link to the registrar's RDAP record from a domain response. Links with "rel" containing "related"
    ///     and an RDAP media type are preferred, otherwise "related" links that look like an RDAP URL are used. Links
    ///     pointing to the response itself are skipped (some registries add a "related" link to themselves).
    /// </summary>
    /// <param name="response">Domain response.</param>
    /// <param name="requestUrl">URL of the request that returned the response.</param>
    /// <returns>URL of the referral, or null if there is none.</returns>
    public static string GetReferralUrl(RDAPResponse response, string requestUrl)
    {
        var selfUrls = new List<string> { requestUrl };

        selfUrls.AddRange(response.Links?.Where(x => HasRel(x, "self")).Select(x => x.Href) ?? []);

        var candidates = response.Links?
            .Where(x => HasRel(x, "related") && IsHttpUrl(x.Href) &&
                        !selfUrls.Any(self => UrlEquals(self, x.Href)))
            .ToList() ?? [];

        return (candidates.FirstOrDefault(x => IsRDAPMediaType(x.Type)) ??
                candidates.FirstOrDefault(x => string.IsNullOrEmpty(x.Type) && HasRDAPPath(x.Href)))?.Href;
    }

    #endregion

    #region Methods

    private async Task FollowReferralAsync(RDAPResult result, TimeSpan timeout, CancellationToken cancellationToken)
    {
        var referralUrl = GetReferralUrl(result.Response, result.RequestUrl);

        if (referralUrl == null)
            return;

        try
        {
            result.Referral = await GetAsync(result.Query, referralUrl, timeout, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (RDAPException ex)
        {
            // The registry response is still valid if the registrar fails.
            result.ReferralError = ex;
        }
    }

    /// <summary>
    ///     Sends a GET request, follows redirects and parses the response.
    /// </summary>
    private async Task<RDAPResult> GetAsync(RDAPQuery query, string url, TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var currentUrl = new Uri(url);

        for (var redirects = 0; ; redirects++)
        {
            if (!visited.Add(currentUrl.AbsoluteUri) || redirects > MaxRedirects)
                throw new RDAPException(RDAPErrorKind.TooManyRedirects) { RequestUrl = currentUrl.AbsoluteUri };

            using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutSource.CancelAfter(timeout);

            HttpResponseMessage response;
            string body;

            try
            {
                response = await _client.GetAsync(currentUrl, timeoutSource.Token).ConfigureAwait(false);
                body = await response.Content.ReadAsStringAsync(timeoutSource.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException)
            {
                var message = ex is OperationCanceledException ? $"Timeout after {timeout.TotalSeconds} s." : ex.Message;

                throw new RDAPException(RDAPErrorKind.Network, message, ex) { RequestUrl = currentUrl.AbsoluteUri };
            }

            using (response)
            {
                if (IsRedirect(response.StatusCode) && response.Headers.Location != null)
                {
                    // The server returns the complete URL of the query (RFC 7480 5.2).
                    var location = response.Headers.Location.IsAbsoluteUri
                        ? response.Headers.Location
                        : new Uri(currentUrl, response.Headers.Location);

                    if (currentUrl.Scheme == Uri.UriSchemeHttps && location.Scheme != Uri.UriSchemeHttps)
                        throw new RDAPException(RDAPErrorKind.InsecureRedirect, location.AbsoluteUri)
                            { RequestUrl = currentUrl.AbsoluteUri };

                    currentUrl = location;
                    continue;
                }

                if (response.StatusCode != HttpStatusCode.OK)
                    throw CreateHttpException(response, body, currentUrl.AbsoluteUri);

                return CreateResult(query, currentUrl.AbsoluteUri, body);
            }
        }
    }

    private static bool IsRedirect(HttpStatusCode statusCode)
    {
        return statusCode is HttpStatusCode.MovedPermanently or HttpStatusCode.Found or HttpStatusCode.SeeOther
            or HttpStatusCode.TemporaryRedirect or HttpStatusCode.PermanentRedirect;
    }

    private static RDAPResult CreateResult(RDAPQuery query, string url, string body)
    {
        RDAPResponse response;
        string formattedJson;

        try
        {
            using var reader = new JsonTextReader(new StringReader(body))
            {
                DateParseHandling = DateParseHandling.None,
                FloatParseHandling = FloatParseHandling.Decimal
            };

            var token = JToken.ReadFrom(reader);

            if (token is not JObject)
                throw new JsonException("The response is not a JSON object.");

            formattedJson = token.ToString(Formatting.Indented);
            response = JsonConvert.DeserializeObject<RDAPResponse>(body, SerializerSettings);
        }
        catch (JsonException ex)
        {
            throw new RDAPException(RDAPErrorKind.InvalidResponse, ex.Message, ex) { RequestUrl = url };
        }

        return new RDAPResult
        {
            Query = query,
            RequestUrl = url,
            RawJson = body,
            FormattedJson = formattedJson,
            Response = response
        };
    }

    private static RDAPException CreateHttpException(HttpResponseMessage response, string body, string url)
    {
        var kind = response.StatusCode switch
        {
            HttpStatusCode.NotFound => RDAPErrorKind.NotFound,
            HttpStatusCode.BadRequest => RDAPErrorKind.BadRequest,
            HttpStatusCode.TooManyRequests => RDAPErrorKind.RateLimited,
            HttpStatusCode.NotImplemented => RDAPErrorKind.NotImplemented,
            _ => RDAPErrorKind.HttpError
        };

        // Use the RDAP error response (RFC 9083 6), if the server sent one.
        RDAPResponse error = null;

        try
        {
            if (!string.IsNullOrWhiteSpace(body) && body.TrimStart().StartsWith('{'))
                error = JsonConvert.DeserializeObject<RDAPResponse>(body, SerializerSettings);
        }
        catch (JsonException)
        {
            // Not an RDAP error response (e.g. an HTML error page).
        }

        var description = error?.Description?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

        return new RDAPException(kind, $"{(int)response.StatusCode} {response.ReasonPhrase}")
        {
            StatusCode = (int)response.StatusCode,
            ErrorTitle = error?.Title,
            ErrorDescription = description is { Count: > 0 } ? string.Join(" ", description) : null,
            RetryAfter = GetRetryAfter(response.Headers.RetryAfter),
            RequestUrl = url
        };
    }

    private static TimeSpan? GetRetryAfter(RetryConditionHeaderValue retryAfter)
    {
        if (retryAfter?.Delta != null)
            return retryAfter.Delta;

        if (retryAfter?.Date != null && retryAfter.Date > DateTimeOffset.Now)
            return retryAfter.Date - DateTimeOffset.Now;

        return null;
    }

    private static bool HasRel(RDAPLink link, string rel)
    {
        // "rel" can contain multiple space-separated relation types.
        return link.Rel?.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Any(x => x.Equals(rel, StringComparison.OrdinalIgnoreCase)) == true;
    }

    private static bool IsRDAPMediaType(string type)
    {
        return type != null && type.Split(';')[0].Trim().Equals(RDAPMediaType, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsHttpUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
    }

    private static bool HasRDAPPath(string url)
    {
        return new[] { "/domain/", "/ip/", "/autnum/", "/entity/", "/nameserver/" }
            .Any(x => url.Contains(x, StringComparison.OrdinalIgnoreCase));
    }

    private static bool UrlEquals(string a, string b)
    {
        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
            return false;

        return string.Equals(a.TrimEnd('/'), b.TrimEnd('/'), StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}
