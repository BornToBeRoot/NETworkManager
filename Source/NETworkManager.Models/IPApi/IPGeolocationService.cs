using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using log4net;
using NETworkManager.Utilities;
using Newtonsoft.Json;

namespace NETworkManager.Models.IPApi;

/// <summary>
///     Class to interact with the IP geolocation API from ip-api.com.
///     Documentation is available at https://ip-api.com/docs
/// </summary>
public class IPGeolocationService : SingletonBase<IPGeolocationService>
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(IPGeolocationService));

    /// <summary>
    ///     Base URL fo the ip-api free endpoint.
    /// </summary>
    private const string BaseUrl = "http://ip-api.com/json/";

    /// <summary>
    ///     Fields to be returned by the API. See documentation for more details.
    /// </summary>
    private const string Fields =
        "status,message,continent,continentCode,country,countryCode,region,regionName,city,district,zip,lat,lon,timezone,offset,currency,isp,org,as,asname,reverse,mobile,proxy,hosting,query";

    /// <summary>
    ///     Host of the API (shown in messages).
    /// </summary>
    public const string Host = "ip-api.com";

    /// <summary>
    ///     Time until a request fails without a response (instead of the 100 seconds of <see cref="HttpClient" />, e.g.
    ///     if a firewall drops the request or for each hop of the traceroute), like the public IP address request of the
    ///     network connection widget.
    /// </summary>
    public static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(10);

    // A new connection per request (no keep-alive): a check right after a network change (e.g. a VPN connected) must
    // not reuse a connection of the old network, which would return the old public IP address
    private readonly HttpClient _client = new()
        { Timeout = RequestTimeout, DefaultRequestHeaders = { ConnectionClose = true } };

    /// <summary>
    ///     Indicates whether we have reached the rate limit.
    /// </summary>
    private bool _rateLimitIsReached;

    /// <summary>
    ///     Last time the rate limit was reached.
    /// </summary>
    private DateTime _rateLimitLastReached = DateTime.MinValue;

    /// <summary>
    ///     Remaining requests that can be processed until the rate limit window is reset.
    ///     This value is updated by Header "X-Rl". Default is 45 requests.
    /// </summary>
    private int _rateLimitRemainingRequests = 45;

    /// <summary>
    ///     Remaining time in seconds until the rate limit window resets.
    ///     This value is updated by Header "X-Ttl". Default is 60 seconds.
    /// </summary>
    private int _rateLimitRemainingTime = 60;

    /// <summary>
    ///     Gets the IP geolocation details from the API asynchronously.
    /// </summary>
    /// <param name="ipAddressOrFqdn">IP address or FQDN to get the geolocation information's from.</param>
    /// <returns>IP geolocation information's as <see cref="IPGeolocationResult" />.</returns>
    public async Task<IPGeolocationResult> GetIPGeolocationAsync(string ipAddressOrFqdn = "")
    {
        if (IsInRateLimit())
            return new IPGeolocationResult(true,
                GetRemainingSeconds(_rateLimitLastReached, _rateLimitRemainingTime, DateTime.Now));

        var url = GetUrl(ipAddressOrFqdn);

        try
        {
            var response = await _client.GetAsync(url);

            // Check if the request was successful.
            if (response.IsSuccessStatusCode)
            {
                // Update rate limit values. Without them (e.g. removed by a proxy) the answer is still used, the last
                // values are kept (a 429 still stops the requests)
                if (!UpdateRateLimit(response.Headers))
                    Log.Warn("The rate limit headers (X-Rl, X-Ttl) of ip-api.com are missing or invalid.");

                return ParseResponse(await response.Content.ReadAsStringAsync());
            }

            // Consider the request as failed if the status code is not successful or 429.
            if ((int)response.StatusCode != 429)
                return new IPGeolocationResult(new HttpRequestException(response.ReasonPhrase, null,
                    response.StatusCode));

            // Code 429
            // We have already reached the rate limit (on the network). The time until the window resets (X-Ttl) is
            // used if the answer has it, otherwise the default value.
            _rateLimitIsReached = true;
            _rateLimitRemainingTime = response.Headers.TryGetValues("X-Ttl", out var xTtl) &&
                                      int.TryParse(xTtl.FirstOrDefault(), out var remainingTime) && remainingTime >= 0
                ? remainingTime
                : 60;
            _rateLimitRemainingRequests = 0;
            _rateLimitLastReached = DateTime.Now;

            // Like the following requests (incl. the margin), so the time left counts down (61, 60, ...)
            return new IPGeolocationResult(true,
                GetRemainingSeconds(_rateLimitLastReached, _rateLimitRemainingTime, DateTime.Now));
        }
        catch (Exception ex)
        {
            return new IPGeolocationResult(ex);
        }
    }

    /// <summary>
    ///     URL of the request (<c>http://ip-api.com/json/{query}?fields=...</c>).
    /// </summary>
    /// <param name="ipAddressOrFqdn">IP address or host name, empty for the public IP address of the computer.</param>
    /// <returns>URL.</returns>
    public static string GetUrl(string ipAddressOrFqdn)
    {
        // The base URL ends with a slash (no "json//" for the own IP address)
        return $"{BaseUrl}{ipAddressOrFqdn}?fields={Fields}";
    }

    /// <summary>
    ///     Parses the answer of the API.
    /// </summary>
    /// <param name="json">Body of the answer.</param>
    /// <returns>IP geolocation information, an error if the body is empty (e.g. from a proxy or a captive portal).</returns>
    /// <exception cref="JsonException">The body is not JSON.</exception>
    public static IPGeolocationResult ParseResponse(string json)
    {
        var info = JsonConvert.DeserializeObject<IPGeolocationInfo>(json);

        return info == null
            ? new IPGeolocationResult(new InvalidDataException("The response of the API is empty."))
            : new IPGeolocationResult(info);
    }

    /// <summary>
    ///     End of the rate limit: the time window (<c>X-Ttl</c>) plus one second of margin.
    /// </summary>
    /// <param name="lastReached">Time when the rate limit was reached.</param>
    /// <param name="remainingTime">Seconds until the time window resets (<c>X-Ttl</c>, 60 after HTTP 429).</param>
    /// <returns>Time from which requests are sent again.</returns>
    private static DateTime GetRateLimitEnd(DateTime lastReached, int remainingTime)
    {
        return lastReached.AddSeconds(remainingTime + 1);
    }

    /// <summary>
    ///     Seconds until requests are sent again (shown as "try again in ... seconds"), counting down while the rate
    ///     limit is reached instead of the whole time window.
    /// </summary>
    /// <param name="lastReached">Time when the rate limit was reached.</param>
    /// <param name="remainingTime">Seconds until the time window resets (<c>X-Ttl</c>, 60 after HTTP 429).</param>
    /// <param name="now">Current time.</param>
    /// <returns>Seconds, rounded up, at least 1.</returns>
    public static int GetRemainingSeconds(DateTime lastReached, int remainingTime, DateTime now)
    {
        var seconds = (int)Math.Ceiling((GetRateLimitEnd(lastReached, remainingTime) - now).TotalSeconds);

        return Math.Max(1, seconds);
    }

    /// <summary>
    ///     Checks whether the rate limit is reached.
    /// </summary>
    /// <returns>True if the rate limit is reached, false otherwise.</returns>
    private bool IsInRateLimit()
    {
        // If the rate limit is not reached, return false.
        if (!_rateLimitIsReached)
            return false;

        // The rate limit time window is reset when the remaining time is over.
        var lastReached = _rateLimitLastReached;

        // We are still in the rate limit
        if (GetRateLimitEnd(lastReached, _rateLimitRemainingTime) >= DateTime.Now)
            return true;

        // We are not in the rate limit anymore
        _rateLimitIsReached = false;

        return false;
    }

    /// <summary>
    ///     Reads the rate limit from the headers of the response.
    /// </summary>
    /// <param name="headers">Headers of the response.</param>
    /// <param name="remainingRequests">Requests left in the time window (<c>X-Rl</c>).</param>
    /// <param name="remainingTime">Seconds until the time window resets (<c>X-Ttl</c>).</param>
    /// <returns><c>true</c> if both headers are present and valid.</returns>
    public static bool TryParseRateLimit(HttpHeaders headers, out int remainingRequests, out int remainingTime)
    {
        remainingRequests = 0;
        remainingTime = 0;

        return headers.TryGetValues("X-Rl", out var xRl) &&
               int.TryParse(xRl.FirstOrDefault(), out remainingRequests) &&
               headers.TryGetValues("X-Ttl", out var xTtl) &&
               int.TryParse(xTtl.FirstOrDefault(), out remainingTime);
    }

    /// <summary>
    ///     Updates the rate limit values.
    /// </summary>
    /// <param name="headers">Headers from the response.</param>
    /// <returns>True if the update was successful, false otherwise.</returns>
    private bool UpdateRateLimit(HttpHeaders headers)
    {
        if (!TryParseRateLimit(headers, out var remainingRequests, out var remainingTime))
            return false;

        _rateLimitRemainingTime = remainingTime;
        _rateLimitRemainingRequests = remainingRequests;

        // Only allow 40 requests... to prevent a 429 error if other
        // devices or tools on the network (e.g. another NETworkManager
        // instance) doing requests against ip-api.com.
        if (_rateLimitRemainingRequests >= 5)
            return true;

        // We have reached the rate limit (on the network)
        // Disable the service and store the time when the rate limit was reached.
        _rateLimitIsReached = true;
        _rateLimitLastReached = DateTime.Now;

        return true;
    }
}