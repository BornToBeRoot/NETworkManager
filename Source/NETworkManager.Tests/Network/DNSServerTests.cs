using System.Collections.Generic;
using System.Linq;
using NETworkManager.Models.Network;
using Xunit;

namespace NETworkManager.Tests.Network;

public class DNSServerTests
{
    private static readonly List<DNSServerConnectionInfoProfile> Profiles = DNSServer.GetDefaultList();

    [Theory]
    [InlineData("Cloudflare", "1.1.1.1:53; 1.0.0.1:53")]
    [InlineData("cloudflare", "1.1.1.1:53; 1.0.0.1:53")]
    [InlineData("Google Public DNS;Cloudflare", "8.8.8.8:53; 8.8.4.4:53; 1.1.1.1:53; 1.0.0.1:53")]
    [InlineData(" Quad9 ; 1.1.1.1", "9.9.9.9:53; 149.112.112.112:53; 1.1.1.1:53")]
    [InlineData("1.1.1.1:5353; dns.example.com", "1.1.1.1:5353; dns.example.com:53")]
    [InlineData("Cloudflare; 1.1.1.1", "1.1.1.1:53; 1.0.0.1:53")]
    public void ResolveServers(string input, string expected)
    {
        var servers = DNSServer.ResolveServers(input, Profiles, out var useWindowsDNSServer, out var invalidEntries);

        Assert.False(useWindowsDNSServer);
        Assert.Empty(invalidEntries);
        Assert.Equal(expected, string.Join("; ", servers.Select(x => $"{x.Server}:{x.Port}")));
    }

    [Fact]
    public void ResolveServers_WindowsDNSServerAlone()
    {
        var servers = DNSServer.ResolveServers("[windows dns]", Profiles, out var useWindowsDNSServer, out _);

        Assert.True(useWindowsDNSServer);
        Assert.Empty(servers);
    }

    [Fact]
    public void ResolveServers_InvalidEntriesAreSkipped()
    {
        var servers = DNSServer.ResolveServers("Cloudflare; not valid!", Profiles, out _, out var invalidEntries);

        Assert.Equal(2, servers.Count);
        Assert.Equal(["not valid!"], invalidEntries);
    }

    [Fact]
    public void EncodeAndDecodeReferences()
    {
        var cloudflare = Profiles.First(x => x.Name == "Cloudflare");

        var input = DNSServer.EncodeReferences("[Windows DNS]; cloudflare; 9.9.9.9", Profiles);

        Assert.Equal($"[Windows DNS]; {cloudflare.Id:B}; 9.9.9.9", input);
        Assert.True(DNSServer.TryDecodeReferences(input, Profiles, out var decoded));
        Assert.Equal("[Windows DNS]; Cloudflare; 9.9.9.9", decoded);
    }

    [Theory]
    [InlineData("9.9.9.9")]
    [InlineData("[Windows DNS]")]
    public void EncodeReferences_NullWithoutProfileReference(string input)
    {
        Assert.Null(DNSServer.EncodeReferences(input, Profiles));
    }

    [Fact]
    public void DecodeReferences_FalseIfProfileIsMissing()
    {
        Assert.False(DNSServer.TryDecodeReferences($"{System.Guid.NewGuid():B}", Profiles, out var decoded));
        Assert.Null(decoded);
    }

    [Fact]
    public void EncodeAndDecode()
    {
        var cloudflare = Profiles.First(x => x.Name == "Cloudflare");

        var (dnsServer, dnsServerInput) = DNSServer.Encode("Cloudflare; 9.9.9.9", Profiles);

        Assert.Equal("1.1.1.1:53; 1.0.0.1:53; 9.9.9.9", dnsServer);
        Assert.Equal($"{cloudflare.Id:B}; 9.9.9.9", dnsServerInput);
        Assert.Equal("Cloudflare; 9.9.9.9", DNSServer.Decode(dnsServer, dnsServerInput, Profiles, out var notFound));
        Assert.False(notFound);
    }

    [Fact]
    public void Decode_SavedServersIfProfileIsMissing()
    {
        Assert.Equal("1.1.1.1:53", DNSServer.Decode("1.1.1.1:53", $"{System.Guid.NewGuid():B}", Profiles,
            out var notFound));
        Assert.True(notFound);
    }

    [Fact]
    public void ReplaceProfilesByServers()
    {
        Assert.Equal("[Windows DNS]; 1.1.1.1:53; 1.0.0.1:53; 9.9.9.9",
            DNSServer.ReplaceProfilesByServers("[Windows DNS]; Cloudflare; 9.9.9.9", Profiles));
    }

    [Theory]
    [InlineData("Cloudflare", false, 0)]
    [InlineData("google public dns;9.9.9.9", true, 0)]
    [InlineData("dns01", false, 0)]
    [InlineData("dns01", true, 1)]
    [InlineData("Cloudflare;aaa", true, 1)]
    [InlineData("", false, 1)]
    public void GetInvalidEntries(string input, bool hostnameNeedsDomain, int expectedCount)
    {
        Assert.Equal(expectedCount, DNSServer.GetInvalidEntries(input, Profiles, hostnameNeedsDomain).Count);
    }
}
