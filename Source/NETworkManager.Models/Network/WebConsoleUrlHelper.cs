using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using NETworkManager.Utilities;

namespace NETworkManager.Models.Network;

/// <summary>
///     Creates the URL to open a host in the Web Console from scan results (IP Scanner, Port Scanner).
/// </summary>
public static class WebConsoleUrlHelper
{
    /// <summary>
    ///     Web ports in the order of preference (https before http, default ports first).
    /// </summary>
    private static readonly int[] WebPorts = [443, 80, 8443, 8080];

    /// <summary>
    ///     Checks if a port is a web port (80, 443, 8080, 8443).
    /// </summary>
    /// <param name="port">Port number.</param>
    /// <returns>True if it is a web port.</returns>
    public static bool IsWebPort(int port)
    {
        return WebPorts.Contains(port);
    }

    /// <summary>
    ///     Creates the URL for a host: the preferred open web port (443, 80, 8443, 8080), <c>http://host/</c> if
    ///     none of them is open.
    /// </summary>
    /// <param name="host">Hostname or IP address.</param>
    /// <param name="openPorts">Open ports of the host.</param>
    /// <returns>URL.</returns>
    public static string CreateUrl(string host, IEnumerable<int> openPorts)
    {
        var ports = openPorts.ToHashSet();

        var port = WebPorts.FirstOrDefault(ports.Contains);

        return CreateUrl(host, port == 0 ? 80 : port);
    }

    /// <summary>
    ///     Creates the URL for a host and a web port: https for 443 / 8443, otherwise http; default ports (80 / 443)
    ///     are left out, IPv6 addresses in brackets, the trailing dot of a hostname is removed.
    /// </summary>
    /// <param name="host">Hostname or IP address.</param>
    /// <param name="port">Port number.</param>
    /// <returns>URL.</returns>
    public static string CreateUrl(string host, int port)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);

        var scheme = port is 443 or 8443 ? "https" : "http";

        // The zone id of an IPv6 address has to be encoded ("%" -> "%25", RFC 6874)
        if (IPAddress.TryParse(host, out var ipAddress))
        {
            if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
                host = $"[{ipAddress.ToString().Replace("%", "%25")}]";
        }
        else
        {
            // The browser resolves it via Windows (e.g. "gateway." -> "gateway")
            host = DNSClientHelper.SanitizeHostname(host);
        }

        return port is 80 or 443 ? $"{scheme}://{host}/" : $"{scheme}://{host}:{port}/";
    }
}
