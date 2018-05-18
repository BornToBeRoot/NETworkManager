using NETworkManager.Models.Network;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace NETworkManager.Utilities
{
    public static class IPScanRangeHelper
    {
        public static Task<IPAddress[]> ConvertIPRangeToIPAddressesAsync(string[] ipRanges, CancellationToken cancellationToken)
        {
            return Task.Run(() => ConvertIPRangeToIPAddresses(ipRanges, cancellationToken), cancellationToken);
        }

        public static IPAddress[] ConvertIPRangeToIPAddresses(string[] ipRanges, CancellationToken cancellationToken)
        {
            ConcurrentBag<IPAddress> bag = new ConcurrentBag<IPAddress>();

            ParallelOptions parallelOptions = new ParallelOptions()
            {
                CancellationToken = cancellationToken
            };

            foreach (string ipOrRange in ipRanges)
            {
                // Match 192.168.0.1
                if (Regex.IsMatch(ipOrRange, RegexHelper.IPv4AddressRegex))
                {
                    bag.Add(IPAddress.Parse(ipOrRange));
                    continue;
                }

                // Match 192.168.0.0/24 or 192.168.0.0/255.255.255.0
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

        public static Task<List<string>> ResolveHostnamesInIPRangeAsync(string[] ipHostOrRanges, CancellationToken cancellationToken)
        {
            return Task.Run(() => ResolveHostnamesInIPRange(ipHostOrRanges, cancellationToken), cancellationToken);
        }

        public static List<string> ResolveHostnamesInIPRange(string[] ipHostOrRanges, CancellationToken cancellationToken)
        {
            ConcurrentBag<string> bag = new ConcurrentBag<string>();

            ParallelOptions parallelOptions = new ParallelOptions()
            {
                CancellationToken = cancellationToken
            };

            ConcurrentQueue<HostNotFoundException> exceptions = new ConcurrentQueue<HostNotFoundException>();

            Parallel.ForEach(ipHostOrRanges, new ParallelOptions() { CancellationToken = cancellationToken }, ipHostOrRange =>
            {
                // like 192.168.0.1, 192.168.0.0/24, 192.168.0.0/255.255.255.0, 192.168.0.0 - 192.168.0.100, 192.168.[50-100].1
                if (Regex.IsMatch(ipHostOrRange, RegexHelper.IPv4AddressRegex) || Regex.IsMatch(ipHostOrRange, RegexHelper.IPv4AddressCidrRegex) || Regex.IsMatch(ipHostOrRange, RegexHelper.IPv4AddressSubnetmaskRegex) || Regex.IsMatch(ipHostOrRange, RegexHelper.IPv4AddressRangeRegex) || Regex.IsMatch(ipHostOrRange, RegexHelper.IPv4AddressSpecialRangeRegex))
                {
                    bag.Add(ipHostOrRange);
                } // like fritz.box, fritz.box/24 or fritz.box/255.255.255.128
                else if (Regex.IsMatch(ipHostOrRange, RegexHelper.HostnameRegex) || Regex.IsMatch(ipHostOrRange, RegexHelper.HostnameCidrRegex) || Regex.IsMatch(ipHostOrRange, RegexHelper.HostnameSubnetmaskRegex))
                {
                    IPHostEntry ipHostEntrys = null;

                    string[] hostAndSubnet = ipHostOrRange.Split('/');

                    try
                    {
                        ipHostEntrys = Dns.GetHostEntry(hostAndSubnet[0]);
                    }
                    catch (SocketException)
                    {
                        exceptions.Enqueue(new HostNotFoundException(hostAndSubnet[0]));
                        return;
                    }

                    IPAddress ipAddress = null;

                    foreach (IPAddress ip in ipHostEntrys.AddressList)
                    {
                        if (ip.AddressFamily == AddressFamily.InterNetwork)
                        {
                            ipAddress = ip;
                            break;
                        }
                    }

                    if (ipAddress == null)
                        exceptions.Enqueue(new HostNotFoundException(hostAndSubnet[0]));

                    if (ipHostOrRange.Contains('/'))
                        bag.Add(string.Format("{0}/{1}",ipAddress.ToString(), hostAndSubnet[1]));
                    else
                        bag.Add(ipAddress.ToString());
                }
            });

            if (exceptions.Count > 0)
                throw new AggregateException(exceptions);

            return bag.ToList();
        }
    }
}