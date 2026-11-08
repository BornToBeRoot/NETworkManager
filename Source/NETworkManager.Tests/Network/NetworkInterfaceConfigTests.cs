using NETworkManager.Models.Network;
using Xunit;

namespace NETworkManager.Tests.Network;

public class NetworkInterfaceConfigTests
{
    private static NetworkInterfaceConfig CreateStatic(string subnetmask = "255.255.255.0", bool enableStaticDNS = true,
        string primaryDNSServer = "1.1.1.1", string secondaryDNSServer = "1.0.0.1")
    {
        return NetworkInterfaceConfig.Create("Ethernet", true, "192.168.1.10", subnetmask, "192.168.1.1",
            enableStaticDNS, primaryDNSServer, secondaryDNSServer);
    }

    [Fact]
    public void Create_StaticValuesAreKept()
    {
        var config = CreateStatic();

        Assert.Equal("Ethernet", config.Name);
        Assert.True(config.EnableStaticIPAddress);
        Assert.Equal("192.168.1.10", config.IPAddress);
        Assert.Equal("255.255.255.0", config.Subnetmask);
        Assert.Equal("192.168.1.1", config.Gateway);
        Assert.True(config.EnableStaticDNS);
        Assert.Equal("1.1.1.1", config.PrimaryDNSServer);
        Assert.Equal("1.0.0.1", config.SecondaryDNSServer);
    }

    [Theory]
    [InlineData("/24", "255.255.255.0")]
    [InlineData("/16", "255.255.0.0")]
    [InlineData("/32", "255.255.255.255")]
    public void Create_CidrIsConvertedToSubnetmask(string cidr, string expected)
    {
        Assert.Equal(expected, CreateStatic(cidr).Subnetmask);
    }

    [Fact]
    public void Create_StaticDNSWithoutServers_FallsBackToDHCP()
    {
        var config = CreateStatic(primaryDNSServer: "", secondaryDNSServer: null);

        Assert.False(config.EnableStaticDNS);
    }

    [Fact]
    public void Create_OnlySecondaryDNSServer_BecomesPrimary()
    {
        var config = CreateStatic(primaryDNSServer: "", secondaryDNSServer: "8.8.8.8");

        Assert.True(config.EnableStaticDNS);
        Assert.Equal("8.8.8.8", config.PrimaryDNSServer);
        Assert.Equal(string.Empty, config.SecondaryDNSServer);
    }

    [Fact]
    public void Create_DHCP_IgnoresEmptyStaticValues()
    {
        var config = NetworkInterfaceConfig.Create("Ethernet", false, null, null, null, false, null, null);

        Assert.False(config.EnableStaticIPAddress);
        Assert.False(config.EnableStaticDNS);
    }

    [Fact]
    public void Create_ValuesAreTrimmed()
    {
        var config = NetworkInterfaceConfig.Create("Ethernet", true, " 192.168.1.10 ", " /24", "192.168.1.1 ", true,
            " 1.1.1.1", "1.0.0.1 ");

        Assert.Equal("192.168.1.10", config.IPAddress);
        Assert.Equal("255.255.255.0", config.Subnetmask);
        Assert.Equal("192.168.1.1", config.Gateway);
        Assert.Equal("1.1.1.1", config.PrimaryDNSServer);
        Assert.Equal("1.0.0.1", config.SecondaryDNSServer);
    }
}
