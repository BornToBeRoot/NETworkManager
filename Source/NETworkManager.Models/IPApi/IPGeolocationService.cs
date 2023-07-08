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
    private readonly HttpClient client = new();

    /// <summary>
    /// Base URL fo the ip-api free endpoint.
    /// </summary>
    private const string _baseURL = "http://ip-api.com/json/";

    /// <summary>
    /// Fields to be returned by the API. See documentation for more details.
    /// </summary>
    private const string _fields = "status,message,continent,continentCode,country,countryCode,region,regionName,city,district,zip,lat,lon,timezone,offset,currency,isp,org,as,asname,reverse,mobile,proxy,hosting,query";

    /// <summary>
    /// Indicates whether we have reached the rate limit.
    /// </summary>
    private bool _rateLimit_IsReached = false;

    /// <summary>
    /// Remaining requests that can be processed until the rate limit window is reset.
    /// This value is updated by Header "X-Rl". Default is 45 requests.
    /// </summary>        
    private int _rateLimit_RemainingRequests = 45;

    /// <summary>
    /// Remaining time in seconds until the rate limit window resets.
    /// This value is updated by Header "X-Ttl". Default is 60 seconds.
    /// </summary>
    private int _rateLimit_RemainingTime = 60;

    /// <summary>
    /// Last time the rate limit was reached.
    /// </summary>
    private DateTime _rateLimit_LastReached = DateTime.MinValue;

    /// <summary>
    /// Gets the IP geolocation details from the API asynchronously.
    /// </summary>
    /// <returns>IP geolocation informations as <see cref="IPGeolocationResult"/>.</returns>
    public async Task<IPGeolocationResult> GetIPGeolocationAsync(string ipAddress = "")
    {
        if (IsInRateLimit())
            return new IPGeolocationResult(isRateLimitReached: true);

        // If the url is empty, the current IP address from which the request is made is used.
        string url = $"{_baseURL}/{ipAddress}?fields={_fields}";

        try
        {
            var response = await client.GetAsync(url);

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
                _rateLimit_IsReached = true;
                _rateLimit_RemainingTime = 60;
                _rateLimit_RemainingRequests = 0;
                _rateLimit_LastReached = DateTime.Now;

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
        if (!_rateLimit_IsReached)
            return false;

        // The rate limit time window is reset when the remaining time is over.
        DateTime lastReached = _rateLimit_LastReached;

        if (lastReached.AddSeconds(_rateLimit_RemainingTime + 1) < DateTime.Now)
        {
            _rateLimit_IsReached = false;

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
    private bool UpdateRateLimit(HttpResponseHeaders headers)
    {
        // Parse header data
        if (!headers.TryGetValues("X-Rl", out var x_rl))
            return false;

        if (!int.TryParse(x_rl.ToArray()[0], out int remainingRequests))
            return false;

        if (!headers.TryGetValues("X-Ttl", out var x_ttl))
            return false;

        if (!int.TryParse(x_ttl.ToArray()[0], out var remainingTime))
            return false;

        _rateLimit_RemainingTime = remainingTime;
        _rateLimit_RemainingRequests = remainingRequests;

        // Only allow 40 requests... to prevent a 429 error if other
        // devices or tools on the network (e.g. another NETworkManager
        // instance) doing requests agains ip-api.com.
        if (_rateLimit_RemainingRequests < 5)
        {
            _rateLimit_IsReached = true;
            _rateLimit_LastReached = DateTime.Now;
        }

        return true;
    }
}
