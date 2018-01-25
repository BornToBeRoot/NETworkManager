using System;
using System.Net;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network
{
    public class HTTPHeaders
    {
        public static async Task<WebHeaderCollection> GetHeadersAsync(Uri uri, HTTPHeadersOptions options)
        {
            WebHeaderCollection headers;

            HttpWebRequest request = WebRequest.CreateHttp(uri);

            request.Timeout = options.Timeout;

            using (HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync().ConfigureAwait(false)))
            {
                headers = response.Headers;
            }

            return headers;
        }
    }
}