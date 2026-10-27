using NETworkManager.Utilities;
using Xunit;

namespace NETworkManager.Tests.Utilities;

public class DNSClientHelperTests
{
    [Theory]
    [InlineData("server.example.com.", "server.example.com")]
    [InlineData("server.example.com", "server.example.com")]
    [InlineData("server.example.com..", "server.example.com")]
    public void TrimsTrailingDotOfFullyQualifiedName(string name, string expected)
    {
        Assert.Equal(expected, DNSClientHelper.TrimTrailingDot(name));
    }

    [Theory]
    [InlineData("gateway.")]
    [InlineData("gateway")]
    public void KeepsSingleLabelName(string name)
    {
        Assert.Equal(name, DNSClientHelper.TrimTrailingDot(name));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void KeepsNullOrEmpty(string name)
    {
        Assert.Equal(name, DNSClientHelper.TrimTrailingDot(name));
    }

    [Theory]
    [InlineData("gateway.", "gateway")]
    [InlineData("server.example.com.", "server.example.com")]
    [InlineData("gateway", "gateway")]
    [InlineData(".", ".")]
    [InlineData("", "")]
    [InlineData(null, null)]
    public void SanitizesHostname(string hostname, string expected)
    {
        Assert.Equal(expected, DNSClientHelper.SanitizeHostname(hostname));
    }

    [Fact]
    public void TrimmedNameIsNotBareHostname()
    {
        Assert.False(DNSClientHelper.IsBareHostname(DNSClientHelper.TrimTrailingDot("gateway.")));
    }
}
