using System;
using NETworkManager.Models.Network;
using Xunit;

namespace NETworkManager.Tests.Network;

public class WebConsoleUrlHelperTests
{
    [Theory]
    [InlineData("server.example.com", 80, "http://server.example.com/")]
    [InlineData("server.example.com", 443, "https://server.example.com/")]
    [InlineData("10.0.0.1", 8080, "http://10.0.0.1:8080/")]
    [InlineData("10.0.0.1", 8443, "https://10.0.0.1:8443/")]
    [InlineData("2001:db8::1", 443, "https://[2001:db8::1]/")]
    [InlineData("fe80::1%12", 8080, "http://[fe80::1%2512]:8080/")]
    [InlineData("gateway.", 80, "http://gateway/")]
    [InlineData("gateway.", 8443, "https://gateway:8443/")]
    public void CreatesUrlForPort(string host, int port, string expected)
    {
        Assert.Equal(expected, WebConsoleUrlHelper.CreateUrl(host, port));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(".")]
    public void ThrowsWithoutHost(string host)
    {
        Assert.ThrowsAny<ArgumentException>(() => WebConsoleUrlHelper.CreateUrl(host, 80));
    }

    [Theory]
    [InlineData(new[] { 22, 80, 443 }, "https://10.0.0.1/")]
    [InlineData(new[] { 80, 8443 }, "http://10.0.0.1/")]
    [InlineData(new[] { 22, 8443, 8080 }, "https://10.0.0.1:8443/")]
    [InlineData(new[] { 8080 }, "http://10.0.0.1:8080/")]
    [InlineData(new[] { 22, 3389 }, "http://10.0.0.1/")]
    [InlineData(new int[0], "http://10.0.0.1/")]
    public void PrefersHttpsAndDefaultPorts(int[] openPorts, string expected)
    {
        Assert.Equal(expected, WebConsoleUrlHelper.CreateUrl("10.0.0.1", openPorts));
    }

    [Theory]
    [InlineData(80, true)]
    [InlineData(443, true)]
    [InlineData(8080, true)]
    [InlineData(8443, true)]
    [InlineData(22, false)]
    public void DetectsWebPorts(int port, bool expected)
    {
        Assert.Equal(expected, WebConsoleUrlHelper.IsWebPort(port));
    }
}
