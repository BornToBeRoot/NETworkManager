using System.Net;
using System.Net.Sockets;
using NETworkManager.Models.Network;
using NETworkManager.Utilities;
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

    private static DNSClientResultString Resolved(string hostname) => new(hostname, "192.168.1.1:53");

    private static DNSClientResultString NotFound() => new(true, "Non-Existent Domain", "192.168.1.1:53")
        { IsNotFound = true };

    private static DNSClientResultString Error() => new(true, "Timeout");

    [Fact]
    public void ShouldResolveIPv6()
    {
        Assert.True(NetworkConnectionCheck.ShouldResolveIPv6(null));
        Assert.True(NetworkConnectionCheck.ShouldResolveIPv6(NotFound()));
        Assert.False(NetworkConnectionCheck.ShouldResolveIPv6(Resolved("pc.example.com")));
        // The same DNS server would time out again
        Assert.False(NetworkConnectionCheck.ShouldResolveIPv6(Error()));
    }

    [Fact]
    public void GetDNSResult_ResolvedIPv4()
    {
        var (result, hostname) = NetworkConnectionCheck.GetDNSResult(Resolved("pc.example.com"), null);

        Assert.Equal(new ConnectionCheckResult(ConnectionState.OK), result);
        Assert.Equal("pc.example.com", hostname);
    }

    [Fact]
    public void GetDNSResult_ResolvedIPv6AfterIPv4NotFound()
    {
        var (result, hostname) = NetworkConnectionCheck.GetDNSResult(NotFound(), Resolved("pc6.example.com"));

        Assert.Equal(ConnectionState.OK, result.State);
        Assert.Equal("pc6.example.com", hostname);
    }

    [Fact]
    public void GetDNSResult_NotFoundWinsOverError()
    {
        // e.g. IPv4 has no PTR while IPv6 times out: no record, not a broken DNS setup
        var (result, hostname) = NetworkConnectionCheck.GetDNSResult(NotFound(), Error());

        Assert.Equal(new ConnectionCheckResult(ConnectionState.Info, ConnectionCheckReason.PTRNotFound), result);
        Assert.Null(hostname);
    }

    [Fact]
    public void GetDNSResult_Error()
    {
        Assert.Equal(new ConnectionCheckResult(ConnectionState.Critical, ConnectionCheckReason.DNSError),
            NetworkConnectionCheck.GetDNSResult(Error(), null).Result);
    }

    [Fact]
    public void GetDNSResult_NothingToResolve()
    {
        Assert.Equal(new ConnectionCheckResult(ConnectionState.Info, ConnectionCheckReason.NoIPAddressToResolve),
            NetworkConnectionCheck.GetDNSResult(null, null).Result);
    }

    [Theory]
    [InlineData("192.168.1.1", true, true, ConnectionState.OK, ConnectionCheckReason.None)]
    // Some routers block ICMP
    [InlineData("192.168.1.1", true, false, ConnectionState.Warning, ConnectionCheckReason.GatewayNoReply)]
    // Routed via an adapter without a gateway (e.g. VPN)
    [InlineData(null, true, false, ConnectionState.Warning, ConnectionCheckReason.NoGateway)]
    // No route and no gateway (or no local IP address)
    [InlineData(null, false, false, ConnectionState.Critical, ConnectionCheckReason.NoGateway)]
    public void GetGatewayResult(string gateway, bool isRouted, bool isReachable, ConnectionState state,
        ConnectionCheckReason reason)
    {
        Assert.Equal(new ConnectionCheckResult(state, reason),
            NetworkConnectionCheck.GetGatewayResult(Ip(gateway), isRouted, isReachable));
    }
}
