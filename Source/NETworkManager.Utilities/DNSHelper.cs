using System.Threading.Tasks;

namespace NETworkManager.Utilities
{
    public static class DNSHelper
    {
        public static async Task<DNSResultIPAddress> ResolveAorAaaaAsync(string query, bool preferIPv4)
        {
            DNSResultIPAddress firstResult = null;

            for (int i = 0; i < 2; i++)
            {
                if (preferIPv4)
                {
                    var resultIPv4 = await DNS.GetInstance().ResolveAAsync(query);

                    if (!resultIPv4.HasError)
                        return resultIPv4;
                    else
                        firstResult ??= resultIPv4;
                }
                else
                {
                    var resultIPv6 = await DNS.GetInstance().ResolveAaaaAsync(query);

                    if (!resultIPv6.HasError)
                        return resultIPv6;
                    else
                        firstResult ??= resultIPv6;
                }

                preferIPv4 = !preferIPv4;
            }

            return firstResult;
        }
    }
}
