using System.Threading.Tasks;

namespace NETworkManager.Utilities;

public static class DNSClientHelper
{
    public static async Task<DNSClientResultIPAddress> ResolveAorAaaaAsync(string query, bool preferIPv4)
    {
        DNSClientResultIPAddress firstResult = null;

        for (var i = 0; i < 2; i++)
        {
            if (preferIPv4)
            {
                var resultIPv4 = await DNSClient.GetInstance().ResolveAAsync(query);

                if (!resultIPv4.HasError)
                    return resultIPv4;
                firstResult ??= resultIPv4;
            }
            else
            {
                var resultIPv6 = await DNSClient.GetInstance().ResolveAaaaAsync(query);

                if (!resultIPv6.HasError)
                    return resultIPv6;
                firstResult ??= resultIPv6;
            }

            preferIPv4 = !preferIPv4;
        }

        return firstResult;
    }

    public static string FormatDNSClientResultError(string query, DNSClientResult result)
    {
        var statusMessage = $"{query}";

        if (string.IsNullOrEmpty(result.DNSServer))
            statusMessage += $" ==> {result.ErrorMessage}";
        else
            statusMessage += $" @ {result.DNSServer} ==> {result.ErrorMessage}";

        return statusMessage;
    }
}