using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using NETworkManager.Models.Network;
using Xunit;

namespace NETworkManager.Tests.Network;

public class MDNSResolverTests
{
    private const ushort Id = 0x1234;

    private static byte[] EncodeName(string name)
    {
        var bytes = new List<byte>();

        foreach (var label in name.Split('.'))
        {
            bytes.Add((byte)label.Length);
            bytes.AddRange(Encoding.ASCII.GetBytes(label));
        }

        bytes.Add(0);

        return [.. bytes];
    }

    // Legacy unicast response: question repeated, answer name as compression pointer to the question (offset 12)
    private static byte[] CreateResponse(ushort id, string hostname, ushort type = 12, bool compressed = true)
    {
        var question = CreateQuestion();

        var response = new List<byte>
        {
            (byte)(id >> 8), (byte)id, 0x84, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00
        };

        response.AddRange(question);

        // Answer: name (pointer to the question name), type, class (cache flush), TTL, data length, data
        response.AddRange(compressed ? [0xC0, 0x0C] : question[..^4]);
        response.AddRange([(byte)(type >> 8), (byte)type, 0x80, 0x01, 0x00, 0x00, 0x00, 0x78]);

        var data = EncodeName(hostname);

        response.AddRange([(byte)(data.Length >> 8), (byte)data.Length]);
        response.AddRange(data);

        return [.. response];
    }

    private static byte[] CreateQuestion()
    {
        return [.. EncodeName("10.1.168.192.in-addr.arpa"), 0x00, 0x0C, 0x00, 0x01];
    }

    [Theory]
    [InlineData("192.168.1.10", "10.1.168.192.in-addr.arpa")]
    [InlineData("2001:db8::1",
        "1.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.8.b.d.0.1.0.0.2.ip6.arpa")]
    public void GetReverseName(string ipAddress, string expected)
    {
        Assert.Equal(expected, MDNSResolver.GetReverseName(IPAddress.Parse(ipAddress)));
    }

    [Fact]
    public void CreateReverseQuery()
    {
        var query = MDNSResolver.CreateReverseQuery(IPAddress.Parse("192.168.1.10"), Id);

        Assert.Equal(new byte[] { 0x12, 0x34, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
            query[..12]);
        Assert.Equal(CreateQuestion(), query[12..]);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ParsePTRResponse(bool compressed)
    {
        Assert.Equal("macbook.local",
            MDNSResolver.ParsePTRResponse(CreateResponse(Id, "macbook.local", compressed: compressed), Id));
    }

    [Fact]
    public void ParsePTRResponse_IdZero_Accepted()
    {
        Assert.Equal("printer.local", MDNSResolver.ParsePTRResponse(CreateResponse(0, "printer.local"), Id));
    }

    [Fact]
    public void ParsePTRResponse_OtherId_Empty()
    {
        Assert.Equal(string.Empty, MDNSResolver.ParsePTRResponse(CreateResponse(0x9999, "macbook.local"), Id));
    }

    [Fact]
    public void ParsePTRResponse_NoPTR_Empty()
    {
        Assert.Equal(string.Empty, MDNSResolver.ParsePTRResponse(CreateResponse(Id, "macbook.local", 1), Id));
    }

    [Fact]
    public void ParsePTRResponse_Query_Empty()
    {
        var query = MDNSResolver.CreateReverseQuery(IPAddress.Parse("192.168.1.10"), Id);

        Assert.Equal(string.Empty, MDNSResolver.ParsePTRResponse(query, Id));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(20)]
    [InlineData(60)]
    public void ParsePTRResponse_Truncated_Empty(int length)
    {
        var response = CreateResponse(Id, "macbook.local");

        Assert.Equal(string.Empty, MDNSResolver.ParsePTRResponse(response[..length], Id));
    }

    [Fact]
    public void ParsePTRResponse_PointerLoop_Empty()
    {
        // Answer name points to itself
        var response = CreateResponse(Id, "macbook.local");
        var answerOffset = 12 + CreateQuestion().Length;

        response[answerOffset] = 0xC0;
        response[answerOffset + 1] = (byte)answerOffset;

        Assert.Equal(string.Empty, MDNSResolver.ParsePTRResponse(response, Id));
    }

    [Fact]
    public void ParsePTRResponse_TrailingGarbage_Ignored()
    {
        var response = CreateResponse(Id, "macbook.local").Concat(new byte[] { 0xFF, 0xFF }).ToArray();

        Assert.Equal("macbook.local", MDNSResolver.ParsePTRResponse(response, Id));
    }
}
