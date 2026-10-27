using NETworkManager.Models;
using Xunit;

namespace NETworkManager.Tests;

public class ApplicationManagerTests
{
    [Theory]
    [InlineData(ApplicationName.RemoteDesktop)]
    [InlineData(ApplicationName.PuTTY)]
    [InlineData(ApplicationName.TigerVNC)]
    [InlineData(ApplicationName.PowerShell)]
    [InlineData(ApplicationName.WebConsole)]
    public void HostForApplicationResolvedViaWindowsWithoutTrailingDot(ApplicationName name)
    {
        Assert.Equal("gateway", ApplicationManager.GetHostForApplication(name, "gateway."));
    }

    [Theory]
    [InlineData(ApplicationName.PingMonitor)]
    [InlineData(ApplicationName.Traceroute)]
    [InlineData(ApplicationName.DNSLookup)]
    [InlineData(ApplicationName.PortScanner)]
    public void HostForApplicationResolvedViaNETworkManagerKeepsTrailingDot(ApplicationName name)
    {
        Assert.Equal("gateway.", ApplicationManager.GetHostForApplication(name, "gateway."));
    }

    [Fact]
    public void HostForApplicationKeepsIPAddress()
    {
        Assert.Equal("10.0.0.1", ApplicationManager.GetHostForApplication(ApplicationName.RemoteDesktop, "10.0.0.1"));
    }
}
