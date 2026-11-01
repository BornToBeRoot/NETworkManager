using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network;

/// <summary>
///     Reads what a host reveals in the SMB negotiation on TCP port 445 without credentials: an SMB2 <c>NEGOTIATE</c>
///     (dialect, signing, server GUID, system time) and a <c>SESSION_SETUP</c> with an NTLM <c>NEGOTIATE</c> message,
///     whose <c>CHALLENGE</c> answer contains the version of the operating system (major.minor.build), the computer and
///     domain names and the time of the host. The connection is closed after the challenge: no credentials are sent and
///     no logon is attempted (no failed logon on the host). Exception-free: null if the host doesn't answer or the
///     answer is invalid.
/// </summary>
public static class SMBInfoResolver
{
    /// <summary>
    ///     TCP port of SMB.
    /// </summary>
    public const int SMBPort = 445;

    // Max. size of an answer (NBT session header length), larger answers are invalid for a negotiate / session setup
    private const int MaxMessageSize = 64 * 1024;

    private const int HeaderSize = 64;

    private const ushort CommandNegotiate = 0x0000;
    private const ushort CommandSessionSetup = 0x0001;

    private const uint StatusSuccess = 0x00000000;
    private const uint StatusMoreProcessingRequired = 0xC0000016;

    // Dialects of the negotiate request: 2.0.2, 2.1, 3.0, 3.0.2, 3.1.1 (3.1.1 needs the preauth integrity context)
    private static readonly ushort[] Dialects = [0x0202, 0x0210, 0x0300, 0x0302, 0x0311];

    // NTLMSSP signature "NTLMSSP\0"
    private static readonly byte[] NTLMSSPSignature = "NTLMSSP\0"u8.ToArray();

    private const uint NTLMSSP_NEGOTIATE_UNICODE = 0x00000001;
    private const uint NTLMSSP_REQUEST_TARGET = 0x00000004;
    private const uint NTLMSSP_NEGOTIATE_NTLM = 0x00000200;
    private const uint NTLMSSP_NEGOTIATE_ALWAYS_SIGN = 0x00008000;
    private const uint NTLMSSP_NEGOTIATE_EXTENDED_SESSIONSECURITY = 0x00080000;
    private const uint NTLMSSP_NEGOTIATE_TARGET_INFO = 0x00800000;
    private const uint NTLMSSP_NEGOTIATE_VERSION = 0x02000000;
    private const uint NTLMSSP_NEGOTIATE_128 = 0x20000000;
    private const uint NTLMSSP_NEGOTIATE_56 = 0x80000000;

