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
        HttpClient client = new();

        /// <summary>
        /// Base URL fo the ip-api free endpoint.
        /// </summary>
        private const string _baseURL = "http://edns.ip-api.com/json";
       
        public async Task<IPDNSApiResult> GetIPDNSDetailsAsync()
        {
            try
            {
                var response = await client.GetAsync(_baseURL);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var info = JsonConvert.DeserializeObject<IPDNSApiInfo>(json);

                    return new IPDNSApiResult(info);
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
