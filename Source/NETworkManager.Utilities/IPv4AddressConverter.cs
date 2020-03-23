using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace NETworkManager.Utilities
{
    /// <summary>
    /// Class provides static methods to convert an IPv4 address.
    /// </summary>
    public static class IPv4AddressConverter
    {
        /// <summary>
        /// Method to convert a IPv4 address binary string ("11000000.10101000.00000001.00000001") to human readable string ("192.168.1.1").
        /// </summary>
        /// <param name="binaryString">IPv4 address as binary string ("11111111111111111111111100000000").</param>
        /// <returns>Converted IPv4 address as human readable string ("255.255.255.0").</returns>
        public static string ToHumanString(string binaryString)
        {
            var ipv4AddressBinary = binaryString.Replace(".", "");

            var ipv4AddressOctets = new List<string>();

            for (var i = 0; i < ipv4AddressBinary.Length; i += 8)
                ipv4AddressOctets.Add(Convert.ToInt32(ipv4AddressBinary.Substring(i, 8), 2).ToString());

            return string.Join(".", ipv4AddressOctets);
        }

        /// <summary>
        /// Method to convert an human readable IPv4 address ("192.168.1.1") to binary string ("11000000.10101000.00000001.00000001").
        /// </summary>
        /// <param name="humanString">IPv4 address as human readable string ("192.168.1.1").</param>
        /// <returns>Converted IPv4-Address as binary string ("11000000.10101000.00000001.00000001").</returns>
        public static string ToBinaryString(string humanString)
        {
            return string.Join(".", humanString.Split('.').Select(x => Convert.ToString(int.Parse(x), 2).PadLeft(8, '0')).ToArray());
        }

        /// <summary>
        /// Method to convert an <see cref="IPAddress"/> to <see cref="int"/>.
        /// </summary>
        /// <param name="ipAddress"><see cref="IPAddress"/>.</param>
        /// <returns>IP address as <see cref="int"/></returns>

        public static int ToInt32(IPAddress ipAddress)
        {
            var bytes = ipAddress.GetAddressBytes();

            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return BitConverter.ToInt32(bytes, 0);
        }

        /// <summary>
        /// Method to convert an <see cref="int"/> to an <see cref="IPAddress"/>.
        /// </summary>
        /// <param name="i">IP address as <see cref="int"/>.</param>
        /// <returns>Converted <see cref="IPAddress"/>.</returns>
        public static IPAddress FromInt32(int i)
        {
            var bytes = BitConverter.GetBytes(i);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return new IPAddress(bytes);
        }      
    }
}
