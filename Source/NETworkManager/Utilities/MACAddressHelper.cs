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
            Regex regex = new Regex("[-|:|.]");
            string mac = regex.Replace(macAddress, "");

            // Build the byte-array
            byte[] bytes = new byte[mac.Length / 2];

            // Convert the MAC-Address into byte and fill it...
            for (int i = 0; i < 12; i += 2)
                bytes[i / 2] = Convert.ToByte(mac.Substring(i, 2), 16);

            return bytes;
        }

        public static string GetDefaultFormat(string macAddress)
        {
            macAddress = macAddress.ToUpper();

            if (macAddress.Contains("-"))
                return macAddress.Replace("-", ":");

            if (!macAddress.Contains(":"))
                return string.Join(":", Enumerable.Range(0, 6).Select(i => macAddress.Substring(i * 2, 2)));

            return macAddress;
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
