using NETworkManager.Utilities;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network
{
    /// <summary>
    /// Class to interact with the IP DNS API from ip-api.com.
    /// Documentation is available at https://ip-api.com/docs
    /// </summary>
    public class IPDNSApiService : SingletonBase<IPDNSApiService>
    {
        private readonly HttpClient client = new();

        /// <summary>
        /// Base URL fo the ip-api free endpoint.
        /// </summary>
        private const string _baseURL = "http://edns.ip-api.com/json";

        /// <summary>
        /// Gets the IP DNS details from the API asynchronously.
        /// </summary>
        /// <returns>IP DNS informations as <see cref="IPDNSApiResult"/>.</returns>
        public async Task<IPDNSApiResult> GetIPDNSDetailsAsync()
        {
            Debug.WriteLine("Check DNS...");

            try
            {
                var response = await client.GetAsync(_baseURL);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var info = JsonConvert.DeserializeObject<IPDNSApiDeserializationInfo>(json);

                    return new IPDNSApiResult(IPDNSApiInfo.Parse(info));
                }
                else
                {
                    Debug.WriteLine($"Error code: {(int)response.StatusCode}");
                    return new IPDNSApiResult(true, $"Error code: {(int)response.StatusCode}");
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return new IPDNSApiResult(true, ex.Message);
            }
        }
    }
}
