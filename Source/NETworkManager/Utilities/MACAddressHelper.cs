using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace NETworkManager.Utilities
{
    public static class MACAddressHelper
    {
        /// <summary>
        /// Convert a MAC-Address to a byte array
        /// </summary>
        /// <param name="macAddress"></param>
        /// <returns></returns>
        public static byte[] ConvertStringToByteArray(string macAddress)
        {
            // Regex to replace "-" and ":" in MAC-Address
            var regex = new Regex("[-|:|.]");
            var mac = regex.Replace(macAddress, "");

            // Build the byte-array
            var bytes = new byte[mac.Length / 2];

            // Convert the MAC-Address into byte and fill it...
            for (var i = 0; i < 12; i += 2)
                bytes[i / 2] = Convert.ToByte(mac.Substring(i, 2), 16);

            return bytes;
        }

        public static string GetDefaultFormat(string macAddress)
        {
            macAddress = macAddress.ToUpper();

            if (macAddress.Contains("-"))
                return macAddress.Replace("-", ":");

            return !macAddress.Contains(":") ? string.Join(":", Enumerable.Range(0, 6).Select(i => macAddress.Substring(i * 2, 2))) : macAddress;
        }

        public static string GetDefaultFormat(PhysicalAddress macAddress)
        {
            return GetDefaultFormat(macAddress.ToString());
        }

        public static string Format(string macAddress, string separator = "")
        {
            macAddress = macAddress.ToUpper().Replace("-", "").Replace(":", "");

            return string.Join(separator, Enumerable.Range(0, 6).Select(i => macAddress.Substring(i * 2, 2)));
        }
    }
}
