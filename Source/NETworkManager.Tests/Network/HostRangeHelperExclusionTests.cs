using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NETworkManager.Models.Network;
using Xunit;

namespace NETworkManager.Tests.Network;

public class HostRangeHelperExclusionTests
{
    private static async Task<string[]> ResolveAsync(string input)
    {
        var (hosts, _, _) = await HostRangeHelper.ResolveAsync(HostRangeHelper.CreateListFromInput(input), true, false,
            CancellationToken.None);

        return [.. hosts.Select(x => x.ipAddress.ToString())];
    }

    [Theory]
    [InlineData("10.0.0.0/29; !10.0.0.1", "10.0.0.0,10.0.0.2,10.0.0.3,10.0.0.4,10.0.0.5,10.0.0.6,10.0.0.7")]
    [InlineData("10.0.0.0/29; !10.0.0.0/30", "10.0.0.4,10.0.0.5,10.0.0.6,10.0.0.7")]
    [InlineData("10.0.0.0/29; !10.0.0.0/255.255.255.252", "10.0.0.4,10.0.0.5,10.0.0.6,10.0.0.7")]
    [InlineData("10.0.0.0/29; !10.0.0.2-5", "10.0.0.0,10.0.0.1,10.0.0.6,10.0.0.7")]
    [InlineData("10.0.0.0/29; !10.0.0.2 - 10.0.0.5", "10.0.0.0,10.0.0.1,10.0.0.6,10.0.0.7")]
    [InlineData("10.0.0.0/29; !10.0.0.[0,7]", "10.0.0.1,10.0.0.2,10.0.0.3,10.0.0.4,10.0.0.5,10.0.0.6")]
    [InlineData("!10.0.0.1; 10.0.0.1-3", "10.0.0.2,10.0.0.3")]
    [InlineData("10.0.0.1-3; !10.0.0.0/8", "")]
    [InlineData("10.0.0.1-3; !192.168.0.1", "10.0.0.1,10.0.0.2,10.0.0.3")]
    [InlineData("2001:db8::1; 2001:db8::2; !2001:db8::1", "2001:db8::2")]
    public async Task ResolveAsync_Exclusions(string input, string expected)
    {
        Assert.Equal(expected, string.Join(",", await ResolveAsync(input)));
    }

    [Fact]
    public async Task ResolveAsync_OnlyExclusions_NoHosts()
    {
        Assert.Empty(await ResolveAsync("!10.0.0.1"));
    }

    [Theory]
    [InlineData("10.0.0.1; !10.0.0.1", 1)]
    [InlineData("10.0.0.0/30; !10.0.0.1-2", 2)]
    [InlineData("10.0.0.1-3; !10.0.0.0/8", 3)]
    [InlineData("10.0.0.1-3; !192.168.0.1", 0)]
    [InlineData("10.0.0.1-3", 0)]
    public async Task ResolveAsync_ExcludedCount(string input, int expected)
    {
        var (_, _, excludedCount) = await HostRangeHelper.ResolveAsync(HostRangeHelper.CreateListFromInput(input),
            true, false, CancellationToken.None);

        Assert.Equal(expected, excludedCount);
    }

    [Theory]
    [InlineData("10.0.0.0/24; !10.0.0.1", 256)]
    [InlineData("10.0.0.0/24; !10.0.0.0/8", 256)]
    [InlineData("!10.0.0.0/8", 0)]
    public void GetHostCount_IgnoresExclusions(string input, long expected)
    {
        Assert.Equal(expected, HostRangeHelper.GetHostCount(HostRangeHelper.CreateListFromInput(input)));
    }

    [Theory]
    [InlineData("!10.0.0.1", true)]
    [InlineData("10.0.0.1", false)]
    public void IsExclusion(string input, bool expected)
    {
        Assert.Equal(expected, HostRangeHelper.IsExclusion(input));
    }
}
