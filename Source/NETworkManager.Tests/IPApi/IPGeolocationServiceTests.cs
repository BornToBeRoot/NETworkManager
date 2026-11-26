using System;
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
}
