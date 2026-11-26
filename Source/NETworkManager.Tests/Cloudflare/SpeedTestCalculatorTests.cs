using System;
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
    public void CurrentBandwidthWithoutValuesIsNull()
    {
        Assert.Null(SpeedTestCalculator.CurrentBandwidth([], null));
    }

    [Fact]
    public void CurrentBandwidthBeforeTheFirstSampleIsTheLiveValue()
    {
        Assert.Equal(50, SpeedTestCalculator.CurrentBandwidth([], 50));
    }

    [Fact]
    public void CurrentBandwidthWithSamplesIgnoresAHigherLiveValue()
    {
        // A burst of the running request must not be shown above the result (P90 of the samples)
        Assert.Equal(100, SpeedTestCalculator.CurrentBandwidth([100], 480));
    }

    [Fact]
    public void DirectionIsNotFinishedWithoutRequests()
    {
        Assert.False(SpeedTestCalculator.IsDirectionFinished([], 1000));
    }

    [Fact]
    public void DirectionIsNotFinishedIfOneRequestWasFaster()
    {
        // The first slow request (e.g. TCP slow start) doesn't end the direction, the whole set decides
        Assert.False(SpeedTestCalculator.IsDirectionFinished([1200, 900, 1100], 1000));
    }

    [Fact]
    public void DirectionIsNotFinishedAtExactlyTheFinishDuration()
    {
        Assert.False(SpeedTestCalculator.IsDirectionFinished([1000, 1500], 1000));
    }

    [Fact]
    public void DirectionIsFinishedIfAllRequestsWereSlower()
    {
        Assert.True(SpeedTestCalculator.IsDirectionFinished([1200, 1100, 1300], 1000));
    }

    private static readonly DateTimeOffset Now = new(2026, 11, 26, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void RetryDelayWithoutHeaderIsFiveSeconds()
    {
        Assert.Equal(TimeSpan.FromSeconds(5), SpeedTestCalculator.GetRetryDelay(null, null, Now));
    }

    [Fact]
    public void RetryDelayIsTheDeltaOfTheHeader()
    {
        Assert.Equal(TimeSpan.FromSeconds(2), SpeedTestCalculator.GetRetryDelay(TimeSpan.FromSeconds(2), null, Now));
    }

    [Fact]
    public void RetryDelayIsTheTimeUntilTheDateOfTheHeader()
    {
        Assert.Equal(TimeSpan.FromSeconds(10),
            SpeedTestCalculator.GetRetryDelay(null, Now.AddSeconds(10), Now));
    }

    [Fact]
    public void RetryDelayOfADateInThePastIsZero()
    {
        Assert.Equal(TimeSpan.Zero, SpeedTestCalculator.GetRetryDelay(null, Now.AddSeconds(-10), Now));
    }

    [Fact]
    public void RetryDelayIsAtMostSixtySeconds()
    {
        Assert.Equal(TimeSpan.FromSeconds(60),
            SpeedTestCalculator.GetRetryDelay(TimeSpan.FromHours(1), null, Now));
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
    public void ServerTimingIsTheRequestDurationAfterOtherMetrics()
    {
        Assert.Equal(12.5,
            SpeedTestCalculator.ParseServerTiming(["cfL4;desc=\"?proto=TCP&rtt=11\";dur=5, cfRequestDuration;dur=12.5"]));
    }

    [Fact]
    public void ServerTimingAcceptsTheShortName()
    {
        Assert.Equal(3, SpeedTestCalculator.ParseServerTiming(["cfReqDur;dur=3"]));
    }

    [Fact]
    public void ServerTimingIsTheSumOfTheSpeedDurationsWithoutRequestDuration()
    {
        Assert.Equal(3.5, SpeedTestCalculator.ParseServerTiming(["cfSpeedA;dur=1.5, cfSpeedB;dur=2"])!.Value, 10);
    }

    [Fact]
    public void ServerTimingReadsAllHeaderValues()
    {
        Assert.Equal(7, SpeedTestCalculator.ParseServerTiming(["cfL4;dur=1", "cfRequestDuration;dur=7"]));
    }

    [Fact]
    public void ServerTimingOfOtherMetricsIsNull()
    {
        Assert.Null(SpeedTestCalculator.ParseServerTiming(["cfL4;dur=5, cfCacheStatus;dur=1"]));
    }

    [Fact]
    public void ServerTimingUpToTheMinimumIsIgnored()
    {
        Assert.Equal(2, SpeedTestCalculator.ParseServerTiming(["cfRequestDuration;dur=0.01, cfSpeedA;dur=2"]));
        Assert.Null(SpeedTestCalculator.ParseServerTiming(["cfSpeedA;dur=0.005"]));
    }

    [Fact]
    public void ServerTimingWithInvalidDurationIsNull()
    {
        Assert.Null(SpeedTestCalculator.ParseServerTiming(["cfRequestDuration;dur=."]));
    }
}
