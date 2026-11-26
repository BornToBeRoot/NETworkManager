using NETworkManager.Models.Cloudflare;
using Xunit;

namespace NETworkManager.Tests.Cloudflare;

public class SpeedTestCalculatorTests
{
    [Fact]
    public void PercentileOfNoValuesIsZero()
    {
        Assert.Equal(0, SpeedTestCalculator.Percentile([], 0.9));
    }

    [Fact]
    public void PercentileOfOneValueIsTheValue()
    {
        Assert.Equal(42, SpeedTestCalculator.Percentile([42], 0.9));
    }

    [Fact]
    public void PercentileOnARankIsTheValue()
    {
        // Rank 0.5 * (5 - 1) = 2 → third value of the sorted list
        Assert.Equal(3, SpeedTestCalculator.Percentile([5, 1, 4, 2, 3], 0.5));
    }

    [Fact]
    public void PercentileBetweenRanksIsInterpolated()
    {
        // Rank 0.9 * (10 - 1) = 8.1 → 9 + 0.1 * (10 - 9)
        Assert.Equal(9.1, SpeedTestCalculator.Percentile([10, 9, 8, 7, 6, 5, 4, 3, 2, 1], 0.9), 10);
    }

    [Fact]
    public void JitterOfFewerThanTwoPingsIsZero()
    {
        Assert.Equal(0, SpeedTestCalculator.Jitter([]));
        Assert.Equal(0, SpeedTestCalculator.Jitter([10]));
    }

    [Fact]
    public void JitterIsTheAverageConsecutiveDifference()
    {
        // |12 - 10| + |9 - 12| + |9 - 9| = 5, 3 differences
        Assert.Equal(5.0 / 3, SpeedTestCalculator.Jitter([10, 12, 9, 9]), 10);
    }

    [Fact]
    public void ServerTimingWithoutHeaderIsNull()
    {
        Assert.Null(SpeedTestCalculator.ParseServerTiming(null));
        Assert.Null(SpeedTestCalculator.ParseServerTiming([]));
    }

    [Fact]
    public void ServerTimingIsTheRequestDuration()
    {
        Assert.Equal(12.5, SpeedTestCalculator.ParseServerTiming(["cfRequestDuration;dur=12.5"]));
    }

    [Fact]
    public void ServerTimingWithInvalidDurationIsNull()
    {
        Assert.Null(SpeedTestCalculator.ParseServerTiming(["cfRequestDuration;dur=."]));
    }
}
