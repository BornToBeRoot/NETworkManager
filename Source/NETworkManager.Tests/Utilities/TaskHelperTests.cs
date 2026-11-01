using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NETworkManager.Utilities;
using Xunit;

namespace NETworkManager.Tests.Utilities;

public class TaskHelperTests
{
    private const int Minimum = 300;

    [Fact]
    public async Task FastActionTakesTheMinimum()
    {
        var stopwatch = Stopwatch.StartNew();

        var result = await TaskHelper.WithMinimumDurationAsync(() => Task.FromResult(42), Minimum);

        Assert.Equal(42, result);
        Assert.True(stopwatch.ElapsedMilliseconds >= Minimum - 20, $"{stopwatch.ElapsedMilliseconds} ms");
    }

    [Fact]
    public async Task SlowActionIsNotDelayedFurther()
    {
        var stopwatch = Stopwatch.StartNew();

        await TaskHelper.WithMinimumDurationAsync(() => Task.Delay(Minimum * 2), Minimum);

        // Minimum and action run at the same time: ~600 ms, not ~900 ms
        Assert.True(stopwatch.ElapsedMilliseconds >= Minimum * 2 - 20, $"{stopwatch.ElapsedMilliseconds} ms");
        Assert.True(stopwatch.ElapsedMilliseconds < Minimum * 3 - 50, $"{stopwatch.ElapsedMilliseconds} ms");
    }

    [Fact]
    public async Task ExceptionIsThrownAfterTheMinimum()
    {
        var stopwatch = Stopwatch.StartNew();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            TaskHelper.WithMinimumDurationAsync(() => throw new InvalidOperationException(), Minimum));

        Assert.True(stopwatch.ElapsedMilliseconds >= Minimum - 20, $"{stopwatch.ElapsedMilliseconds} ms");
    }
}
