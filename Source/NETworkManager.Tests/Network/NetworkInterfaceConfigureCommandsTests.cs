using NETworkManager.Models.Network;
using Xunit;

namespace NETworkManager.Tests.Network;

public class NetworkInterfaceConfigureCommandsTests
{
    [Fact]
    public void StaticIPAndDNS()
    {
        var config = NetworkInterfaceConfig.Create("Ethernet", true, "192.168.1.10", "/24", "192.168.1.1", true,
            "1.1.1.1", "1.0.0.1");

        Assert.Equal([
            "& netsh interface ipv4 set address name='Ethernet' source=static address=192.168.1.10 mask=255.255.255.0 gateway=192.168.1.1",
            "& netsh interface ipv4 set dnsservers name='Ethernet' source=static address=1.1.1.1 register=primary validate=no",
            "& netsh interface ipv4 add dnsservers name='Ethernet' address=1.0.0.1 index=2 validate=no"
        ], NetworkInterface.GetConfigureCommands(config, true, true));
    }

    [Fact]
    public void StaticIPWithoutGateway_UsesGatewayNone()
    {
        var config = NetworkInterfaceConfig.Create("Ethernet", true, "10.0.0.5", "255.0.0.0", "", true, "10.0.0.1",
            "");

        Assert.Equal(
            "& netsh interface ipv4 set address name='Ethernet' source=static address=10.0.0.5 mask=255.0.0.0 gateway=none",
            NetworkInterface.GetConfigureCommands(config, false, false)[0]);
    }

    [Fact]
    public void DHCP_SkippedIfAlreadyEnabled()
    {
        var config = NetworkInterfaceConfig.Create("Ethernet", false, null, null, null, false, null, null);

        Assert.Empty(NetworkInterface.GetConfigureCommands(config, true, true));
        Assert.Equal([
            "& netsh interface ipv4 set address name='Ethernet' source=dhcp",
            "& netsh interface ipv4 set dnsservers name='Ethernet' source=dhcp"
        ], NetworkInterface.GetConfigureCommands(config, null, false));
    }

    [Fact]
    public void NameIsEscaped()
    {
        var config = NetworkInterfaceConfig.Create("John's LAN", false, null, null, null, false, null, null);

        Assert.Equal("& netsh interface ipv4 set address name='John''s LAN' source=dhcp",
            NetworkInterface.GetConfigureCommands(config, false, true)[0]);
    }
}
