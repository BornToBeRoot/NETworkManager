using System.Collections.Concurrent;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NETworkManager.Helpers;


namespace NETworkManager.Helpers
{
    public static class IPScanRangeHelper
    {

        public static Task<IPAddress[]> ConvertIPRangeToIPAddressArrayAsync(string ipRange, CancellationToken cancellationToken)
        {
            return Task.Run(() => ConvertIPRangeToIPAddressArray(ipRange, cancellationToken), cancellationToken);
        }

        public static IPAddress[] ConvertIPRangeToIPAddressArray(string ipRange, CancellationToken cancellationToken)
        {
            ConcurrentBag<IPAddress> bag = new ConcurrentBag<IPAddress>();

            ParallelOptions parallelOptions = new ParallelOptions()
            {
                CancellationToken = cancellationToken
            };

            foreach (string ipOrRange in ipRange.Replace(" ", "").Split(';'))
            {
                if (Regex.IsMatch(ipOrRange, RegexHelper.IPv4AddressRegex))
                    bag.Add(IPAddress.Parse(ipOrRange));

                if (Regex.IsMatch(ipOrRange, RegexHelper.IPv4AddressCidrRegex))
                {
                    string[] subnet = ipOrRange.Split('/');

                    IPAddress ip = IPAddress.Parse(subnet[0]);
                    IPAddress subnetmask = IPAddress.Parse(SubnetmaskHelper.ConvertCidrToSubnetmask(int.Parse(subnet[1])));

                    IPAddress networkAddress = SubnetHelper.GetIPv4NetworkAddress(ip, subnetmask);
                    IPAddress broadcast = SubnetHelper.GetIPv4Broadcast(ip, subnetmask);

                    Parallel.For(IPv4AddressHelper.ConvertToInt32(networkAddress), IPv4AddressHelper.ConvertToInt32(broadcast) + 1, parallelOptions, i =>
                    {
                        bag.Add(IPv4AddressHelper.ConvertFromInt32(i));

                        parallelOptions.CancellationToken.ThrowIfCancellationRequested();
                    });
                }

                if (Regex.IsMatch(ipOrRange, RegexHelper.IPv4AddressSubnetmaskRegex))
                {
                    string[] subnet = ipOrRange.Split('/');

                    IPAddress ip = IPAddress.Parse(subnet[0]);
                    IPAddress subnetmask = IPAddress.Parse(subnet[1]);

                    IPAddress networkAddress = SubnetHelper.GetIPv4NetworkAddress(ip, subnetmask);
                    IPAddress broadcast = SubnetHelper.GetIPv4Broadcast(ip, subnetmask);

                    Parallel.For(IPv4AddressHelper.ConvertToInt32(networkAddress), IPv4AddressHelper.ConvertToInt32(broadcast) + 1, parallelOptions, i =>
                    {
                        bag.Add(IPv4AddressHelper.ConvertFromInt32(i));

                        parallelOptions.CancellationToken.ThrowIfCancellationRequested();
                    });
                }

                if (Regex.IsMatch(ipOrRange, RegexHelper.IPv4AddressRangeRegex))
                {
                    string[] range = ipOrRange.Split('-');

                    Parallel.For(IPv4AddressHelper.ConvertToInt32(IPAddress.Parse(range[0])), IPv4AddressHelper.ConvertToInt32(IPAddress.Parse(range[1])) + 1, parallelOptions, i =>
                    {
                        bag.Add(IPv4AddressHelper.ConvertFromInt32(i));

                        parallelOptions.CancellationToken.ThrowIfCancellationRequested();
                    });
                }
            }

            return bag.ToArray();
        }
    }
}