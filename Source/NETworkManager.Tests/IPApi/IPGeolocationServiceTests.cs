using System;
using System.Net.Http;
using NETworkManager.Models.IPApi;
using Xunit;

namespace NETworkManager.Tests.IPApi;

public class IPGeolocationServiceTests
{
    private static readonly DateTime LastReached = new(2026, 11, 26, 12, 0, 0);

    [Fact]
    public void RemainingSecondsRightAfterTheRateLimitIsTheWindowWithMargin()
    {
        Assert.Equal(61, IPGeolocationService.GetRemainingSeconds(LastReached, 60, LastReached));
    }

    [Fact]
    public void RemainingSecondsCountDown()
    {
        // Not the whole window again ("try again in 60 seconds" 50 seconds later)
        Assert.Equal(11, IPGeolocationService.GetRemainingSeconds(LastReached, 60, LastReached.AddSeconds(50)));
    }

    [Fact]
    public void RemainingSecondsAreRoundedUp()
    {
        Assert.Equal(2, IPGeolocationService.GetRemainingSeconds(LastReached, 10, LastReached.AddSeconds(9.5)));
    }

    [Fact]
    public void RemainingSecondsAreAtLeastOne()
    {
        Assert.Equal(1, IPGeolocationService.GetRemainingSeconds(LastReached, 10, LastReached.AddSeconds(11)));
        Assert.Equal(1, IPGeolocationService.GetRemainingSeconds(LastReached, 10, LastReached.AddMinutes(5)));
    }

    [Theory]
    [InlineData("", "http://ip-api.com/json/?fields=")]
    [InlineData("8.8.8.8", "http://ip-api.com/json/8.8.8.8?fields=")]
    [InlineData("2001:db8::1", "http://ip-api.com/json/2001:db8::1?fields=")]
    [InlineData("example.com", "http://ip-api.com/json/example.com?fields=")]
    public void UrlHasOneSlashBeforeTheQuery(string query, string expectedStart)
    {
        Assert.StartsWith(expectedStart, IPGeolocationService.GetUrl(query));
    }

    [Fact]
    public void ResponseIsParsed()
    {
        var result = IPGeolocationService.ParseResponse(
            "{\"status\":\"success\",\"country\":\"Germany\",\"countryCode\":\"DE\",\"offset\":3600,\"query\":\"203.0.113.10\"}");

        Assert.False(result.HasError);
        Assert.Equal("Germany", result.Info.Country);
        Assert.Equal(3600, result.Info.Offset);
        Assert.Equal("203.0.113.10", result.Info.Query);
    }

    [Fact]
    public void FailedQueryHasTheReason()
    {
        // HTTP 200, no error of the request: the IP Geolocation tool shows the reason, the traceroute an empty hop
        var result = IPGeolocationService.ParseResponse(
            "{\"status\":\"fail\",\"message\":\"private range\",\"query\":\"192.168.1.1\"}");

        Assert.False(result.HasError);
        Assert.True(result.Info.HasFailed);
        Assert.Equal("private range", result.Info.Message);
    }

    [Fact]
    public void SuccessfulQueryHasNotFailed()
    {
        Assert.False(IPGeolocationService.ParseResponse("{\"status\":\"success\"}").Info.HasFailed);
    }

    [Theory]
    [InlineData("")]
    [InlineData("null")]
    public void EmptyResponseIsAnError(string json)
    {
        Assert.True(IPGeolocationService.ParseResponse(json).HasError);
    }

    private static HttpResponseMessage CreateResponse(string xRl, string xTtl)
    {
        var response = new HttpResponseMessage();

        if (xRl != null)
            response.Headers.Add("X-Rl", xRl);

        if (xTtl != null)
            response.Headers.Add("X-Ttl", xTtl);

        return response;
    }

    [Fact]
    public void RateLimitIsReadFromTheHeaders()
    {
        Assert.True(IPGeolocationService.TryParseRateLimit(CreateResponse("44", "59").Headers,
            out var remainingRequests, out var remainingTime));
        Assert.Equal(44, remainingRequests);
        Assert.Equal(59, remainingTime);
    }

    [Theory]
    [InlineData(null, "59")]
    [InlineData("44", null)]
    [InlineData(null, null)]
    [InlineData("many", "59")]
    [InlineData("44", "")]
    public void RateLimitWithoutValidHeadersIsNotRead(string xRl, string xTtl)
    {
        // e.g. removed by a proxy: the answer is used anyway
        Assert.False(IPGeolocationService.TryParseRateLimit(CreateResponse(xRl, xTtl).Headers, out _, out _));
    }
}
