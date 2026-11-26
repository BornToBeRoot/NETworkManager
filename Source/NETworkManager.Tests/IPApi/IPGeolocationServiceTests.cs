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
