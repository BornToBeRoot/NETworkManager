using System;
using NETworkManager.Utilities;
using Xunit;

namespace NETworkManager.Tests.Utilities;

public class DateTimeHelperTests
{
    [Theory]
    [InlineData(0, "00:00")]
    [InlineData(5, "00:05")]
    [InlineData(59, "00:59")]
    [InlineData(272, "04:32")]
    [InlineData(3599, "59:59")]
    [InlineData(3600, "1:00:00")]
    [InlineData(3725, "1:02:05")]
    [InlineData(90000, "25:00:00")]
    [InlineData(-3, "00:00")]
    public void ConvertsTimeSpanToCountdown(int seconds, string expected)
    {
        Assert.Equal(expected, DateTimeHelper.TimeSpanToCountdownString(TimeSpan.FromSeconds(seconds)));
    }
}