    /// <summary>
    ///     Reads the SMB information of a host.
    /// </summary>
    /// <param name="ipAddress">IP address of the host.</param>
    /// <param name="timeout">Timeout in milliseconds for the connection and both requests.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>SMB information, null if the host didn't answer the negotiate request or the answer is invalid.</returns>
    public static async Task<SMBInfo> ResolveAsync(IPAddress ipAddress, int timeout,
        CancellationToken cancellationToken)
    {
        using var timeoutCts = new CancellationTokenSource(timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        try
        {
            using var socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Close with a reset (no TIME_WAIT sockets on large scans, see PortProbe)
            socket.LingerState = new LingerOption(true, 0);
            socket.NoDelay = true;

            await socket.ConnectAsync(new IPEndPoint(ipAddress, SMBPort), linkedCts.Token).ConfigureAwait(false);

            await using var stream = new NetworkStream(socket, false);

            // Negotiate: dialect, signing, GUID, time (the round trip time corrects the time offset)
            var sentAt = DateTime.UtcNow;
            var stopwatch = Stopwatch.StartNew();

            await WriteMessageAsync(stream, CreateNegotiateRequest(Guid.NewGuid(), RandomNumberGenerator.GetBytes(32)),
                linkedCts.Token).ConfigureAwait(false);

            var negotiateResponse = await ReadMessageAsync(stream, linkedCts.Token).ConfigureAwait(false);

            // Local time when the host answered: the middle of the round trip
            var negotiateTime = sentAt + stopwatch.Elapsed / 2;

            var negotiate = ParseNegotiateResponse(negotiateResponse);

            if (negotiate == null)
                return null;

            // Session setup with an NTLM negotiate message: the answer contains the challenge (no logon)
            NTLMChallengeInfo ntlm = null;
            var sessionSetupTime = DateTime.MinValue;

            try
            {
                var sessionSetupSent = stopwatch.Elapsed;

                await WriteMessageAsync(stream, CreateSessionSetupRequest(), linkedCts.Token).ConfigureAwait(false);

                var sessionSetupResponse = await ReadMessageAsync(stream, linkedCts.Token).ConfigureAwait(false);

                sessionSetupTime = sentAt + (sessionSetupSent + stopwatch.Elapsed) / 2;

                ntlm = ParseSessionSetupResponse(sessionSetupResponse);
            }
            catch (Exception) when (!cancellationToken.IsCancellationRequested)
            {
                // No challenge (e.g. NTLM disabled, the host closed the connection or the timeout): negotiate values only
            }

            // Time of the host and the local time when it was sent: of the negotiate response, otherwise of the NTLM
            // challenge (its own round trip, not the one of the negotiate request)
            var (systemTime, localTime) = negotiate.SystemTime != null
                ? (negotiate.SystemTime, negotiateTime)
                : (ntlm?.Timestamp, sessionSetupTime);

            return new SMBInfo(negotiate.Dialect, negotiate.SigningRequired, negotiate.ServerGuid, systemTime,
                systemTime - localTime, ntlm);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            // Refused, timeout, closed connection, invalid answer
            return null;
        }
    }

    #region Requests

    /// <summary>
    ///     Creates an SMB2 <c>NEGOTIATE</c> request (dialects 2.0.2 - 3.1.1 with the preauth integrity context of
    ///     3.1.1).
    /// </summary>
    /// <param name="clientGuid">GUID of the client.</param>
    /// <param name="salt">Salt of the preauth integrity context (32 bytes).</param>
    /// <returns>SMB2 message (without the 4 byte session header).</returns>
    public static byte[] CreateNegotiateRequest(Guid clientGuid, byte[] salt)
    {
        // Body: 36 bytes + dialects, the negotiate context is 8 byte aligned (relative to the SMB2 header)
        var dialectsEnd = HeaderSize + 36 + Dialects.Length * 2;
        var contextOffset = (dialectsEnd + 7) & ~7;
        var contextDataLength = 2 + 2 + 2 + salt.Length;

        var message = new byte[contextOffset + 8 + contextDataLength];
        var span = message.AsSpan();

        WriteHeader(span, CommandNegotiate, 0);

        var body = span[HeaderSize..];
        BinaryPrimitives.WriteUInt16LittleEndian(body, 36); // StructureSize
        BinaryPrimitives.WriteUInt16LittleEndian(body[2..], (ushort)Dialects.Length);
        BinaryPrimitives.WriteUInt16LittleEndian(body[4..], 0x0001); // SecurityMode: signing enabled
        clientGuid.TryWriteBytes(body[12..]);
        BinaryPrimitives.WriteUInt32LittleEndian(body[28..], (uint)contextOffset); // NegotiateContextOffset
        BinaryPrimitives.WriteUInt16LittleEndian(body[32..], 1); // NegotiateContextCount

        for (var i = 0; i < Dialects.Length; i++)
            BinaryPrimitives.WriteUInt16LittleEndian(body[(36 + i * 2)..], Dialects[i]);

        // SMB2_PREAUTH_INTEGRITY_CAPABILITIES: SHA-512 with a salt
        var context = span[contextOffset..];
        BinaryPrimitives.WriteUInt16LittleEndian(context, 0x0001); // ContextType
        BinaryPrimitives.WriteUInt16LittleEndian(context[2..], (ushort)contextDataLength);
        BinaryPrimitives.WriteUInt16LittleEndian(context[8..], 1); // HashAlgorithmCount
        BinaryPrimitives.WriteUInt16LittleEndian(context[10..], (ushort)salt.Length);
        BinaryPrimitives.WriteUInt16LittleEndian(context[12..], 0x0001); // SHA-512
        salt.CopyTo(context[14..]);

        return message;
    }

    /// <summary>
    ///     Creates an SMB2 <c>SESSION_SETUP</c> request with an NTLM <c>NEGOTIATE</c> message (in an SPNEGO token).
    /// </summary>
    /// <returns>SMB2 message (without the 4 byte session header).</returns>
    public static byte[] CreateSessionSetupRequest()
    {
        var securityBuffer = CreateSPNEGONegTokenInit(CreateNTLMNegotiateMessage());

        const int bufferOffset = HeaderSize + 24;

        var message = new byte[bufferOffset + securityBuffer.Length];
        var span = message.AsSpan();

        WriteHeader(span, CommandSessionSetup, 1);

        var body = span[HeaderSize..];
        BinaryPrimitives.WriteUInt16LittleEndian(body, 25); // StructureSize
        body[3] = 0x01; // SecurityMode: signing enabled
        BinaryPrimitives.WriteUInt16LittleEndian(body[12..], bufferOffset); // SecurityBufferOffset
        BinaryPrimitives.WriteUInt16LittleEndian(body[14..], (ushort)securityBuffer.Length);

        securityBuffer.CopyTo(span[bufferOffset..]);

        return message;
    }

    /// <summary>
    ///     Writes an SMB2 header (sync, no session).
    /// </summary>
    private static void WriteHeader(Span<byte> span, ushort command, ulong messageId)
    {
        span[0] = 0xFE;
        span[1] = (byte)'S';
        span[2] = (byte)'M';
        span[3] = (byte)'B';
        BinaryPrimitives.WriteUInt16LittleEndian(span[4..], HeaderSize); // StructureSize
        BinaryPrimitives.WriteUInt16LittleEndian(span[6..], 1); // CreditCharge
        BinaryPrimitives.WriteUInt16LittleEndian(span[12..], command);
        BinaryPrimitives.WriteUInt16LittleEndian(span[14..], 1); // CreditRequest
        BinaryPrimitives.WriteUInt64LittleEndian(span[24..], messageId);
    }

    /// <summary>
    ///     Creates an NTLM <c>NEGOTIATE</c> message (no domain / workstation, with version).
    /// </summary>
    private static byte[] CreateNTLMNegotiateMessage()
    {
        var message = new byte[40];
        var span = message.AsSpan();

        NTLMSSPSignature.CopyTo(span);
        BinaryPrimitives.WriteUInt32LittleEndian(span[8..], 1); // MessageType: NEGOTIATE
        BinaryPrimitives.WriteUInt32LittleEndian(span[12..],
            NTLMSSP_NEGOTIATE_UNICODE | NTLMSSP_REQUEST_TARGET | NTLMSSP_NEGOTIATE_NTLM |
            NTLMSSP_NEGOTIATE_ALWAYS_SIGN | NTLMSSP_NEGOTIATE_EXTENDED_SESSIONSECURITY |
            NTLMSSP_NEGOTIATE_TARGET_INFO | NTLMSSP_NEGOTIATE_VERSION | NTLMSSP_NEGOTIATE_128 |
            NTLMSSP_NEGOTIATE_56);

        // Domain / workstation fields (16 bytes) empty, version 10.0 with NTLM revision 15
        span[32] = 10;
        span[39] = 0x0F;

        return message;
    }

    /// <summary>
    ///     Wraps an NTLM message in an SPNEGO <c>NegTokenInit</c> (GSS-API initial context token).
    /// </summary>
    private static byte[] CreateSPNEGONegTokenInit(byte[] ntlmMessage)
    {
        // OID 1.3.6.1.4.1.311.2.2.10 (NTLMSSP)
        byte[] ntlmOID = [0x06, 0x0A, 0x2B, 0x06, 0x01, 0x04, 0x01, 0x82, 0x37, 0x02, 0x02, 0x0A];
        // OID 1.3.6.1.5.5.2 (SPNEGO)
        byte[] spnegoOID = [0x06, 0x06, 0x2B, 0x06, 0x01, 0x05, 0x05, 0x02];

        var mechTypes = DER(0xA0, DER(0x30, ntlmOID));
        var mechToken = DER(0xA2, DER(0x04, ntlmMessage));
        var negTokenInit = DER(0xA0, DER(0x30, [.. mechTypes, .. mechToken]));

        return DER(0x60, [.. spnegoOID, .. negTokenInit]);
    }

    /// <summary>
    ///     Encodes a DER element (tag, length, value).
    /// </summary>
    private static byte[] DER(byte tag, byte[] value)
    {
        byte[] length = value.Length switch
        {
            < 0x80 => [(byte)value.Length],
            <= 0xFF => [0x81, (byte)value.Length],
            _ => [0x82, (byte)(value.Length >> 8), (byte)value.Length]
        };

        return [tag, .. length, .. value];
    }

    #endregion

    #region Responses

    /// <summary>
    ///     Values of an SMB2 <c>NEGOTIATE</c> response.
    /// </summary>
    /// <param name="Dialect">Negotiated dialect, e.g. "3.1.1".</param>
    /// <param name="SigningRequired">The server requires signed messages.</param>
    /// <param name="ServerGuid">GUID of the server.</param>
    /// <param name="SystemTime">Time of the server in UTC, null if not set.</param>
    public sealed record NegotiateResponse(string Dialect, bool SigningRequired, Guid ServerGuid, DateTime? SystemTime);

    /// <summary>
    ///     Parses an SMB2 <c>NEGOTIATE</c> response.
    /// </summary>
    /// <param name="message">SMB2 message (without the 4 byte session header).</param>
    /// <returns>Values, null if it is not a successful negotiate response.</returns>
    public static NegotiateResponse ParseNegotiateResponse(ReadOnlySpan<byte> message)
    {
        if (!IsResponse(message, CommandNegotiate, StatusSuccess) || message.Length < HeaderSize + 64)
            return null;

        var body = message[HeaderSize..];

        if (BinaryPrimitives.ReadUInt16LittleEndian(body) != 65)
            return null;

        var securityMode = BinaryPrimitives.ReadUInt16LittleEndian(body[2..]);
        var dialect = BinaryPrimitives.ReadUInt16LittleEndian(body[4..]);

        return new NegotiateResponse(FormatDialect(dialect), (securityMode & 0x0002) != 0,
            new Guid(body.Slice(8, 16)), FromFileTime(BinaryPrimitives.ReadInt64LittleEndian(body[40..])));
    }

    /// <summary>
    ///     Parses an SMB2 <c>SESSION_SETUP</c> response that contains an NTLM <c>CHALLENGE</c> message.
    /// </summary>
    /// <param name="message">SMB2 message (without the 4 byte session header).</param>
    /// <returns>Values of the challenge, null if there is none (e.g. NTLM disabled).</returns>
    public static NTLMChallengeInfo ParseSessionSetupResponse(ReadOnlySpan<byte> message)
    {
        if (!IsResponse(message, CommandSessionSetup, StatusMoreProcessingRequired) ||
            message.Length < HeaderSize + 8)
            return null;

        var body = message[HeaderSize..];

        int offset = BinaryPrimitives.ReadUInt16LittleEndian(body[4..]);
        int length = BinaryPrimitives.ReadUInt16LittleEndian(body[6..]);

        if (offset < HeaderSize + 8 || offset + length > message.Length)
            return null;

        // The NTLM message is in the SPNEGO answer (NegTokenResp), found by its signature
        var securityBuffer = message.Slice(offset, length);
        var index = securityBuffer.IndexOf(NTLMSSPSignature);

        return index < 0 ? null : ParseNTLMChallenge(securityBuffer[index..]);
    }

    /// <summary>
    ///     Parses an NTLM <c>CHALLENGE</c> message: version (if sent) and the values of the target info.
    /// </summary>
    /// <param name="message">NTLM message, starting with the signature (may be followed by other data).</param>
    /// <returns>Values, null if it is not a challenge message.</returns>
    public static NTLMChallengeInfo ParseNTLMChallenge(ReadOnlySpan<byte> message)
    {
        if (message.Length < 48 || !message[..8].SequenceEqual(NTLMSSPSignature) ||
            BinaryPrimitives.ReadUInt32LittleEndian(message[8..]) != 2)
            return null;

        var flags = BinaryPrimitives.ReadUInt32LittleEndian(message[20..]);

        int targetInfoLength = BinaryPrimitives.ReadUInt16LittleEndian(message[40..]);
        var targetInfoOffset = BinaryPrimitives.ReadUInt32LittleEndian(message[44..]);

        // Version (8 bytes after the target info fields), only if negotiated and the payload starts after it
        Version version = null;

        if ((flags & NTLMSSP_NEGOTIATE_VERSION) != 0 && message.Length >= 56 &&
            (targetInfoLength == 0 || targetInfoOffset >= 56) && message[48] != 0)
            version = new Version(message[48], message[49], BinaryPrimitives.ReadUInt16LittleEndian(message[50..]));

        string nbComputer = string.Empty, nbDomain = string.Empty, dnsComputer = string.Empty,
            dnsDomain = string.Empty, dnsTree = string.Empty;
        DateTime? timestamp = null;

        if (targetInfoLength > 0 && targetInfoOffset + (ulong)targetInfoLength <= (ulong)message.Length)
        {
            var targetInfo = message.Slice((int)targetInfoOffset, targetInfoLength);

            // AV_PAIR list: id, length, value; ends with MsvAvEOL
            while (targetInfo.Length >= 4)
            {
                var id = BinaryPrimitives.ReadUInt16LittleEndian(targetInfo);
                int length = BinaryPrimitives.ReadUInt16LittleEndian(targetInfo[2..]);

                if (id == 0 || targetInfo.Length < 4 + length)
                    break;

                var value = targetInfo.Slice(4, length);

                switch (id)
                {
                    case 1:
                        nbComputer = ToString(value);
                        break;
                    case 2:
                        nbDomain = ToString(value);
                        break;
                    case 3:
                        dnsComputer = ToString(value);
                        break;
                    case 4:
                        dnsDomain = ToString(value);
                        break;
                    case 5:
                        dnsTree = ToString(value);
                        break;
                    case 7 when length == 8:
                        timestamp = FromFileTime(BinaryPrimitives.ReadInt64LittleEndian(value));
                        break;
                }

                targetInfo = targetInfo[(4 + length)..];
            }
        }

        return new NTLMChallengeInfo(version, nbComputer, nbDomain, dnsComputer, dnsDomain, dnsTree, timestamp);
    }

    /// <summary>
    ///     Formats an SMB2 dialect, e.g. 0x0311 -> "3.1.1", 0x0210 -> "2.1".
    /// </summary>
    public static string FormatDialect(ushort dialect)
    {
        var major = (dialect >> 8) & 0xF;
        var minor = (dialect >> 4) & 0xF;
        var patch = dialect & 0xF;

        return patch == 0 ? $"{major}.{minor}" : $"{major}.{minor}.{patch}";
    }

    /// <summary>
    ///     The message is an SMB2 response of the command with the status.
    /// </summary>
    private static bool IsResponse(ReadOnlySpan<byte> message, ushort command, uint status)
    {
        return message.Length >= HeaderSize &&
               message[0] == 0xFE && message[1] == 'S' && message[2] == 'M' && message[3] == 'B' &&
               BinaryPrimitives.ReadUInt32LittleEndian(message[8..]) == status &&
               BinaryPrimitives.ReadUInt16LittleEndian(message[12..]) == command &&
               (BinaryPrimitives.ReadUInt32LittleEndian(message[16..]) & 0x00000001) != 0; // SMB2_FLAGS_SERVER_TO_REDIR
    }

    /// <summary>
    ///     Converts a FILETIME to UTC, null if it is not set or invalid.
    /// </summary>
    private static DateTime? FromFileTime(long fileTime)
    {
        if (fileTime <= 0)
            return null;

        try
        {
            return DateTime.FromFileTimeUtc(fileTime);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    /// <summary>
    ///     Decodes a UTF-16 value of the target info without control characters.
    /// </summary>
    private static string ToString(ReadOnlySpan<byte> value)
    {
        var text = Encoding.Unicode.GetString(value);

        var builder = new StringBuilder(text.Length);

        foreach (var c in text)
            if (!char.IsControl(c))
                builder.Append(c);

        return builder.ToString();
    }

    #endregion

    #region Transport

    /// <summary>
    ///     Writes an SMB2 message with the 4 byte session header (direct TCP transport: zero byte + 24 bit length).
    /// </summary>
    private static async Task WriteMessageAsync(Stream stream, byte[] message, CancellationToken cancellationToken)
    {
        var packet = new byte[4 + message.Length];
        packet[1] = (byte)(message.Length >> 16);
        packet[2] = (byte)(message.Length >> 8);
        packet[3] = (byte)message.Length;
        message.CopyTo(packet, 4);

        await stream.WriteAsync(packet, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Reads an SMB2 message (without the 4 byte session header).
    /// </summary>
    private static async Task<byte[]> ReadMessageAsync(Stream stream, CancellationToken cancellationToken)
    {
        var header = new byte[4];
        await stream.ReadExactlyAsync(header, cancellationToken).ConfigureAwait(false);

        var length = (header[1] << 16) | (header[2] << 8) | header[3];

        if (header[0] != 0 || length is < HeaderSize or > MaxMessageSize)
            throw new InvalidDataException("Invalid SMB session header.");

        var message = new byte[length];
        await stream.ReadExactlyAsync(message, cancellationToken).ConfigureAwait(false);

        return message;
    }

    #endregion
}
