using System.Net;
using System.Net.Sockets;
using NETworkManager.Models.Network;
using Xunit;

namespace NETworkManager.Tests.Network;

public class NetworkConnectionCheckTests
{
    private static IPAddress Ip(string ipAddress) => ipAddress == null ? null : IPAddress.Parse(ipAddress);

    [Theory]
    [InlineData("192.168.1.10", true, ConnectionState.OK, ConnectionCheckReason.None)]
    [InlineData(null, false, ConnectionState.Critical, ConnectionCheckReason.NoIPv4Address)]
    // DHCP failed (#3288: the address is still shown)
    [InlineData("169.254.10.20", false, ConnectionState.Warning, ConnectionCheckReason.IPv4LinkLocal)]
    [InlineData("169.254.10.20", true, ConnectionState.Warning, ConnectionCheckReason.IPv4LinkLocal)]
    // No route: address of the first active network adapter (fallback)
    [InlineData("192.168.1.10", false, ConnectionState.Warning, ConnectionCheckReason.NoRoute)]
    public void GetLocalIPAddressResult_IPv4(string ipAddress, bool isRouted, ConnectionState state,
        ConnectionCheckReason reason)
    {
        Assert.Equal(new ConnectionCheckResult(state, reason),
            NetworkConnectionCheck.GetLocalIPAddressResult(Ip(ipAddress), AddressFamily.InterNetwork, isRouted));
    }

    [Theory]
    [InlineData("2001:db8::10", true, ConnectionState.OK, ConnectionCheckReason.None)]
    [InlineData("fd00::10", true, ConnectionState.OK, ConnectionCheckReason.None)]
    // IPv4-only network: not an error
    [InlineData(null, false, ConnectionState.Info, ConnectionCheckReason.IPv6NotAvailable)]
    [InlineData("fe80::10", false, ConnectionState.Info, ConnectionCheckReason.IPv6NotAvailable)]
    // Unique local address without a route to the internet (e.g. some routers)
    [InlineData("fd00::10", false, ConnectionState.Info, ConnectionCheckReason.IPv6NotAvailable)]
    public void GetLocalIPAddressResult_IPv6(string ipAddress, bool isRouted, ConnectionState state,
        ConnectionCheckReason reason)
    {
        Assert.Equal(new ConnectionCheckResult(state, reason),
            NetworkConnectionCheck.GetLocalIPAddressResult(Ip(ipAddress), AddressFamily.InterNetworkV6, isRouted));
    }
}
