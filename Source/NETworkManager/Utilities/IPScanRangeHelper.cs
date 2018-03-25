using NETworkManager.Models.Network;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace NETworkManager.Utilities
{
    public static class IPScanRangeHelper
    {
        public static Task<IPAddress[]> ConvertIPRangeToIPAddressesAsync(string ipRange, CancellationToken cancellationToken)
        {
            return Task.Run(() => ConvertIPRangeToIPAddresses(ipRange, cancellationToken), cancellationToken);
        }

        public static IPAddress[] ConvertIPRangeToIPAddresses(string ipRange, CancellationToken cancellationToken)
        {
            ConcurrentBag<IPAddress> bag = new ConcurrentBag<IPAddress>();

            ParallelOptions parallelOptions = new ParallelOptions()
            {
                CancellationToken = cancellationToken
            };

            foreach (string ipOrRange in ipRange.Replace(" ", "").Split(';'))
            {
                // Match 192.168.0.1
                if (Regex.IsMatch(ipOrRange, RegexHelper.IPv4AddressRegex))
                {
                    bag.Add(IPAddress.Parse(ipOrRange));
                    continue;
                }

                // Match 192.168.0.0/24
                if (Regex.IsMatch(ipOrRange, RegexHelper.IPv4AddressCidrRegex) || Regex.IsMatch(ipOrRange, RegexHelper.IPv4AddressSubnetmaskRegex))
                {
                    string[] subnet = ipOrRange.Split('/');

                    IPAddress ip = IPAddress.Parse(subnet[0]);
                    IPAddress subnetmask = int.TryParse(subnet[1], out int cidr) ? IPAddress.Parse(Subnetmask.ConvertCidrToSubnetmask(cidr)) : IPAddress.Parse(subnet[1]);

                    IPAddress networkAddress = Subnet.GetIPv4NetworkAddress(ip, subnetmask);
                    IPAddress broadcast = Subnet.GetIPv4Broadcast(ip, subnetmask);

                    Parallel.For(IPv4AddressHelper.ConvertToInt32(networkAddress), IPv4AddressHelper.ConvertToInt32(broadcast) + 1, parallelOptions, i =>
                    {
                        bag.Add(IPv4AddressHelper.ConvertFromInt32(i));

                        parallelOptions.CancellationToken.ThrowIfCancellationRequested();
                    });

                    continue;
                }

                // Match 192.168.0.0 - 192.168.0.100
                if (Regex.IsMatch(ipOrRange, RegexHelper.IPv4AddressRangeRegex))
                {
                    string[] range = ipOrRange.Split('-');

                    Parallel.For(IPv4AddressHelper.ConvertToInt32(IPAddress.Parse(range[0])), IPv4AddressHelper.ConvertToInt32(IPAddress.Parse(range[1])) + 1, parallelOptions, i =>
                    {
                        bag.Add(IPv4AddressHelper.ConvertFromInt32(i));

                        parallelOptions.CancellationToken.ThrowIfCancellationRequested();
                    });

                    continue;
                }

                // Convert 192.168.[50-100,200].1 to 192.168.50.1, 192.168.51.1, 192.168.52.1, {..}, 192.168.200.1
                if (Regex.IsMatch(ipOrRange, RegexHelper.IPv4AddressSpecialRangeRegex))
                {
                    string[] octets = ipOrRange.Split('.');

                    List<List<int>> list = new List<List<int>>();

                    // Go through each octet...
                    for (int i = 0; i < octets.Length; i++)
                    {
                        List<int> innerList = new List<int>();

                        // Create a range for each octet
                        if (Regex.IsMatch(octets[i], RegexHelper.SpecialRangeRegex))
                        {
                            foreach (string numberOrRange in octets[i].Substring(1, octets[i].Length - 2).Split(','))
                            {
                                // 50-100
                                if (numberOrRange.Contains("-"))
                                {
                                    string[] rangeNumbers = numberOrRange.Split('-');

                                    for (int j = int.Parse(rangeNumbers[0]); j < (int.Parse(rangeNumbers[1]) + 1); j++)
                                    {
                                        innerList.Add(j);
                                    }
                                } // 200
                                else
                                {
                                    innerList.Add(int.Parse(numberOrRange));
                                }
                            }
                        }
                        else
                        {
                            innerList.Add(int.Parse(octets[i]));
                        }

                        list.Add(innerList);
                    }

                    // Build the new ipv4
                    foreach (int i in list[0])
                    {
                        foreach (int j in list[1])
                        {
                            foreach (int k in list[2])
                            {
                                foreach (int h in list[3])
                                {
                                    bag.Add(IPAddress.Parse(string.Format("{0}.{1}.{2}.{3}", i, j, k, h)));
                                }
                            }
                        }
                    }

                    continue;
                }
            }

            return bag.ToArray();
        }
    }
}