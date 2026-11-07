using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NETworkManager.Models.Network;
using Xunit;

namespace NETworkManager.Tests.Network;

public class SMBInfoResolverTests
{
    // CHALLENGE_MESSAGE example of MS-NLMP 4.2.4.3: version 6.0.6000, target info NetBIOS domain "Domain" and NetBIOS
    // computer "Server"
    private const string SpecChallenge =
        "4e544c4d53535000020000000c000c003800000033828ae20123456789abcdef00000000000000002400240044000000" +
        "060070170000000f53006500720076006500720002000c0044006f006d00610069006e0001000c00530065007200760065" +
        "00720000000000";

    private static readonly Guid ServerGuid = Guid.Parse("8c5b6f4e-3d2a-4b1c-9e8f-7a6b5c4d3e2f");

    private static readonly DateTime ServerTime = new(2026, 11, 1, 12, 30, 45, DateTimeKind.Utc);

    private static byte[] CreateHeader(ushort command, uint status, bool response = true)
    {
        var header = new byte[64];
        header[0] = 0xFE;
        header[1] = (byte)'S';
        header[2] = (byte)'M';
        header[3] = (byte)'B';
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(4), 64);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(8), status);
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(12), command);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(16), response ? 1u : 0u);

        return header;
    }

    private static byte[] CreateNegotiateResponse(ushort dialect = 0x0311, ushort securityMode = 0x0003,
        uint status = 0, bool response = true, long? systemTime = null)
    {
        var body = new byte[65 + 7];
        var span = body.AsSpan();
        BinaryPrimitives.WriteUInt16LittleEndian(span, 65);
        BinaryPrimitives.WriteUInt16LittleEndian(span[2..], securityMode);
        BinaryPrimitives.WriteUInt16LittleEndian(span[4..], dialect);
        ServerGuid.TryWriteBytes(span[8..]);
        BinaryPrimitives.WriteInt64LittleEndian(span[40..], systemTime ?? ServerTime.ToFileTimeUtc());

        return [.. CreateHeader(0x0000, status, response), .. body];
    }

    private static byte[] AVPair(ushort id, byte[] value)
    {
        var pair = new byte[4 + value.Length];
        BinaryPrimitives.WriteUInt16LittleEndian(pair, id);
        BinaryPrimitives.WriteUInt16LittleEndian(pair.AsSpan(2), (ushort)value.Length);
        value.CopyTo(pair, 4);

        return pair;
    }

    // Challenge like Windows 11 25H2 in the domain CORP / corp.example.com
    private static byte[] CreateChallenge(bool withVersion = true)
    {
        var targetInfo = new List<byte>();
        targetInfo.AddRange(AVPair(2, Encoding.Unicode.GetBytes("CORP")));
        targetInfo.AddRange(AVPair(1, Encoding.Unicode.GetBytes("CLIENT01")));
        targetInfo.AddRange(AVPair(4, Encoding.Unicode.GetBytes("corp.example.com")));
        targetInfo.AddRange(AVPair(3, Encoding.Unicode.GetBytes("client01.corp.example.com")));
        targetInfo.AddRange(AVPair(5, Encoding.Unicode.GetBytes("example.com")));
        targetInfo.AddRange(AVPair(7, BitConverter.GetBytes(ServerTime.ToFileTimeUtc())));
        targetInfo.AddRange(AVPair(0, []));

        var targetName = Encoding.Unicode.GetBytes("CORP");

        var message = new byte[56 + targetName.Length + targetInfo.Count];
        var span = message.AsSpan();
        "NTLMSSP\0"u8.CopyTo(span);
        BinaryPrimitives.WriteUInt32LittleEndian(span[8..], 2);
        BinaryPrimitives.WriteUInt16LittleEndian(span[12..], (ushort)targetName.Length);
        BinaryPrimitives.WriteUInt16LittleEndian(span[14..], (ushort)targetName.Length);
        BinaryPrimitives.WriteUInt32LittleEndian(span[16..], 56);
        BinaryPrimitives.WriteUInt32LittleEndian(span[20..], withVersion ? 0xE28A8235u : 0xE08A8235u);
        BinaryPrimitives.WriteUInt16LittleEndian(span[40..], (ushort)targetInfo.Count);
        BinaryPrimitives.WriteUInt16LittleEndian(span[42..], (ushort)targetInfo.Count);
        BinaryPrimitives.WriteUInt32LittleEndian(span[44..], (uint)(56 + targetName.Length));

        if (withVersion)
        {
            span[48] = 10;
            span[49] = 0;
            BinaryPrimitives.WriteUInt16LittleEndian(span[50..], 26200);
            span[55] = 0x0F;
        }

        targetName.CopyTo(span[56..]);
        targetInfo.ToArray().CopyTo(span[(56 + targetName.Length)..]);

        return message;
    }

    private static byte[] CreateSessionSetupResponse(byte[] securityBuffer, uint status = 0xC0000016)
    {
        var body = new byte[8];
        BinaryPrimitives.WriteUInt16LittleEndian(body, 9);
        BinaryPrimitives.WriteUInt16LittleEndian(body.AsSpan(4), 72);
        BinaryPrimitives.WriteUInt16LittleEndian(body.AsSpan(6), (ushort)securityBuffer.Length);

        return [.. CreateHeader(0x0001, status), .. body, .. securityBuffer];
    }

    [Fact]
    public void CreateNegotiateRequest_DialectsAndPreauthContext()
    {
        var salt = Enumerable.Range(0, 32).Select(x => (byte)x).ToArray();

        var request = SMBInfoResolver.CreateNegotiateRequest(Guid.NewGuid(), salt);

        Assert.Equal([0xFE, (byte)'S', (byte)'M', (byte)'B'], request[..4]);
        Assert.Equal(0, BinaryPrimitives.ReadUInt16LittleEndian(request.AsSpan(12))); // NEGOTIATE
        Assert.Equal(36, BinaryPrimitives.ReadUInt16LittleEndian(request.AsSpan(64)));
        Assert.Equal(5, BinaryPrimitives.ReadUInt16LittleEndian(request.AsSpan(66)));

        ushort[] dialects = [.. Enumerable.Range(0, 5).Select(i => BinaryPrimitives.ReadUInt16LittleEndian(request.AsSpan(100 + i * 2)))];
        Assert.Equal([0x0202, 0x0210, 0x0300, 0x0302, 0x0311], dialects);

        // Negotiate context 8 byte aligned after the dialects: preauth integrity (SHA-512) with the salt
        var contextOffset = (int)BinaryPrimitives.ReadUInt32LittleEndian(request.AsSpan(92));
        Assert.Equal(112, contextOffset);
        Assert.Equal(1, BinaryPrimitives.ReadUInt16LittleEndian(request.AsSpan(96)));
        Assert.Equal(1, BinaryPrimitives.ReadUInt16LittleEndian(request.AsSpan(contextOffset)));
        Assert.Equal(38, BinaryPrimitives.ReadUInt16LittleEndian(request.AsSpan(contextOffset + 2)));
        Assert.Equal(1, BinaryPrimitives.ReadUInt16LittleEndian(request.AsSpan(contextOffset + 12)));
        Assert.Equal(salt, request[(contextOffset + 14)..]);
    }

    [Fact]
    public void CreateSessionSetupRequest_NTLMNegotiateInSPNEGO()
    {
        var request = SMBInfoResolver.CreateSessionSetupRequest();

        Assert.Equal(1, BinaryPrimitives.ReadUInt16LittleEndian(request.AsSpan(12))); // SESSION_SETUP
        Assert.Equal(1UL, BinaryPrimitives.ReadUInt64LittleEndian(request.AsSpan(24))); // MessageId

        var offset = BinaryPrimitives.ReadUInt16LittleEndian(request.AsSpan(76));
        var length = BinaryPrimitives.ReadUInt16LittleEndian(request.AsSpan(78));
        Assert.Equal(88, offset);
        Assert.Equal(request.Length, offset + length);

        // GSS-API token with the SPNEGO OID, NTLM NEGOTIATE (type 1) with the version flag
        var buffer = request.AsSpan(offset, length);
        Assert.Equal(0x60, buffer[0]);
        Assert.Equal(length - 2, buffer[1]);

        var index = buffer.IndexOf("NTLMSSP\0"u8);
        Assert.True(index > 0);
        Assert.Equal(40, length - index);
        Assert.Equal(1u, BinaryPrimitives.ReadUInt32LittleEndian(buffer[(index + 8)..]));
        Assert.NotEqual(0u, BinaryPrimitives.ReadUInt32LittleEndian(buffer[(index + 12)..]) & 0x02000000);
    }

    [Fact]
    public void ParseNegotiateResponse()
    {
        var response = SMBInfoResolver.ParseNegotiateResponse(CreateNegotiateResponse());

        Assert.Equal("3.1.1", response.Dialect);
        Assert.True(response.SigningRequired);
        Assert.Equal(ServerGuid, response.ServerGuid);
        Assert.Equal(ServerTime, response.SystemTime);
    }

    [Fact]
    public void ParseNegotiateResponse_SigningNotRequired_NoTime()
    {
        var response =
            SMBInfoResolver.ParseNegotiateResponse(CreateNegotiateResponse(0x0210, 0x0001, systemTime: 0));

        Assert.Equal("2.1", response.Dialect);
        Assert.False(response.SigningRequired);
        Assert.Null(response.SystemTime);
    }

    [Fact]
    public void ParseNegotiateResponse_Invalid()
    {
        // Error status, request instead of response, wrong command, truncated, garbage
        Assert.Null(SMBInfoResolver.ParseNegotiateResponse(CreateNegotiateResponse(status: 0xC0000022)));
        Assert.Null(SMBInfoResolver.ParseNegotiateResponse(CreateNegotiateResponse(response: false)));
        Assert.Null(SMBInfoResolver.ParseNegotiateResponse(CreateSessionSetupResponse([], 0)));
        Assert.Null(SMBInfoResolver.ParseNegotiateResponse(CreateNegotiateResponse()[..100]));
        Assert.Null(SMBInfoResolver.ParseNegotiateResponse(new byte[200]));
    }

    [Fact]
    public void ParseNTLMChallenge_SpecExample()
    {
        var challenge = SMBInfoResolver.ParseNTLMChallenge(Convert.FromHexString(SpecChallenge));

        Assert.Equal(new Version(6, 0, 6000), challenge.Version);
        Assert.Equal("Server", challenge.NetBIOSComputerName);
        Assert.Equal("Domain", challenge.NetBIOSDomainName);
        Assert.Equal(string.Empty, challenge.DNSComputerName);
        Assert.Null(challenge.Timestamp);
        Assert.True(challenge.IsDomainMember);
    }

    [Fact]
    public void ParseSessionSetupResponse_ChallengeInSPNEGO()
    {
        // NegTokenResp prefix (accept-incomplete, NTLMSSP OID, responseToken) in front of the NTLM message
        byte[] spnegoPrefix =
        [
            0xA1, 0x81, 0xC0, 0x30, 0x81, 0xBD, 0xA0, 0x03, 0x0A, 0x01, 0x01, 0xA1, 0x0C, 0x06, 0x0A, 0x2B, 0x06, 0x01,
            0x04, 0x01, 0x82, 0x37, 0x02, 0x02, 0x0A, 0xA2, 0x81, 0xA7, 0x04, 0x81, 0xA4
        ];

        var challenge = SMBInfoResolver.ParseSessionSetupResponse(
            CreateSessionSetupResponse([.. spnegoPrefix, .. CreateChallenge()]));

        Assert.Equal(new Version(10, 0, 26200), challenge.Version);
        Assert.Equal("CLIENT01", challenge.NetBIOSComputerName);
        Assert.Equal("CORP", challenge.NetBIOSDomainName);
        Assert.Equal("client01.corp.example.com", challenge.DNSComputerName);
        Assert.Equal("corp.example.com", challenge.DNSDomainName);
        Assert.Equal("example.com", challenge.DNSTreeName);
        Assert.Equal(ServerTime, challenge.Timestamp);
        Assert.True(challenge.IsDomainMember);
    }

    [Fact]
    public void ParseNTLMChallenge_WithoutVersion()
    {
        Assert.Null(SMBInfoResolver.ParseNTLMChallenge(CreateChallenge(false)).Version);
    }

    [Fact]
    public void ParseNTLMChallenge_Workgroup()
    {
        // A computer in a workgroup sends its own name as domain
        var challenge = new NTLMChallengeInfo(null, "PC01", "PC01", "PC01", "PC01", string.Empty, null);

        Assert.False(challenge.IsDomainMember);
    }

    [Fact]
    public void ParseSessionSetupResponse_NoChallenge()
    {
        // Logon failure / success status, no NTLM message, truncated buffer, invalid offset
        Assert.Null(SMBInfoResolver.ParseSessionSetupResponse(CreateSessionSetupResponse(CreateChallenge(), 0xC000006D)));
        Assert.Null(SMBInfoResolver.ParseSessionSetupResponse(CreateSessionSetupResponse([0xA1, 0x03, 0x30, 0x01, 0x00])));
        Assert.Null(SMBInfoResolver.ParseSessionSetupResponse(CreateSessionSetupResponse(CreateChallenge()[..30])));

        var invalidOffset = CreateSessionSetupResponse(CreateChallenge());
        BinaryPrimitives.WriteUInt16LittleEndian(invalidOffset.AsSpan(68), 0xFFF0);
        Assert.Null(SMBInfoResolver.ParseSessionSetupResponse(invalidOffset));
    }

    [Fact]
    public void ParseNTLMChallenge_TruncatedTargetInfo()
    {
        // Target info longer than the message: values empty, no exception
        var challenge = CreateChallenge();
        BinaryPrimitives.WriteUInt16LittleEndian(challenge.AsSpan(40), 0x7FFF);

        var info = SMBInfoResolver.ParseNTLMChallenge(challenge);

        Assert.Equal(new Version(10, 0, 26200), info.Version);
        Assert.Equal(string.Empty, info.NetBIOSComputerName);
    }

    [Theory]
    [InlineData(0x0202, "2.0.2")]
    [InlineData(0x0210, "2.1")]
    [InlineData(0x0300, "3.0")]
    [InlineData(0x0302, "3.0.2")]
    [InlineData(0x0311, "3.1.1")]
    public void FormatDialect(ushort dialect, string expected)
    {
        Assert.Equal(expected, SMBInfoResolver.FormatDialect(dialect));
    }
}
