using NETworkManager.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network
{
    /// <summary>
    /// Class to interact with the IP Geolocation API from ip-api.com.
    /// Documentation is available at https://ip-api.com/docs
    /// </summary>
    public class IPGeoApiService : SingletonBase<IPGeoApiService>
    {
        HttpClient client = new();

        /// <summary>
        /// Base URL fo the ip-api free endpoint.
        /// </summary>
        private const string _baseURL = "http://ip-api.com/json/";

        private const string _fields = "status,message,continent,continentCode,country,countryCode,region,regionName,city,district,zip,lat,lon,timezone,offset,currency,isp,org,as,asname,reverse,mobile,proxy,hosting,query";

        /// <summary>
        /// Remaining time in seconds until the rate limit window resets.
        /// This value is updated by Header "X-Ttl". Default is 60 seconds.
        /// </summary>
        private int _rateLimit_RemainingTime = 60;

        /// <summary>
        /// Remaining requests that can be executed until the rate limit window is reset.
        /// This value is updated by Header "X-Rl". Default is 45 requests.
        /// </summary>
        private int _rateLimit_RemainingRequests = 45;

       
        public async Task<IPGeoApiResult> GetIPDetailsAsync(string ipAddress = "")
        {
            // ToDo: Implement rate limiting check

            // If the url is empty, the current IP address from which the request is made is used.
            string url = $"{_baseURL}/{ipAddress}?fields={_fields}";

            try
            {
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var info = JsonConvert.DeserializeObject<IPGeoApiInfo>(json);

                    return new IPGeoApiResult(info);
                }
                else if ((int)response.StatusCode == 429)
                {
                    Debug.WriteLine("Rate limit..");
                    return new IPGeoApiResult(true, "Rate limit...");
                }
                else
                {
                    Debug.WriteLine($"Error code: {(int)response.StatusCode}");
                    return new IPGeoApiResult(true, $"Error code: {(int)response.StatusCode}");
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return new IPGeoApiResult(true, ex.Message);
            }
        }
    }
}
