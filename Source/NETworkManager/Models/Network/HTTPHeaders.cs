using System;
using System.Net;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network
{
    public class HTTPHeaders
    {
        public static async Task<WebHeaderCollection> GetHeadersAsync(Uri uri)
        {
            WebHeaderCollection headers;

            HttpWebRequest request = WebRequest.CreateHttp(uri);

            using (HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync().ConfigureAwait(false)))
            {
                headers = response.Headers;
            }

            return headers;
        }
    }
}