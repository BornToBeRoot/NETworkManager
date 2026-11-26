using System;
using System.Net.Http;
using System.Threading.Tasks;
using NETworkManager.Utilities;
using Newtonsoft.Json;

namespace NETworkManager.Models.IPApi;

/// <summary>
///     Class to interact with the DNS resolver API from ip-api.com.
///     Documentation is available at https://ip-api.com/docs
/// </summary>
public class DNSResolverService : SingletonBase<DNSResolverService>
{
    /// <summary>
    ///     Base URL fo the edns.ip-api.
    /// </summary>
    private const string BaseUrl = "https://edns.ip-api.com/json";

    /// <summary>
    ///     Host of the API (shown in messages).
    /// </summary>
    public const string Host = "edns.ip-api.com";

    /// <summary>
    ///     Time until a request fails without a response (instead of the 100 seconds of <see cref="HttpClient" />, e.g.
    ///     if a firewall drops the request), like the public IP address request of the network connection widget.
    /// </summary>
    public static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(10);

    private readonly HttpClient _client = new() { Timeout = RequestTimeout };

    /// <summary>
    ///     Gets the IP DNS details from the API asynchronously.
    /// </summary>
    /// <returns>IP DNS information's as <see cref="DNSResolverResult" />.</returns>
    public async Task<DNSResolverResult> GetDNSResolverAsync()
    {
        try
        {
            var response = await _client.GetAsync(BaseUrl);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var info = JsonConvert.DeserializeObject<DNSResolverDeserializationInfo>(json);

                return new DNSResolverResult(DNSResolverInfo.Parse(info));
            }

            return new DNSResolverResult(true, response.ReasonPhrase, (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return new DNSResolverResult(true, ex.Message);
        }
    }
}