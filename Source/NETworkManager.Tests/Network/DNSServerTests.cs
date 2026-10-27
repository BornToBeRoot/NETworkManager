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
