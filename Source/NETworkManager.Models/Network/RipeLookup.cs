using NETworkManager.Utilities;
using System.Text;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network
{
    public static class RipeLookup
    {
        private static readonly string RipeApi = $"http://rest.db.ripe.net/search.xml?query-string=";
        private static readonly string Flags = "flags=no-filtering";

        static RipeLookup(){}

        public static Task<string> QueryAsync(string domain)
        {
            return Task.Run(() => Query(BuildApiPath(domain)));
        }

        private static string BuildApiPath(string domain)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(RipeApi);
            sb.Append(domain);
            sb.Append('&');
            sb.Append(Flags);

            return sb.ToString();
        }

        public static async Task<string> Query(string domain)
        {
            RestApiClient client = new RestApiClient();
            var response = await client.Get(domain);

            if(response is not null)
                return RestApiClient.ConvertXmlToDictionary(response);

            return "";
        }
    }
}
