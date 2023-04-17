using System;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NETworkManager.Utilities
{
    public class RestApiClient
    {
        private HttpClient _httpClient;

        public RestApiClient()
        {
            _httpClient = new HttpClient();
        }

        public async Task<string> Get(string apiUrl)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        public static string ConvertXmlToDictionary(string xml)
        {
            StringBuilder stringBuilder = new StringBuilder();

            string pattern = @"<attribute name=""([^""]+)"" value=""([^""]+)""/>";
            MatchCollection matches = Regex.Matches(xml, pattern);

            foreach (Match match in matches)
            {
                if (match.Groups.Count == 3)
                {
                    string attributeName = match.Groups[1].Value;
                    string attributeValue = match.Groups[2].Value;
                    stringBuilder.AppendLine(attributeName + "=" + attributeValue);
                }
            }
            return stringBuilder.ToString();
        }
    }
}
