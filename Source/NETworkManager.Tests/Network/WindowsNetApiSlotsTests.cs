using System;
using System.Threading;
using System.Threading.Tasks;
using NETworkManager.Models.Network;
using Xunit;

namespace NETworkManager.Tests.Network;

/// <summary>
///     Slots of the blocking NetApi requests, shared by all scans: the max. number is passed with every request.
/// </summary>
public class WindowsNetApiSlotsTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan ShortTimeout = TimeSpan.FromMilliseconds(100);

    [Fact]
    public async Task WaitAsync_TakesSlotsUpToTheLimit()
    {
        var slots = new WindowsNetApiSlots();

        Assert.True(await slots.WaitAsync(2, Timeout, CancellationToken.None));
        Assert.True(await slots.WaitAsync(2, Timeout, CancellationToken.None));
        Assert.False(await slots.WaitAsync(2, ShortTimeout, CancellationToken.None));
        Assert.Equal(2, slots.Used);
    }

    [Fact]
    public async Task WaitAsync_StartsWhenASlotIsReleased()
    {
        var slots = new WindowsNetApiSlots();

        Assert.True(await slots.WaitAsync(1, Timeout, CancellationToken.None));

        var waiting = slots.WaitAsync(1, Timeout, CancellationToken.None);

        Assert.False(waiting.IsCompleted);

        slots.Release();

        Assert.True(await waiting);
        Assert.Equal(1, slots.Used);
    }

    [Fact]
    public async Task WaitAsync_HigherLimitAppliesWhileSlotsAreUsed()
    {
        var slots = new WindowsNetApiSlots();

        Assert.True(await slots.WaitAsync(1, Timeout, CancellationToken.None));

        // E.g. the setting was raised: calls of the earlier scan still count
        Assert.True(await slots.WaitAsync(2, Timeout, CancellationToken.None));
        Assert.False(await slots.WaitAsync(2, ShortTimeout, CancellationToken.None));
    }

    [Fact]
    public async Task WaitAsync_LowerLimitWaitsUntilEnoughSlotsAreReleased()
    {
        var slots = new WindowsNetApiSlots();

        for (var i = 0; i < 3; i++)
            Assert.True(await slots.WaitAsync(3, Timeout, CancellationToken.None));

        // E.g. the setting was lowered: no new call until less than 1 slot is used
        var waiting = slots.WaitAsync(1, Timeout, CancellationToken.None);

        slots.Release();
        slots.Release();

        await Task.Delay(ShortTimeout);
        Assert.False(waiting.IsCompleted);

        slots.Release();

        Assert.True(await waiting);
        Assert.Equal(1, slots.Used);
    }

    [Fact]
    public async Task WaitAsync_Canceled_Throws()
    {
        var slots = new WindowsNetApiSlots();

        Assert.True(await slots.WaitAsync(1, Timeout, CancellationToken.None));

        using var cancellationTokenSource = new CancellationTokenSource(ShortTimeout);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            slots.WaitAsync(1, Timeout, cancellationTokenSource.Token));
        Assert.Equal(1, slots.Used);
    }

    [Fact]
    public void Release_WithoutSlot_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => new WindowsNetApiSlots().Release());
    }
}
