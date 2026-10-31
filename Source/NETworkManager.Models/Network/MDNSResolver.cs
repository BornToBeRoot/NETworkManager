using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network;

/// <summary>
///     Resolves the mDNS hostname (e.g. "macbook.local") of a host: reverse lookup (<c>PTR</c>) sent as unicast query
///     to UDP port 5353 of the host (RFC 6762: a query from a port other than 5353 is answered with a unicast
///     response to the sender, which repeats the ID and the question). Answered by Apple devices, Linux (Avahi),
///     Android, printers and many IoT devices - usually only for requests from the local subnet.
/// </summary>
public static class MDNSResolver
{
    private const int MDNSPort = 5353;

    private const ushort TypePTR = 12;
    private const ushort ClassIN = 1;

    // Maximum number of compression pointers followed in a name (protection against loops)
    private const int MaxCompressionPointers = 32;

    /// <summary>
    ///     Resolves the mDNS hostname of a host.
    /// </summary>
    /// <param name="ipAddress">IP address of the host.</param>
    /// <param name="timeout">Timeout in milliseconds to wait for the answer.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Hostname without trailing dot (e.g. "macbook.local"), empty if the host didn't answer.</returns>
    public static async Task<string> ResolveAsync(IPAddress ipAddress, int timeout,
        CancellationToken cancellationToken)
    {
        try
        {
            var id = (ushort)Random.Shared.Next(1, ushort.MaxValue);

            var response = await UdpRequest.SendAsync(ipAddress, MDNSPort, CreateReverseQuery(ipAddress, id),
                timeout, cancellationToken).ConfigureAwait(false);

            return response == null ? string.Empty : ParsePTRResponse(response, id);
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    /// <summary>
    ///     Gets the reverse lookup name of an IP address (e.g. "4.3.2.1.in-addr.arpa", nibbles + "ip6.arpa" for IPv6).
    /// </summary>
    /// <param name="ipAddress">IP address.</param>
    /// <returns>Reverse lookup name.</returns>
    public static string GetReverseName(IPAddress ipAddress)
    {
        var bytes = ipAddress.GetAddressBytes();

        if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
            return $"{bytes[3]}.{bytes[2]}.{bytes[1]}.{bytes[0]}.in-addr.arpa";

        var builder = new StringBuilder();

        for (var i = bytes.Length - 1; i >= 0; i--)
            builder.Append($"{bytes[i] & 0x0F:x}.{bytes[i] >> 4:x}.");

        return builder.Append("ip6.arpa").ToString();
    }

    /// <summary>
    ///     Creates a DNS query for the <c>PTR</c> record of the reverse lookup name of an IP address.
    /// </summary>
    /// <param name="ipAddress">IP address to look up.</param>
    /// <param name="id">ID of the query (repeated in the answer).</param>
    /// <returns>DNS message.</returns>
    public static byte[] CreateReverseQuery(IPAddress ipAddress, ushort id)
    {
        var query = new List<byte>
        {
            // Header: ID, flags (standard query), 1 question, no answer / authority / additional records
            (byte)(id >> 8), (byte)id, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };

        foreach (var label in GetReverseName(ipAddress).Split('.'))
        {
            query.Add((byte)label.Length);
            query.AddRange(Encoding.ASCII.GetBytes(label));
        }

        query.AddRange([0x00, TypePTR >> 8, TypePTR & 0xFF, ClassIN >> 8, ClassIN & 0xFF]);

        return [.. query];
    }

    /// <summary>
    ///     Gets the hostname from the answer to <see cref="CreateReverseQuery" />: the first <c>PTR</c> record of the
    ///     answer section.
    /// </summary>
    /// <param name="response">DNS message of the answer.</param>
    /// <param name="id">ID of the query.</param>
    /// <returns>
    ///     Hostname without trailing dot, empty if the answer is invalid or contains no <c>PTR</c> record.
    /// </returns>
    public static string ParsePTRResponse(byte[] response, ushort id)
    {
        // Header (12 bytes), must be a response (QR bit) to the query (ID, some responders use 0)
        if (response.Length < 12 || (response[2] & 0x80) == 0)
            return string.Empty;

        var responseId = (ushort)((response[0] << 8) | response[1]);

        if (responseId != id && responseId != 0)
            return string.Empty;

        var questions = (response[4] << 8) | response[5];
        var answers = (response[6] << 8) | response[7];

        var offset = 12;

        // Skip the question(s): name, type, class
        for (var i = 0; i < questions; i++)
        {
            if (ReadName(response, ref offset) == null || offset + 4 > response.Length)
                return string.Empty;

            offset += 4;
        }

        for (var i = 0; i < answers; i++)
        {
            // Name, type, class (cache flush bit in mDNS), TTL, data length, data
            if (ReadName(response, ref offset) == null || offset + 10 > response.Length)
                return string.Empty;

            var type = (response[offset] << 8) | response[offset + 1];
            var dataLength = (response[offset + 8] << 8) | response[offset + 9];

            offset += 10;

            if (offset + dataLength > response.Length)
                return string.Empty;

            if (type == TypePTR)
            {
                var dataOffset = offset;

                return ReadName(response, ref dataOffset) ?? string.Empty;
            }

            offset += dataLength;
        }

        return string.Empty;
    }

    /// <summary>
    ///     Reads a (compressed) domain name at <paramref name="offset" /> and moves the offset behind it.
    /// </summary>
    /// <param name="message">DNS message.</param>
    /// <param name="offset">Offset of the name, behind the name afterwards.</param>
    /// <returns>Name without trailing dot, null if the name is invalid.</returns>
    private static string ReadName(byte[] message, ref int offset)
    {
        var labels = new List<string>();
        var position = offset;
        var endOffset = -1;
        var pointers = 0;

        while (true)
        {
            if (position >= message.Length)
                return null;

            var length = message[position];

            // End of the name
            if (length == 0)
            {
                position++;
                break;
            }

            // Compression pointer (2 bytes) to a name earlier in the message
            if ((length & 0xC0) == 0xC0)
            {
                if (position + 1 >= message.Length || ++pointers > MaxCompressionPointers)
                    return null;

                // The name ends after the first pointer
                if (endOffset < 0)
                    endOffset = position + 2;

                position = ((length & 0x3F) << 8) | message[position + 1];
                continue;
            }

            if ((length & 0xC0) != 0 || position + 1 + length > message.Length)
                return null;

            labels.Add(Encoding.UTF8.GetString(message, position + 1, length));
            position += 1 + length;
        }

        offset = endOffset >= 0 ? endOffset : position;

        return string.Join(".", labels);
    }
}
