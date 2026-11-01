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

    [Theory]
    [InlineData(0, "+0.0 s")]
    [InlineData(1.234, "+1.2 s")]
    [InlineData(-0.04, "-0.0 s")]
    [InlineData(59.9, "+59.9 s")]
    [InlineData(59.97, "+1 min 0 s")]
    [InlineData(-185, "-3 min 5 s")]
    [InlineData(7380, "+2 h 3 min")]
    [InlineData(-100800, "-1 d 4 h")]
    public void ConvertsTimeSpanToOffset(double seconds, string expected)
    {
        Assert.Equal(expected,
            DateTimeHelper.TimeSpanToOffsetString(TimeSpan.FromSeconds(seconds),
                System.Globalization.CultureInfo.InvariantCulture));
    }
}
