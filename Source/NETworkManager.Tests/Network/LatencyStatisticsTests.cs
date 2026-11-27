using System;
using System.Collections.Generic;
using NETworkManager.Models.Network;
using Xunit;

namespace NETworkManager.Tests.Network;

public class LatencyStatisticsTests
{
    private static readonly DateTime Start = new(2026, 11, 28, 12, 0, 0);

    private static LatencySample Reply(int second, long time) => new(Start.AddSeconds(second), true, time);

    private static LatencySample Lost(int second) => new(Start.AddSeconds(second), false, 0);

    [Fact]
    public void StatisticsOfNoPingsAreEmpty()
    {
        var info = LatencyStatistics.Calculate([]);

        Assert.Equal(0, info.Count);
        Assert.Null(info.Minimum);
        Assert.Null(info.Average);
        Assert.Null(info.Maximum);
        Assert.Equal(0, info.PacketLoss);
    }

    [Fact]
    public void StatisticsUseOnlyTheReplies()
    {
        var info = LatencyStatistics.Calculate([Reply(0, 10), Lost(2), Reply(4, 20), Reply(6, 30)]);

        Assert.Equal(4, info.Count);
        Assert.Equal(1, info.Lost);
        Assert.Equal(10, info.Minimum);
        Assert.Equal(20, info.Average);
        Assert.Equal(30, info.Maximum);
        Assert.Equal(25, info.PacketLoss);
    }

    [Fact]
    public void StatisticsWithoutAReplyAreAllLost()
    {
        var info = LatencyStatistics.Calculate([Lost(0), Lost(2)]);

        Assert.Equal(2, info.Lost);
        Assert.Null(info.Average);
        Assert.Equal(100, info.PacketLoss);
    }

    [Fact]
    public void StateWithoutPingsIsNone()
    {
        Assert.Equal(LatencyState.None, LatencyStatistics.GetState([]));
    }

    [Fact]
    public void StateWithOnlyRepliesIsOk()
    {
        Assert.Equal(LatencyState.Ok, LatencyStatistics.GetState([Reply(0, 9), Reply(2, 8)]));
    }

    [Fact]
    public void StateWithALostPingInTheWindowIsLost()
    {
        Assert.Equal(LatencyState.Lost, LatencyStatistics.GetState([Reply(0, 9), Lost(2), Reply(4, 8)]));
    }

    [Fact]
    public void StateDependsOnTheLastPing()
    {
        Assert.Equal(LatencyState.Failed, LatencyStatistics.GetState([Reply(0, 9), Reply(2, 8), Lost(4)]));
    }

    [Fact]
    public void RemoveOutdatedKeepsTheTimeWindow()
    {
        List<LatencySample> samples = [Reply(0, 1), Reply(30, 2), Reply(59, 3), Reply(61, 4), Reply(90, 5)];

        LatencyStatistics.RemoveOutdated(samples, Start.AddSeconds(120), TimeSpan.FromSeconds(60));

        Assert.Equal([4L, 5L], samples.ConvertAll(x => x.Time));
    }

    [Fact]
    public void RemoveOutdatedKeepsAllRecentPings()
    {
        List<LatencySample> samples = [Reply(0, 1), Reply(2, 2)];

        LatencyStatistics.RemoveOutdated(samples, Start.AddSeconds(10), TimeSpan.FromSeconds(60));

        Assert.Equal(2, samples.Count);
    }
}
