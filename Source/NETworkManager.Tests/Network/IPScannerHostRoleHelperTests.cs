using System;
using System.Net;
using NETworkManager.Models.Network;
using Xunit;

namespace NETworkManager.Tests.Network;

public class IPScannerHostRoleHelperTests
{
    private static NetworkInterfaceInfo CreateInterface(bool isOperational = true)
    {
        return new NetworkInterfaceInfo
        {
            IsOperational = isOperational,
            IPv4Address =
                [new Tuple<IPAddress, IPAddress>(IPAddress.Parse("192.168.1.23"), IPAddress.Parse("255.255.255.0"))],
            IPv6Address = [IPAddress.Parse("2001:db8::23")],
            IPv6AddressLinkLocal = [IPAddress.Parse("fe80::23%12")],
            IPv4Gateway = [IPAddress.Parse("192.168.1.1")],
            IPv6Gateway = [IPAddress.Parse("fe80::1%12")],
            DNSServer = [IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.2")],
            DhcpServer = [IPAddress.Parse("192.168.1.1")]
        };
    }

    [Theory]
    [InlineData("192.168.1.23", IPScannerHostRole.LocalComputer)]
    [InlineData("2001:db8::23", IPScannerHostRole.LocalComputer)]
    [InlineData("fe80::23", IPScannerHostRole.LocalComputer)]
    [InlineData("fe80::23%12", IPScannerHostRole.LocalComputer)]
    [InlineData("192.168.1.1",
        IPScannerHostRole.DefaultGateway | IPScannerHostRole.DNSServer | IPScannerHostRole.DHCPServer)]
    [InlineData("fe80::1%12", IPScannerHostRole.DefaultGateway)]
    [InlineData("fe80::1", IPScannerHostRole.DefaultGateway)]
    // Another link: the same link-local address is another host
    [InlineData("fe80::1%3", IPScannerHostRole.None)]
    [InlineData("fe80::23%7", IPScannerHostRole.None)]
    [InlineData("192.168.1.2", IPScannerHostRole.DNSServer)]
    [InlineData("192.168.1.50", IPScannerHostRole.None)]
    public void GetRole(string ipAddress, IPScannerHostRole expected)
    {
        var roles = IPScannerHostRoleHelper.GetRoles([CreateInterface()]);

        Assert.Equal(expected, IPScannerHostRoleHelper.GetRole(roles, IPAddress.Parse(ipAddress)));
    }

    [Fact]
    public void GetRole_SameLinkLocalAddressOnTwoInterfaces()
    {
        // fe80::1 is the gateway of interface 12 and a DNS server on interface 7 (another router)
        var roles = IPScannerHostRoleHelper.GetRoles(
        [
            CreateInterface(),
            new NetworkInterfaceInfo
            {
                IsOperational = true,
                DNSServer = [IPAddress.Parse("fe80::1%7"), IPAddress.Parse("fe80::53")]
            }
        ]);

        Assert.Equal(IPScannerHostRole.DefaultGateway, IPScannerHostRoleHelper.GetRole(roles, IPAddress.Parse("fe80::1%12")));
        Assert.Equal(IPScannerHostRole.DNSServer, IPScannerHostRoleHelper.GetRole(roles, IPAddress.Parse("fe80::1%7")));

        // Interface not known: both
        Assert.Equal(IPScannerHostRole.DefaultGateway | IPScannerHostRole.DNSServer,
            IPScannerHostRoleHelper.GetRole(roles, IPAddress.Parse("fe80::1")));

        // Configured without scope id: on every interface
        Assert.Equal(IPScannerHostRole.DNSServer, IPScannerHostRoleHelper.GetRole(roles, IPAddress.Parse("fe80::53%12")));
    }

    [Fact]
    public void GetRoles_NotOperational_Skipped()
    {
        Assert.Empty(IPScannerHostRoleHelper.GetRoles([CreateInterface(false)]));
    }

    [Theory]
    [InlineData(IPScannerHostRole.None, IPScannerHostRole.None)]
    [InlineData(IPScannerHostRole.DHCPServer | IPScannerHostRole.DNSServer, IPScannerHostRole.DNSServer)]
    [InlineData(IPScannerHostRole.DNSServer | IPScannerHostRole.DefaultGateway, IPScannerHostRole.DefaultGateway)]
    [InlineData(IPScannerHostRole.LocalComputer | IPScannerHostRole.DNSServer, IPScannerHostRole.LocalComputer)]
    public void GetPrimaryRole(IPScannerHostRole role, IPScannerHostRole expected)
    {
        Assert.Equal(expected, IPScannerHostRoleHelper.GetPrimaryRole(role));
    }
}
