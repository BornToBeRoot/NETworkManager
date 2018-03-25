using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace NETworkManager.Utilities
{
    public static class IPv4AddressHelper
    {
        public const int IPv4MulticastStart = -536870912;
        public const int IPv4MulticastEnd = -268435457;

        /// <summary>
        /// Convert a binary IPv4-Address into a human readable string.
        /// </summary>
        /// <param name="s">11111111.11111111.11111111.00000000 or 11111111111111111111111100000000</param>
        /// <returns>255.255.255.0</returns>
        public static string BinaryStringToHumanString(string s)
        {
            // Replace all dots
            string ipv4AddressBinary = s.Replace(".", "");

            // List to store octets temporary
            List<string> ipv4AddressOctets = new List<string>();

            // Split the binary IPv4-Address into 4 octets and convert them into decimal numbers
            for (int i = 0; i < ipv4AddressBinary.Length; i += 8)
                ipv4AddressOctets.Add(Convert.ToInt32(ipv4AddressBinary.Substring(i, 8), 2).ToString());

            // Seperate each octet with a dot
            return string.Join(".", ipv4AddressOctets);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s">192.168.1.1</param>
        /// <returns>11000000.10101000.00000001.00000001</returns>
        public static string HumanStringToBinaryString(string s)
        {
            return string.Join(".", (s.Split('.').Select(x => Convert.ToString(int.Parse(x), 2).PadLeft(8, '0'))).ToArray());
        }

        public static int ConvertToInt32(IPAddress ipAddress)
        {
            byte[] bytes = ipAddress.GetAddressBytes();

            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return BitConverter.ToInt32(bytes, 0);
        }

        public static IPAddress ConvertFromInt32(int i)
        {
            byte[] bytes = BitConverter.GetBytes(i);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return new IPAddress(bytes);
        }

        public static IPAddress IncrementIPv4Address(IPAddress ipAddress, int i)
        {
            return ConvertFromInt32(ConvertToInt32(ipAddress) + i);
        }

        public static IPAddress DecrementIPv4Address(IPAddress ipAddress, int i)
        {
            return ConvertFromInt32(ConvertToInt32(ipAddress) - i);
        }

        public static bool IsMulticast(IPAddress ipAddress)
        {
            int ip = ConvertToInt32(ipAddress);

            return (ip >= IPv4MulticastStart && ip <= IPv4MulticastEnd);
        }
    }
}
