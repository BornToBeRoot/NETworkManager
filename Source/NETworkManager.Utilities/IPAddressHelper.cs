using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace NETworkManager.Utilities;

public static class IPAddressHelper
{
    /// <summary>
    ///     Checks if a host is an IP address: an IPv4 address in the full form (shortened forms like "10" are host
    ///     names) or an IPv6 address.
    /// </summary>
    /// <param name="host">Host to check.</param>
    /// <returns>True if the host is an IP address.</returns>
    public static bool IsIPAddress(string host)
    {
        return !string.IsNullOrWhiteSpace(host) &&
               (RegexHelper.IPv4AddressRegex().IsMatch(host.Trim()) ||
                host.Contains(':') && IPAddress.TryParse(host.Trim(), out var ipAddress) &&
                ipAddress.AddressFamily == AddressFamily.InterNetworkV6);
    }

    /// <summary>
    ///     Checks if all hosts (separated by ";", empty entries like a trailing ";" are ignored) are IP addresses (see
    ///     <see cref="IsIPAddress" />).
    /// </summary>
    /// <param name="hosts">Hosts to check.</param>
    /// <returns>True if there is at least one host and all hosts are IP addresses.</returns>
    public static bool AreIPAddresses(string hosts)
    {
        var entries = (hosts ?? string.Empty).Split(';', StringSplitOptions.RemoveEmptyEntries |
                                                         StringSplitOptions.TrimEntries);

        return entries.Length > 0 && entries.All(IsIPAddress);
    }

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
    ///     Returns the IP address without the IPv6 scope id (e.g. "fe80::1%12" -> "fe80::1"), so it can be compared
    ///     with (or used as key for) an address that was entered without one. IPv4 addresses are returned as is.
    /// </summary>
    /// <param name="ipAddress">IP address.</param>
    /// <returns>IP address without scope id.</returns>
    public static IPAddress WithoutScopeId(IPAddress ipAddress)
    {
        return ipAddress.AddressFamily == AddressFamily.InterNetworkV6 && ipAddress.ScopeId != 0
            ? new IPAddress(ipAddress.GetAddressBytes())
            : ipAddress;
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