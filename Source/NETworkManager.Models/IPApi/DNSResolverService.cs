using NETworkManager.Utilities;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NETworkManager.Models.IPApi;

/// <summary>
/// Class to interact with the DNS resolver API from ip-api.com.
/// Documentation is available at https://ip-api.com/docs
/// </summary>
public class DNSResolverService : SingletonBase<DNSResolverService>
{
    private readonly HttpClient client = new();

    /// <summary>
    /// Base URL fo the edns.ip-api.
    /// </summary>
    private const string _baseURL = "http://edns.ip-api.com/json";

    /// <summary>
    /// Gets the IP DNS details from the API asynchronously.
    /// </summary>
    /// <returns>IP DNS informations as <see cref="DNSResolverResult"/>.</returns>
    public async Task<DNSResolverResult> GetDNSResolverAsync()
    {
        try
        {
            var response = await client.GetAsync(_baseURL);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var info = JsonConvert.DeserializeObject<DNSResolverDeserializationInfo>(json);

                return new DNSResolverResult(DNSResolverInfo.Parse(info));
            }
            else
            {
                return new DNSResolverResult(hasError: true, response.ReasonPhrase, (int)response.StatusCode);
            }

        }
        catch (Exception ex)
        {
            return new DNSResolverResult(hasError: true, ex.Message);
        }
    }
}
