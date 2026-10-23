using System;
using System.Net;

namespace NETworkManager.Utilities;

public static class IPAddressHelper
{
    /// <summary>
    ///     Test if an IP address is a private address.
    /// </summary>
    /// <param name="ipAddress">IP address.</param>
    /// <returns>True if the IP address is a private address, otherwise false.</returns>
    public static bool IsPrivateIPAddress(IPAddress ipAddress)
    {
        var addressBytes = ipAddress.GetAddressBytes();

        switch (addressBytes.Length)
        {
            // Check for IPv4 private addresses
            case 4 when addressBytes[0] == 10
                        || (addressBytes[0] == 172 && addressBytes[1] >= 16 && addressBytes[1] <= 31)
                        || (addressBytes[0] == 192 && addressBytes[1] == 168):
            // Check for IPv6 unique local addresses (ULA)
            case 16 when addressBytes[0] == 0xFC || addressBytes[0] == 0xFD:
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    ///     Compare two IP addresses.
    /// </summary>
    /// <param name="x">First IP address.</param>
    /// <param name="y">Second IP address.</param>
    /// <returns>0 if the IP addresses are equal, otherwise a negative or positive value.</returns>
    public static int CompareIPAddresses(IPAddress x, IPAddress y)
    {
        // Write the bytes to the stack instead of allocating two arrays per call (GetAddressBytes),
        // this is called very often when sorting large lists (e.g. the hosts of an IP Scanner range)
        Span<byte> xBytes = stackalloc byte[16];
        Span<byte> yBytes = stackalloc byte[16];

        x.TryWriteBytes(xBytes, out var xLength);
        y.TryWriteBytes(yBytes, out var yLength);

        return ByteHelper.Compare(xBytes[..xLength], yBytes[..yLength]);
    }
}