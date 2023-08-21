using NETworkManager.Utilities;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace NETworkManager.Models.IPApi;

/// <summary>
/// Class to interact with the IP geolocation API from ip-api.com.
/// Documentation is available at https://ip-api.com/docs
/// </summary>
public class IPGeolocationService : SingletonBase<IPGeolocationService>
{
    private readonly HttpClient _client = new();

    /// <summary>
    /// Base URL fo the ip-api free endpoint.
    /// </summary>
    private const string BaseUrl = "http://ip-api.com/json/";

    /// <summary>
    /// Fields to be returned by the API. See documentation for more details.
    /// </summary>
    private const string Fields = "status,message,continent,continentCode,country,countryCode,region,regionName,city,district,zip,lat,lon,timezone,offset,currency,isp,org,as,asname,reverse,mobile,proxy,hosting,query";

    /// <summary>
    /// Indicates whether we have reached the rate limit.
    /// </summary>
    private bool _rateLimitIsReached;

    /// <summary>
    /// Remaining requests that can be processed until the rate limit window is reset.
    /// This value is updated by Header "X-Rl". Default is 45 requests.
    /// </summary>        
    private int _rateLimitRemainingRequests = 45;

    /// <summary>
    /// Remaining time in seconds until the rate limit window resets.
    /// This value is updated by Header "X-Ttl". Default is 60 seconds.
    /// </summary>
    private int _rateLimitRemainingTime = 60;

    /// <summary>
    /// Last time the rate limit was reached.
    /// </summary>
    private DateTime _rateLimitLastReached = DateTime.MinValue;

    /// <summary>
    /// Gets the IP geolocation details from the API asynchronously.
    /// </summary>
    /// <returns>IP geolocation information's as <see cref="IPGeolocationResult"/>.</returns>
    public async Task<IPGeolocationResult> GetIPGeolocationAsync(string ipAddress = "")
    {
        if (IsInRateLimit())
            return new IPGeolocationResult(isRateLimitReached: true);

        // If the url is empty, the current IP address from which the request is made is used.
        string url = $"{BaseUrl}/{ipAddress}?fields={Fields}";

        try
        {
            var response = await _client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                // Update rate limit values.
                if (!UpdateRateLimit(response.Headers))
                    return new IPGeolocationResult(hasError: true, "The rate limit value could not be extracted from the http header. The request was probably corrupted. Try again in a few seconds.");

                var json = await response.Content.ReadAsStringAsync();
                var info = JsonConvert.DeserializeObject<IPGeolocationInfo>(json);

                return new IPGeolocationResult(info);
            }
            else if ((int)response.StatusCode == 429)
            {
                // We have already reached the rate limit (on the network)
                _rateLimitIsReached = true;
                _rateLimitRemainingTime = 60;
                _rateLimitRemainingRequests = 0;
                _rateLimitLastReached = DateTime.Now;

                return new IPGeolocationResult(isRateLimitReached: true);
            }
            else
            {
                // Consider any status code except 200 as an error.
                return new IPGeolocationResult(hasError: true, response.ReasonPhrase, (int)response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            return new IPGeolocationResult(hasError: true, ex.Message);
        }
    }

    /// <summary>
    /// Checks whether the rate limit is reached.
    /// </summary>
    /// <returns>True if the rate limit is reached, false otherwise.</returns>
    private bool IsInRateLimit()
    {
        // If the rate limit is not reached, return false.
        if (!_rateLimitIsReached)
            return false;

        // The rate limit time window is reset when the remaining time is over.
        var lastReached = _rateLimitLastReached;

        if (lastReached.AddSeconds(_rateLimitRemainingTime + 1) < DateTime.Now)
        {
            _rateLimitIsReached = false;

            return false;
        }

        // We are still in the rate limit
        return true;
    }

    /// <summary>
    /// Updates the rate limit values.
    /// </summary>
    /// <param name="headers">Headers from the response.</param>
    /// <returns>True if the update was successful, false otherwise.</returns>
    private bool UpdateRateLimit(HttpHeaders headers)
    {
        // Parse header data
        if (!headers.TryGetValues("X-Rl", out var xRl))
            return false;

        if (!int.TryParse(xRl.ToArray()[0], out var remainingRequests))
            return false;

        if (!headers.TryGetValues("X-Ttl", values: out var xTtl))
            return false;

        if (!int.TryParse(xTtl.ToArray()[0], out var remainingTime))
            return false;

        _rateLimitRemainingTime = remainingTime;
        _rateLimitRemainingRequests = remainingRequests;

        // Only allow 40 requests... to prevent a 429 error if other
        // devices or tools on the network (e.g. another NETworkManager
        // instance) doing requests against ip-api.com.
        if (_rateLimitRemainingRequests < 5)
        {
            _rateLimitIsReached = true;
            _rateLimitLastReached = DateTime.Now;
        }

        return true;
    }
}
