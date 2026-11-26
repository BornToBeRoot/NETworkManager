using NETworkManager.Models.IPApi;
using Xunit;

namespace NETworkManager.Tests.IPApi;

public class DNSResolverServiceTests
{
    [Fact]
    public void ResponseIsParsed()
    {
        var result = DNSResolverService.ParseResponse(
            "{\"dns\":{\"ip\":\"198.51.100.53\",\"geo\":\"Germany - Example ISP\"},\"edns\":{\"ip\":\"203.0.113.0\",\"geo\":\"Germany - Example ISP\"}}");

        Assert.False(result.HasError);
        Assert.Equal("198.51.100.53", result.Info.DnsIp);
        Assert.Equal("Germany - Example ISP", result.Info.DnsGeo);
        Assert.Equal("203.0.113.0", result.Info.EdnsIp);
    }

    [Fact]
    public void ResponseWithoutEDNSIsParsed()
    {
        // The resolver doesn't send the client subnet
        var result = DNSResolverService.ParseResponse("{\"dns\":{\"ip\":\"198.51.100.53\",\"geo\":\"Germany\"}}");

        Assert.False(result.HasError);
        Assert.Equal("198.51.100.53", result.Info.DnsIp);
        Assert.Null(result.Info.EdnsIp);
        Assert.Null(result.Info.EdnsGeo);
    }

    [Theory]
    [InlineData("")]
    [InlineData("null")]
    public void EmptyResponseIsAnError(string json)
    {
        var result = DNSResolverService.ParseResponse(json);

        Assert.True(result.HasError);
        Assert.Null(result.Info);
    }
}
