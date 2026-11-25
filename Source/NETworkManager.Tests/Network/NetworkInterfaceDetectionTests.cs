using System.Net;
using System.Net.Sockets;
using NETworkManager.Models.Network;
using Xunit;

namespace NETworkManager.Tests.Network;

public class NetworkInterfaceDetectionTests
{
    private static IPAddress[] Addresses(params string[] addresses) =>
        [.. System.Linq.Enumerable.Select(addresses, IPAddress.Parse)];

    [Fact]
    public void SelectLocalIPAddress_IPv4_PrefersNonLinkLocal()
    {
        // e.g. a disconnected adapter without DHCP (169.254.x.x) before the connected one
        var result = NetworkInterface.SelectLocalIPAddress(
            [Addresses("169.254.10.20", "fe80::1"), Addresses("192.168.1.10")], AddressFamily.InterNetwork);

        Assert.Equal(IPAddress.Parse("192.168.1.10"), result);
    }

    [Fact]
    public void SelectLocalIPAddress_IPv4_OnlyLinkLocal()
    {
        var result = NetworkInterface.SelectLocalIPAddress([Addresses("169.254.10.20")], AddressFamily.InterNetwork);

        Assert.Equal(IPAddress.Parse("169.254.10.20"), result);
    }

    [Fact]
    public void SelectLocalIPAddress_IPv6_PrefersGlobalOrUniqueLocal()
    {
        var result = NetworkInterface.SelectLocalIPAddress(
            [Addresses("192.168.1.10", "fe80::1"), Addresses("fe80::2", "fd00::10")], AddressFamily.InterNetworkV6);

        Assert.Equal(IPAddress.Parse("fd00::10"), result);
    }

    [Fact]
    public void SelectLocalIPAddress_IPv6_FallbackLinkLocal()
    {
        var result = NetworkInterface.SelectLocalIPAddress([Addresses("192.168.1.10", "fe80::1")],
            AddressFamily.InterNetworkV6);

        Assert.Equal(IPAddress.Parse("fe80::1"), result);
    }

    [Fact]
    public void SelectLocalIPAddress_NoAddressOfFamily()
    {
        Assert.Null(NetworkInterface.SelectLocalIPAddress([Addresses("fe80::1")], AddressFamily.InterNetwork));
        Assert.Null(NetworkInterface.SelectLocalIPAddress([], AddressFamily.InterNetworkV6));
    }

    [Theory]
    [InlineData("169.254.0.1", true)]
    [InlineData("169.253.0.1", false)]
    [InlineData("192.168.1.1", false)]
    [InlineData("fe80::1", true)]
    [InlineData("2001:db8::1", false)]
    [InlineData("fd00::1", false)]
    public void IsLinkLocal(string ipAddress, bool expected)
    {
        Assert.Equal(expected, NetworkInterface.IsLinkLocal(IPAddress.Parse(ipAddress)));
    }

    [Fact]
    public void SelectGateway_GatewayOfTheInterfaceWithTheLocalAddress()
    {
        var result = NetworkInterface.SelectGateway(
        [
            (Addresses("10.0.0.5"), Addresses("10.0.0.1")),
            (Addresses("192.168.1.10", "fe80::10"), Addresses("fe80::1", "192.168.1.1"))
        ], IPAddress.Parse("192.168.1.10"));

        Assert.Equal(IPAddress.Parse("192.168.1.1"), result);
    }

    [Fact]
    public void SelectGateway_IPv6()
    {
        var result = NetworkInterface.SelectGateway(
            [(Addresses("192.168.1.10", "2001:db8::10"), Addresses("192.168.1.1", "fe80::1"))],
            IPAddress.Parse("2001:db8::10"));

        Assert.Equal(IPAddress.Parse("fe80::1"), result);
    }

    [Fact]
    public void SelectGateway_IgnoresScopeId()
    {
        var local = IPAddress.Parse("fe80::10");
        local.ScopeId = 12;

        var result = NetworkInterface.SelectGateway([(Addresses("fe80::10"), Addresses("fe80::1"))], local);

        Assert.Equal(IPAddress.Parse("fe80::1"), result);
    }

    [Fact]
    public void SelectGateway_SkipsUnspecifiedGateway()
    {
        // e.g. a VPN adapter (on-link default route)
        var result = NetworkInterface.SelectGateway([(Addresses("10.8.0.2"), Addresses("0.0.0.0"))],
            IPAddress.Parse("10.8.0.2"));

        Assert.Null(result);
    }

    [Fact]
    public void SelectGateway_LocalAddressNotFound()
    {
        var result = NetworkInterface.SelectGateway([(Addresses("192.168.1.10"), Addresses("192.168.1.1"))],
            IPAddress.Parse("10.0.0.5"));

        Assert.Null(result);
    }
}
