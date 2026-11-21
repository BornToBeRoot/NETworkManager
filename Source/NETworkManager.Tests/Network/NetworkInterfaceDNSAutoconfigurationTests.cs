using NETworkManager.Models.Network;
using Xunit;

namespace NETworkManager.Tests.Network;

public class NetworkInterfaceDNSAutoconfigurationTests
{
    [Theory]
    // DHCP, no static DNS servers: obtained automatically
    [InlineData(true, true, true)]
    // DHCP with static DNS servers
    [InlineData(false, true, false)]
    // Static IP address without DNS servers ("NameServer" is empty, but nothing is obtained automatically)
    [InlineData(true, false, false)]
    // Static IP address and static DNS servers
    [InlineData(false, false, false)]
    // Registry value not readable
    [InlineData(null, true, false)]
    public void IsDNSAutoconfigured(bool? noStaticDNSServers, bool dhcpEnabled, bool expected)
    {
        Assert.Equal(expected, NetworkInterface.IsDNSAutoconfigured(noStaticDNSServers, dhcpEnabled));
    }
}
