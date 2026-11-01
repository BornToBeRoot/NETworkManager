using System;
using NETworkManager.Utilities;
using Xunit;

namespace NETworkManager.Tests.Utilities;

public class AutoRefreshTimeTests
{
    [Theory]
    [InlineData(5, TimeUnit.Second, 5)]
    [InlineData(30, TimeUnit.Second, 30)]
    [InlineData(1, TimeUnit.Minute, 60)]
    [InlineData(5, TimeUnit.Minute, 300)]
    [InlineData(1, TimeUnit.Hour, 3600)]
    [InlineData(2, TimeUnit.Hour, 7200)]
    public void CalculatesTimeSpan(int value, TimeUnit timeUnit, int expectedSeconds)
    {
        Assert.Equal(TimeSpan.FromSeconds(expectedSeconds),
            AutoRefreshTime.CalculateTimeSpan(new AutoRefreshTimeInfo(value, timeUnit)));
    }

    [Fact]
    public void ThrowsForNoTimeUnit()
    {
        Assert.Throws<Exception>(() => AutoRefreshTime.CalculateTimeSpan(new AutoRefreshTimeInfo(1, TimeUnit.None)));
    }
}
