using NETworkManager.Utilities;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace NETworkManager.Models.Network;

/// <summary>
///     Class contains information about a server.
/// </summary>
public class ServerConnectionInfo
{
    /// <summary>
    ///     Create an instance of <see cref="ServerConnectionInfo" />.
    /// </summary>
    public ServerConnectionInfo()
    {

    }

    /// <summary>
    ///     Create an instance of <see cref="ServerConnectionInfo" /> with parameters. Default transport protocol is TCP.
    /// </summary>
    /// <param name="server">Server name or IP address.</param>
    /// <param name="port">Port used for the connection.</param>
    public ServerConnectionInfo(string server, int port)
    {
        Server = server;
        Port = port;
        TransportProtocol = TransportProtocol.Tcp;
    }

    /// <summary>
    ///     Create an instance of <see cref="ServerConnectionInfo" /> with parameters.
    /// </summary>
    /// <param name="server">Server name or IP address.</param>
    /// <param name="port">Port used for the connection.</param>
    /// <param name="transportProtocol">Transport protocol used for the connection.</param>
    public ServerConnectionInfo(string server, int port, TransportProtocol transportProtocol)
    {
        Server = server;
        Port = port;
        TransportProtocol = transportProtocol;
    }

    /// <summary>
    ///     Server name or IP address.
    /// </summary>
    public string Server { get; set; }

    /// <summary>
    ///     Port used for the connection.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    ///     Transport protocol used for the connection.
    /// </summary>
    public TransportProtocol TransportProtocol { get; set; }

    /// <summary>
    ///     Tries to parse a server connection string into a <see cref="ServerConnectionInfo" /> object.
    ///     Supports formats: IPv4, IPv4:port, IPv6, [IPv6], [IPv6]:port, hostname, hostname:port
    /// </summary>
    /// <param name="input">Server connection string to parse.</param>
    /// <param name="serverConnectionInfo">Parsed <see cref="ServerConnectionInfo" /> object if successful.</param>
    /// <param name="defaultPort">Default port to use if not specified in input.</param>
    /// <param name="transportProtocol">Transport protocol to set in the parsed object.</param>
    /// <returns>True if parsing was successful; otherwise, false.</returns>
    public static bool TryParse(string input, out ServerConnectionInfo serverConnectionInfo, int defaultPort, TransportProtocol transportProtocol)
    {
        serverConnectionInfo = null;

        if (string.IsNullOrWhiteSpace(input))
            return false;

        input = input.Trim();

        Debug.WriteLine("Parse server connection info input: " + input);

        // [IPv6]:Port
        if (input.StartsWith('[') && input.Contains("]:"))
        {
            var endIndex = input.IndexOf("]:", StringComparison.Ordinal);
            var ipPart = input[1..endIndex];
            var portPart = input[(endIndex + 2)..];

            Debug.WriteLine($"IPv6 with port detected. IP: {ipPart}, Port: {portPart}");

            if (IPAddress.TryParse(ipPart, out IPAddress ip) && ip.AddressFamily == AddressFamily.InterNetworkV6 &&
                int.TryParse(portPart, out int port) && port > 0 && port <= 65535)
            {
                serverConnectionInfo = new ServerConnectionInfo(ip.ToString(), port, transportProtocol);
                return true;
            }
        }
        // [IPv6]
        else if (input.StartsWith('[') && input.EndsWith(']'))
        {
            var ipPart = input[1..^1];

            Debug.WriteLine($"IPv6 without port detected. IP: {ipPart}");

            if (IPAddress.TryParse(ipPart, out IPAddress ip) && ip.AddressFamily == AddressFamily.InterNetworkV6)
            {
                serverConnectionInfo = new ServerConnectionInfo(ip.ToString(), defaultPort, transportProtocol);
                return true;
            }
        }
        // IPv6 without brackets (contains multiple colons)
        else if (input.Count(c => c == ':') > 1)
        {
            Debug.WriteLine($"IPv6 without port detected. IP: {input}");

            if (IPAddress.TryParse(input, out IPAddress ip) && ip.AddressFamily == AddressFamily.InterNetworkV6)
            {
                serverConnectionInfo = new ServerConnectionInfo(ip.ToString(), defaultPort, transportProtocol);
                return true;
            }
        }
        // IPv4/hostname:port (single colon)
        else if (input.Contains(':'))
        {
            var parts = input.Split([':'], 2);

            Debug.WriteLine($"IPv4 or Hostname with port detected. Host: {parts[0]}, Port: {parts[1]}");

            if ((RegexHelper.IPv4AddressRegex().IsMatch(parts[0]) || RegexHelper.HostnameOrDomainRegex().IsMatch(parts[0])) &&
                int.TryParse(parts[1], out int port) && port > 0 && port <= 65535)
            {
                serverConnectionInfo = new ServerConnectionInfo(parts[0], port, transportProtocol);
                return true;
            }
        }
        // IPv4/hostname
        else
        {
            if (RegexHelper.IPv4AddressRegex().IsMatch(input) || RegexHelper.HostnameOrDomainRegex().IsMatch(input))
            {
                Debug.WriteLine($"IPv4 or Hostname without port detected. Host: {input}");

                serverConnectionInfo = new ServerConnectionInfo(input, defaultPort, transportProtocol);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Parses a server connection string into a <see cref="ServerConnectionInfo" /> object.
    ///     Supports formats: IPv4, IPv4:port, IPv6, [IPv6], [IPv6]:port, hostname, hostname:port
    /// </summary>
    /// <param name="input">Server connection string to parse.</param>
    /// <param name="defaultPort">Default port to use if not specified in input.</param>
    /// <returns>Parsed <see cref="ServerConnectionInfo" /> object.</returns>
    /// <exception cref="FormatException">Thrown when the input string is not in a valid format.</exception>
    public static ServerConnectionInfo Parse(string input, int defaultPort, TransportProtocol transportProtocol)
    {
        if (TryParse(input, out var serverConnectionInfo, defaultPort, transportProtocol))
            return serverConnectionInfo;

        throw new FormatException($"Could not parse server connection info from input: {input}");
    }

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>Server:Port</returns>
    public override string ToString()
    {
        return $"{Server}:{Port}";
    }
}