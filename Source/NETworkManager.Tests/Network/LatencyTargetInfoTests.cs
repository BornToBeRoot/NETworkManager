using NETworkManager.Models.Network;
using Xunit;

namespace NETworkManager.Tests.Network;

public class LatencyTargetInfoTests
{
    [Fact]
    public void SameNameHostAndGatewayIsTheSameTarget()
    {
        Assert.True(new LatencyTargetInfo("Cloudflare", "1.1.1.1")
            .IsSameTarget(new LatencyTargetInfo("Cloudflare", "1.1.1.1")));
    }

    [Theory]
    [InlineData("CF", "1.1.1.1", false)]
    [InlineData("Cloudflare", "1.0.0.1", false)]
    [InlineData("Cloudflare", "1.1.1.1", true)]
    public void AnotherValueIsAnotherTarget(string name, string host, bool isGateway)
    {
        Assert.False(new LatencyTargetInfo("Cloudflare", "1.1.1.1")
            .IsSameTarget(new LatencyTargetInfo(name, host, isGateway)));
    }

    [Fact]
    public void NoTargetIsNotTheSameTarget()
    {
        Assert.False(new LatencyTargetInfo("Gateway", null, true).IsSameTarget(null));
    }
}
