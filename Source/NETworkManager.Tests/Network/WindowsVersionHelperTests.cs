using System;
using NETworkManager.Models.Network;
using Xunit;

namespace NETworkManager.Tests.Network;

public class WindowsVersionHelperTests
{
    [Theory]
    [InlineData("10.0.26200", null, "Windows 11 25H2")]
    [InlineData("10.0.26100", null, "Windows 11 24H2 / Server 2025")]
    [InlineData("10.0.26100", true, "Windows Server 2025")]
    [InlineData("10.0.26100", false, "Windows 11 24H2")]
    [InlineData("10.0.22631", null, "Windows 11 23H2")]
    [InlineData("10.0.20348", false, "Windows Server 2022")]
    [InlineData("10.0.19045", null, "Windows 10 22H2")]
    [InlineData("10.0.17763", null, "Windows 10 1809 / Server 2019")]
    [InlineData("10.0.14393", true, "Windows Server 2016")]
    [InlineData("10.0.27000", null, "Windows 11 (build 27000)")]
    [InlineData("10.0.27000", true, "Windows (build 27000)")]
    [InlineData("10.0.12345", null, "Windows (build 12345)")]
    [InlineData("6.3.9600", null, "Windows 8.1 / Server 2012 R2")]
    [InlineData("6.1.7601", true, "Windows Server 2008 R2")]
    [InlineData("5.1.2600", null, "Windows XP")]
    public void GetName(string version, bool? isServer, string expected)
    {
        Assert.Equal(expected, WindowsVersionHelper.GetName(Version.Parse(version), isServer));
    }

    [Theory]
    [InlineData("6.1.0")] // Samba
    [InlineData("4.0.1381")] // Unknown major / minor
    public void GetName_Unknown(string version)
    {
        Assert.Null(WindowsVersionHelper.GetName(Version.Parse(version), null));
    }

    [Fact]
    public void GetName_Null()
    {
        Assert.Null(WindowsVersionHelper.GetName(null, null));
    }
}
