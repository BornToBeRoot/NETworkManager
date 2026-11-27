using NETworkManager.Models.Network;
using Xunit;

namespace NETworkManager.Tests.Network;

public class LatencyTargetInfoTests
{
    [Fact]
    public void SameNameAndHostIsTheSameTarget()
    {
        Assert.True(new LatencyTargetInfo("Cloudflare", "1.1.1.1")
            .IsSameTarget(new LatencyTargetInfo("Cloudflare", "1.1.1.1")));
    }

    [Theory]
    [InlineData("CF", "1.1.1.1")]
    [InlineData("Cloudflare", "1.0.0.1")]
    public void AnotherValueIsAnotherTarget(string name, string host)
    {
        Assert.False(new LatencyTargetInfo("Cloudflare", "1.1.1.1")
            .IsSameTarget(new LatencyTargetInfo(name, host)));
    }

    [Fact]
    public void NoTargetIsNotTheSameTarget()
    {
        Assert.False(new LatencyTargetInfo("Cloudflare", "1.1.1.1").IsSameTarget(null));
    }
}
