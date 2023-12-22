using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace NETworkManager.Utilities;

public static class MACAddressHelper
{
    /// <summary>
    /// Convert a MAC-Address to a byte array
    /// </summary>
    /// <param name="macAddress">MAC-Address to convert</param>
    /// <returns>Byte array of the MAC-Address</returns>
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


    /// <summary>
    /// Format a MAC-Address to the default format (00:00:00:00:00:00).
    /// </summary>
    /// <param name="macAddress">MAC-Address to format</param>
    /// <returns>MAC-Address in default format</returns>
    public static string GetDefaultFormat(string macAddress)
    {
        return Format(macAddress, ":");
    }

    /// <summary>
    /// Format a MAC-Address to a specific format (e.g. 00:00:00:00:00:00, 00-00-00-00-00-00, 0000.0000.0000 or 000000000000)
    /// </summary>
    /// <param name="macAddress">MAC-Address to format</param>
    /// <param name="separator">Separator to use (e.g. -, :, . or empty)</param>
    /// <param name="toUpper">Convert the MAC-Address to upper case</param>
    /// <returns></returns>
    public static string Format(string macAddress, string separator = "", bool toUpper = true)
    {
        macAddress = macAddress.Replace("-", "")
            .Replace(":", "")
            .Replace(".", "");

        if(toUpper)
            macAddress = macAddress.ToUpper();

        return separator switch
        {
            "-" => string.Join(separator, Enumerable.Range(0, 6).Select(i => macAddress.Substring(i * 2, 2))),
            ":" => string.Join(separator, Enumerable.Range(0, 6).Select(i => macAddress.Substring(i * 2, 2))),
            "." => string.Join(separator, Enumerable.Range(0, 3).Select(i => macAddress.Substring(i * 4, 4))),
            _ => macAddress
        };
    }


    /// <summary>
    /// Compare two MAC-Addresses.
    /// </summary>
    /// <param name="x">First MAC-Address.</param>
    /// <param name="y">Second MAC-Address.</param>
    /// <returns>0 if the MAC-Addresses are equal, otherwise a negative or positive value.</returns>
    public static int CompareMACAddresses(PhysicalAddress x, PhysicalAddress y)
    {
        return ByteHelper.Compare(x.GetAddressBytes(), y.GetAddressBytes());
    }
}