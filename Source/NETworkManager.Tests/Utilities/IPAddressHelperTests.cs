using NETworkManager.Utilities;
using Xunit;

namespace NETworkManager.Tests.Utilities;

public class IPAddressHelperTests
{
    [Theory]
    [InlineData("10.0.0.1", true)]
    [InlineData(" 10.0.0.1 ", true)]
    [InlineData("2001:db8::1", true)]
    [InlineData("::1", true)]
    [InlineData("10", false)]
    [InlineData("127.1", false)]
    [InlineData("server-01.borntoberoot.net", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsIPAddress(string host, bool expected)
    {
        Assert.Equal(expected, IPAddressHelper.IsIPAddress(host));
    }

    [Theory]
    [InlineData("10.0.0.1", true)]
    [InlineData("10.0.0.1; 2001:db8::1", true)]
    [InlineData("10.0.0.1; 10.0.0.2;", true)]
    [InlineData("10.0.0.1;; 10.0.0.2", true)]
    [InlineData("10.0.0.1; example.com", false)]
    [InlineData(";", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void AreIPAddresses(string hosts, bool expected)
    {
        Assert.Equal(expected, IPAddressHelper.AreIPAddresses(hosts));
    }
}
